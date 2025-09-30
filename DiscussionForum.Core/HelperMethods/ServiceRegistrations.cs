using Azure.Identity;
using Azure.Storage.Blobs;
using DiscussionForum.Core.Services;
using DiscussionForum.Shared.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiscussionForum.Core.HelperMethods;
public static class ServiceRegistrations
{
    public static IServiceCollection RegisterCoreServices(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        // Register service implementations
        services.AddScoped<ITopicsService, TopicsService>();
        services.AddScoped<IMessagesService, MessagesService>();
        services.AddScoped<IMessageLikesService, MessageLikesService>();
        services.AddScoped<IUsersService, UsersService>();
        
        services.AddDbContext<AppDbContext>(x =>
        {
            x.UseAzureSql(configuration.GetConnectionString("SqlServer"));
            if (isDevelopment)
            {
                x.EnableSensitiveDataLogging();
                x.EnableDetailedErrors();
            }
        }, ServiceLifetime.Transient);
        if (isDevelopment)
        {
            services.AddSingleton(new BlobServiceClient("DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;"));
        }
        else
        {
            ManagedIdentityCredential credential = new(configuration["ManagedIdentityId"]);
            services.AddSingleton(new BlobServiceClient(new Uri(configuration["FileStorageSettings:StorageUri"] ?? throw new ArgumentNullException("StorageUri configuration value")), credential));
        }
        services.AddDataProtection().PersistKeysToDbContext<AppDbContext>();
        services.AddScoped<IFileService, AzureBlobStorageService>();
        return services;
    }
}
