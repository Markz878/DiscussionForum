using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiscussionForum.Core.HelperMethods;

public static class DataSeeder
{
    public static void SeedData(IServiceProvider serviceProvider,
                                int topicMinCount = 80,
                                int topicMaxCount = 80,
                                int messageMinCount = 1,
                                int messageMaxCount = 50,
                                int attachedFileMinCount = 0,
                                int attachedFileMaxCount = 4)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        db.Users.ExecuteDelete();
        db.Users.Add(Fakers.Admin);
        db.Users.Add(Fakers.User);
        db.SaveChanges();
        List<Topic> topics = Fakers.GetTopics(topicMinCount, topicMaxCount, messageMinCount, messageMaxCount, attachedFileMinCount, attachedFileMaxCount);
        db.Topics.AddRange(topics);
        db.SaveChanges();
    }
}