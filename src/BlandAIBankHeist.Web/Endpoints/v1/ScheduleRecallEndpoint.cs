using BlandAIBankHeist.Web.Models;
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
        IBlandApiService blandApiService, [FromBody]ScheduleRecallDTO scheduleRecallRequest)
    {
        logger.LogInformation("Schedule recall requested with call id {CallID}.", scheduleRecallRequest.CallId);

        var callDetails = await blandApiService.GetCallDetailsAsync(scheduleRecallRequest.CallId);
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
                    apiOptions.CurrentValue.JobVoice, "office");
                logger.LogInformation("Successfully scheduled recall with call id {CallID}.", scheduleRecallRequest.CallId);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "Failed to schedule recall with user with call id {CallID}.", scheduleRecallRequest.CallId);
            }
        });

        return Results.Ok();
    }
}
