using Azure.Core;
using Azure.Identity;
using DiscussionForum.Core.DataAccess;
using DiscussionForum.Core.DataAccess.Models;
using DiscussionForum.Core.HelperMethods;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        AccessToken token = credential.GetToken(new TokenRequestContext(["https://database.windows.net/.default"], tenantId: tenantId));
        sqlConnection.AccessToken = token.Token;
        sqlConnection.Open();
        AppDbContext db = new(new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(sqlConnection)
        .LogTo(Console.WriteLine, LogLevel.Warning).Options);
        List<Topic> topics = Fakers.GetTopics(10000, 10000);
        db.Users.Add(Fakers.Admin);
        db.Users.Add(Fakers.User);
        db.SaveChanges();
        db.Topics.AddRange(topics);
        db.SaveChanges();
    }
}
else
{
    Console.WriteLine("Delete and rebuild database?");
    string? response = Console.ReadLine();
    AppDbContext db = new(new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(configuration.GetConnectionString("SqlServer"))
        .LogTo(Console.WriteLine, LogLevel.Warning).Options);
    if (response is not null and "y")
    {
        db.Database.EnsureDeleted();
        db.Database.Migrate();
        db.Users.Add(Fakers.Admin);
        db.Users.Add(Fakers.User);
        db.SaveChanges();
    }
    List<Topic> topics = Fakers.GetTopics(10000, 10000);
    db.Topics.AddRange(topics);
    db.SaveChanges();
}


