using System.Text.Json.Serialization;

namespace BlandAIBankHeist.Web.Models;

public class CallDetailsModel
{
    [JsonPropertyName("call_id")]
    public required string CallId { get; init; }

    [JsonPropertyName("to")]
    public required string ToPhoneNumber { get; init; }
}
