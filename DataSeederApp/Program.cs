using Azure.Core;
using Azure.Identity;
using DiscussionForum.Core.DataAccess;
using DiscussionForum.Core.DataAccess.Models;
using DiscussionForum.Core.HelperMethods;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

IConfiguration configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
string? tenantId = configuration["TenantId"];
if (!string.IsNullOrEmpty(tenantId))
{
    Console.WriteLine("Targeting cloud, are you sure?");
    string? response = Console.ReadLine();
    if (response is not null and "y")
    {
        SqlConnection sqlConnection = new(configuration.GetConnectionString("SqlServer"));
        AzureCliCredential credential = new(new AzureCliCredentialOptions() { TenantId = tenantId });
        sqlConnection.Open();
        AppDbContext db = new(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(sqlConnection)
            .LogTo(Console.WriteLine, LogLevel.Information).Options);
        List<Topic> topics = Fakers.GetTopics(100, 100);
        db.Users.ExecuteDelete();
        db.Users.Add(Fakers.Admin);
        db.Users.Add(Fakers.User);
        db.SaveChanges();

        Console.WriteLine("Adding topics");
        foreach (Topic[] batch in topics.Chunk(topics.Count / 10))
        {
            db.Topics.AddRange(batch);
            db.SaveChanges();
            Console.WriteLine($"Added {batch.Length} topics..");
        }
        Console.WriteLine("Done!");
    }
}
else
{
    Console.WriteLine("Delete and rebuild database (y)?");
    string? response = Console.ReadLine();
    if (response is "y")
    {
        Console.WriteLine("Deleting and rebuilding database..");
        IServiceProvider serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("SqlServer"));
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
                options.EnableThreadSafetyChecks(false);
            })
            .BuildServiceProvider();
        DataSeeder.SeedData(serviceProvider);
        Console.WriteLine("Database seeded.");
    }
}


