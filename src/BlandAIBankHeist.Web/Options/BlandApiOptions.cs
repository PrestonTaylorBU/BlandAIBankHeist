namespace BlandAIBankHeist.Web.Options;

public sealed class BlandApiOptions
{
    public const string Section = "BlandApi";

    public required string ApiUrl { get; init; }
    public required string ApiKey { get; init; }
    public required string BankHeistIntroductionPathwayId { get; init; }
    public required string BankHeistJobPathwayId { get; init; }
    public required int RecallDelayInSeconds { get; init; }
}