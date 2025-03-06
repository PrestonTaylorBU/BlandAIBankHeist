using BlandAIBankHeist.Web.Models;

namespace BlandAIBankHeist.Web.Services;

public interface IBlandApiService
{
    public Task<string> TryToQueueCallAsync(string phoneNumber);
    public Task<CallDetailsModel?> GetCallDetailsAsync(string callId);
}
