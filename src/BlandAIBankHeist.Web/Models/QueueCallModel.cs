using System.Text.Json.Serialization;

namespace BlandAIBankHeist.Web.Models;

public sealed class QueueCallModel
{
    [JsonPropertyName("phone_number")]
    public required string PhoneNumber { get; init; }

    [JsonPropertyName("pathway_id")]
    public required string PathwayId { get; init; }

    [JsonPropertyName("voice")]
    public required string Voice { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("background_track")]
    public string? BackgroundTrack { get; init; }
}
