using System.Text.Json.Serialization;

namespace BlandAIBankHeist.Web.Models;

public sealed class ScheduleRecallDTO
{
    [JsonPropertyName("call_id")]
    public required string CallId { get; init; }
}
