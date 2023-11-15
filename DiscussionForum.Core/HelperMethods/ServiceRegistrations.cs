using Azure.Identity;
using Azure.Storage.Blobs;
using DiscussionForum.Core.Behaviors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiscussionForum.Core.HelperMethods;
public static class ServiceRegistrations
{
    public static IServiceCollection RegisterCoreServices(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        services.AddValidatorsFromAssemblyContaining<AppDbContext>(ServiceLifetime.Singleton);
        services.AddMediatR(x => x.RegisterServicesFromAssemblyContaining<AppDbContext>().AddOpenRequestPreProcessor(typeof(ValidationBehavior<>)));
        services.AddDbContext<AppDbContext>(x =>
        {
            x.UseSqlServer(configuration.GetConnectionString("SqlServer"), o => o.EnableRetryOnFailure(3));
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
        services.AddSingleton<IFileService, AzureBlobStorageService>();
        return services;
    }
}
