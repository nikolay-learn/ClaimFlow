using ClaimFlow.Api.Domain;

namespace ClaimFlow.Api.Tests.Domain;

public class PayoutCalculatorTests
{
    public static TheoryData<decimal, decimal, decimal, decimal> Cases =>
        new()
        {
            // claimed, deductible, limit, expected
            { 1200m, 250m, 5000m, 951m }, // normal claim
            { 200m, 250m, 5000m, 0m }, // loss smaller than the deductible
            { 9000m, 250m, 5000m, 5000m }, // capped by the coverage limit
            { 0m, 0m, 5000m, 0m }, // nothing claimed
        };

    [Theory]
    [MemberData(nameof(Cases))]
    public void Calculate_applies_deductible_and_limit(decimal claimed, decimal deductible, decimal limit, decimal expected)
    {
        decimal payout = PayoutCalculator.Calculate(claimed, deductible, limit);

        Assert.Equal(payout, expected);
    }
}
