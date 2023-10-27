using Bogus;
using DiscussionForum.Shared.DTO.Users;

namespace DiscussionForum.Core.HelperMethods;

internal static class Fakers
{
    private static int messageMinCount;
    private static int messageMaxCount;
    private static int attachedFileMinCount;
    private static int attachedFileMaxCount;
    private static readonly SemaphoreSlim _semaphore = new(1);
    private static int seed = 2;

    public static User Admin { get; } = new()
    {
        Id = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
        UserName = "Test Admin",
        Email = "admin@email.com",
        Role = Role.Admin
    };

    public static User User { get; } = new()
    {
        Id = Guid.Parse("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB"),
        UserName = "Test User",
        Email = "user@email.com",
        Role = Role.User
    };

    internal static List<Topic> GetTopics(int topicMinCount = 60,
                                          int topicMaxCount = 120,
                                          int messageMinCount = 1,
                                          int messageMaxCount = 30,
                                          int attachedFileMinCount = 0,
                                          int attachedFileMaxCount = 4,
                                          int seed = 2)
    {
        _semaphore.Wait();
        Fakers.messageMinCount = messageMinCount;
        Fakers.messageMaxCount = messageMaxCount;
        Fakers.attachedFileMinCount = attachedFileMinCount;
        Fakers.attachedFileMaxCount = attachedFileMaxCount;
        Fakers.seed = seed;
        List<Topic> result = TopicFaker.GenerateBetween(topicMinCount, topicMaxCount);
        _semaphore.Release();
        return result;
    }

    private static readonly Faker<Topic> TopicFaker = new Faker<Topic>().UseSeed(seed)
        .RuleFor(x => x.UserId, f => f.PickRandom(Admin.Id, User.Id))
        .RuleFor(x => x.CreatedAt, f => f.Date.PastOffset(1))
        .RuleFor(x => x.Title, f => f.Rant.Review())
        .RuleFor(x => x.Messages, f => MessageFaker.GenerateBetween(messageMinCount, messageMaxCount))
        .RuleFor(x => x.LastMessageTimeStamp, (_, x) => x.Messages.Max(x => x.CreatedAt));

    private static readonly Faker<Message> MessageFaker = new Faker<Message>().UseSeed(seed)
        .RuleFor(x => x.UserId, f => f.PickRandom(Admin.Id, User.Id))
        .RuleFor(x => x.CreatedAt, f => f.Date.PastOffset(1))
        .RuleFor(x => x.Content, f => f.Lorem.Paragraphs(2))
        .RuleFor(x => x.MessageLikes, f => MessageLikesFaker.GenerateBetween(0, 2).DistinctBy(x => x.UserId).ToList())
        .RuleFor(x => x.AttachedFiles, f => MessageAttachmentsFaker.GenerateBetween(attachedFileMinCount, attachedFileMaxCount));

    private static readonly Faker<MessageLike> MessageLikesFaker = new Faker<MessageLike>().UseSeed(seed)
        .RuleFor(x => x.UserId, f => f.PickRandom(Admin.Id, User.Id));

    private static readonly Faker<MessageAttachedFile> MessageAttachmentsFaker = new Faker<MessageAttachedFile>().UseSeed(seed)
        .RuleFor(x => x.Name, f => f.System.CommonFileName());
}
