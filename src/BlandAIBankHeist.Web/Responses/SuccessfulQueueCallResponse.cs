using System.Text.Json.Serialization;

namespace BlandAIBankHeist.Web.Responses;

public sealed class SuccessfulQueueCallResponse
{
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("call_id")]
    public required string CallId { get; init; }
}
