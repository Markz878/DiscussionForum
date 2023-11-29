using DiscussionForum.Shared.DTO.Topics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscussionForum.Shared.DTO;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(AddMessageResponse))]
[JsonSerializable(typeof(EditMessageRequest))]
[JsonSerializable(typeof(EditMessageResult))]
[JsonSerializable(typeof(EditTopicTitleRequest))]
[JsonSerializable(typeof(ListLatestTopicsResult))]
[JsonSerializable(typeof(GetTopicByIdResult))]
public partial class JsonContext : JsonSerializerContext
{
}
