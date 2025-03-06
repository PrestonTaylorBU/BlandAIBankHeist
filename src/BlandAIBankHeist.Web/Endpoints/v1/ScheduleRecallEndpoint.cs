using BlandAIBankHeist.Web.Options;
using BlandAIBankHeist.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BlandAIBankHeist.Web.Endpoints.v1;

// TODO: The v1 endpoints probably should be protected by an API key or something similar.
public static class ScheduleRecallEndpoint
{
    static public void MapScheduleRecallEndpoint(this WebApplication app)
    {
        app.MapPost("/v1/recall", ScheduleRecall);
    }

    public class ScheduleRecallAPI;

    static public async Task<IResult> ScheduleRecall(ILogger<ScheduleRecallAPI> logger, IOptionsMonitor<BlandApiOptions> apiOptions,
        IBlandApiService blandApiService, [FromBody]string callId)
    {
        logger.LogInformation("Schedule recall requested with call id {CallID}.", callId);

        var callDetails = await blandApiService.GetCallDetailsAsync(callId);
        if (callDetails is null)
        {
            return Results.BadRequest("Invalid call id provided.");
        }

        // NOTE: This is a hack as the start_time variable for the BlandAPI didn't seem to work so we have to create a task here.
        var _ = Task.Run(async () =>
        {
            await Task.Delay(apiOptions.CurrentValue.RecallDelayInSeconds * 1000);

            try
            {
                var newCallId = await blandApiService.TryToQueueCallAsync(callDetails.ToPhoneNumber, apiOptions.CurrentValue.BankHeistJobPathwayId,
                    apiOptions.CurrentValue.JobVoice);
                logger.LogInformation("Successfully scheduled recall with call id {CallID}.", callId);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "Failed to schedule recall with user with call id {CallID}.", callId);
            }
        });

        return Results.Ok();
    }
}
