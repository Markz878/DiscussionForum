using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace DiscussionForum.Tests.IntegrationTests.Infrastructure;

internal static class Extensions
{
    internal static async Task<Dictionary<string, string[]>> ToProblemDetailsDictionary(this HttpResponseMessage response)
    {
        ProblemDetails? content = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        ArgumentNullException.ThrowIfNull(content);
        if (content.Extensions.TryGetValue("errors", out object? errors) && errors is not null)
        {
            string errorsString = errors.ToString() ?? string.Empty;
            Dictionary<string, string[]>? errorDictionary = JsonSerializer.Deserialize<Dictionary<string, string[]>>(errorsString);
            ArgumentNullException.ThrowIfNull(errorDictionary);
            return errorDictionary;
        }
        throw new KeyNotFoundException("No key named 'errors' in ProblemDetails");
    }
}
