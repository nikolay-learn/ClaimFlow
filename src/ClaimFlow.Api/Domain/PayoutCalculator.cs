namespace ClaimFlow.Api.Domain;

/// <summary>
/// Calculates how much the insurer pays for a claim.
/// Rule: payout = claimed amount minus the deductible, but never below zero
/// and never above the policy's coverage limit.
/// </summary>
public static class PayoutCalculator
{
    public static decimal Calculate(decimal claimedAmount, decimal deductible, decimal coverageLimit)
    {
        if (claimedAmount <= 0m)
        {
            return 0m;
        }

        decimal afterDeductible = claimedAmount - deductible;

        if (afterDeductible <= 0m)
        {
            return 0m;
        }

        return Math.Min(afterDeductible, coverageLimit);
    }

    public static decimal CalculateWithCoinsurance(decimal claimedAmount, decimal deductible, decimal coverageLimit, decimal rate)
    {
        var aftereDeductible = Calculate(claimedAmount, deductible, decimal.MaxValue);
        var insurerShare = Math.Round(aftereDeductible * rate, 2, MidpointRounding.ToEven);
        return Math.Min(insurerShare, coverageLimit);
    }

    public static string Classify(decimal amount)
    {
        return amount switch
        {
            < 1_000m => "Small",
            < 10_000m => "Medium",
            _ => "Large",
        };
    }
}
