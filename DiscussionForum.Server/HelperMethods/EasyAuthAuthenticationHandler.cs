using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace DiscussionForum.Server.HelperMethods;

public class EasyAuthAuthenticationHandler : AuthenticationHandler<EasyAuthAuthenticationOptions>
{
    public const string EasyAuthPrincipalIDP = "X-MS-CLIENT-PRINCIPAL-IDP";
    public const string EasyAuthPrincipalName = "X-MS-CLIENT-PRINCIPAL-NAME";
    public const string EasyAuthPrincipalID = "X-MS-CLIENT-PRINCIPAL-ID";
    private readonly ILogger<EasyAuthAuthenticationHandler> _logger;
    private readonly IMediator _mediator;
    private readonly IDistributedCache _cache;
    private readonly DistributedCacheEntryOptions _cacheOptions = new() { SlidingExpiration = TimeSpan.FromMinutes(1) };

    public EasyAuthAuthenticationHandler(IMediator mediator, IDistributedCache cache, IOptionsMonitor<EasyAuthAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
    {
        _logger = logger.CreateLogger<EasyAuthAuthenticationHandler>();
        _mediator = mediator;
        _cache = cache;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            string? easyAuthProvider = Context.Request.Headers[EasyAuthPrincipalIDP].FirstOrDefault();
            string? msClientPrincipalName = Context.Request.Headers[EasyAuthPrincipalName].FirstOrDefault();
            string? msClientPrincipalId = Context.Request.Headers[EasyAuthPrincipalID].FirstOrDefault();
            if (string.IsNullOrEmpty(easyAuthProvider) || string.IsNullOrEmpty(msClientPrincipalId) || string.IsNullOrEmpty(msClientPrincipalName))
            {
                return AuthenticateResult.NoResult();
            }
            ClaimsPrincipal principal = new();
            List<Claim> claims = new()
            {
                new Claim(ClaimConstants.EmailNameClaimName, msClientPrincipalName),
                new Claim(ClaimConstants.IdClaimName, msClientPrincipalId)
            };
            Guid userId = Guid.Parse(msClientPrincipalId);
            await GetOrCreateUserClaims(userId, claims);
            principal.AddIdentity(new ClaimsIdentity(claims, easyAuthProvider, ClaimConstants.EmailNameClaimName, ClaimConstants.RoleClaimName));
            AuthenticationTicket ticket = new(principal, easyAuthProvider);
            AuthenticateResult success = AuthenticateResult.Success(ticket);
            Context.User = principal;
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while performing authentication");
            return AuthenticateResult.Fail(ex);
        }
    }

    private async Task GetOrCreateUserClaims(Guid userId, List<Claim> claims)
    {
        string cacheKey = $"users/{userId}";
        string? roleAndUserName = await _cache.GetStringAsync(cacheKey);
        if (roleAndUserName == null)
        {
            GetUserInfoResult? response = await _mediator.Send(new GetUserInfo() { Id = userId });
            if (response == null)
            {
                return;
            }
            claims.Add(new Claim(ClaimConstants.RoleClaimName, response.Role.ToString()));
            claims.Add(new Claim(ClaimConstants.UserNameClaimName, response.UserName));
            await _cache.SetStringAsync(cacheKey, $"{response.Role};{response.UserName}", _cacheOptions);
        }
        else
        {
            string[] roleAndUserNameCacheValue = roleAndUserName.Split(';', 2);
            claims.Add(new Claim(ClaimConstants.RoleClaimName, roleAndUserNameCacheValue[0]));
            claims.Add(new Claim(ClaimConstants.UserNameClaimName, roleAndUserNameCacheValue[1]));
            await _cache.RefreshAsync(cacheKey);
        }
    }
}

public class EasyAuthAuthenticationOptions : AuthenticationSchemeOptions
{
    public EasyAuthAuthenticationOptions()
    {
        Events = new object();
    }
}

public static class EasyAuthAuthenticationBuilderExtensions
{
    public const string EASYAUTHSCHEMENAME = "EasyAuth";

    public static AuthenticationBuilder AddAzureContainerAppsEasyAuth(this AuthenticationBuilder authenticationBuilder, Action<EasyAuthAuthenticationOptions>? configure = null)
    {
        configure ??= o => { };
        return authenticationBuilder.AddScheme<EasyAuthAuthenticationOptions, EasyAuthAuthenticationHandler>(EASYAUTHSCHEMENAME, EASYAUTHSCHEMENAME, configure);
    }
}
