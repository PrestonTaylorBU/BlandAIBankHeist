namespace BlandAIBankHeist.Web.Services;

public interface IBlandApiService
{
    public Task<string> TryToQueueCallAsync(string phoneNumber);
}
