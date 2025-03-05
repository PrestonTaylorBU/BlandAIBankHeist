using System.Text.Json.Serialization;

namespace BlandAIBankHeist.Web.Models;

public sealed class QueueCallModel
{
    [JsonPropertyName("phone_number")]
    public required string PhoneNumber { get; init; }

    [JsonPropertyName("task")]
    public required string Task { get; init; }
}
