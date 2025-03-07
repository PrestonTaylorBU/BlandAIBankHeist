using BlandAIBankHeist.Web.Models;

namespace BlandAIBankHeist.Web.Services;

public interface IBlandApiService
{
    public Task<string> TryToQueueCallAsync(string phoneNumber, string pathwayId, string voice, string? backgroundTrack = null);
    public Task<CallDetailsModel?> GetCallDetailsAsync(string callId);
}
