using FluentValidation;
using FluentValidation.Results;

namespace DiscussionForum.Server.Filters;

public class ExceptionFilter : IEndpointFilter
{
    private readonly ILogger<ExceptionFilter> _logger;

    public ExceptionFilter(ILogger<ExceptionFilter> logger)
    {
        _logger = logger;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        _logger.LogWarning("GOT REQUEST in path {Path}", context.HttpContext.Request.Path);
        try
        {
            return await next(context);
        }
        catch (BusinessException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
        catch (NotFoundException ex)
        {
            return TypedResults.NotFound(ex.Message);
        }
        catch (ForbiddenException)
        {
            return TypedResults.Unauthorized();
        }
        catch (ConflictException ex)
        {
            return TypedResults.Conflict(ex.Message);
        }
        catch (ValidationException ex)
        {
            return TypedResults.ValidationProblem(new ValidationResult(ex.Errors).ToDictionary());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return TypedResults.Problem();
        }
    }
}
