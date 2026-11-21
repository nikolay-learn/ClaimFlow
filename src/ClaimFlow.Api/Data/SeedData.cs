using ClaimFlow.Api.Domain;

namespace ClaimFlow.Api.Data;

public static class SeedData
{
    public static IEnumerable<Claim> Claims()
    {
        var windshield = new Claim
        {
            PolicyNumber = "POL-1",
            Description = "Cracked window by a flying stone",
            ClaimedAmount = 480.00m,
            IncidentDate = new DateOnly(2026, 9, 14)
        };
        windshield.Approve("Standard glass repair");
        yield return windshield;

        var bumper = new Claim
        {
            PolicyNumber = "POL-1",
            Description = "Rear bumper damaged in a parking lot",
            ClaimedAmount = 1_250.00m,
            IncidentDate = new DateOnly(2026, 10, 20)
        };
        bumper.StartReview();
        yield return bumper;

        var flood = new Claim
        {
            PolicyNumber = "POL-300555",
            Description = "Basement flooded after a burst pipe",
            ClaimedAmount = 7_900.00m,
            IncidentDate = new DateOnly(2026, 8, 2)
        };
        flood.Reject("Pipe damage from lack of maintenance is excluded");
        yield return flood;

        yield return new Claim
        {
            PolicyNumber = "POL-420017",
            Description = "Laptop stolen from a locked car",
            ClaimedAmount = 2_100.00m,
            IncidentDate = new DateOnly(2026, 10, 1)
        };

        yield return new Claim
        {
            PolicyNumber = "POL-300555",
            Description = "Roof tiles blown off during a storm",
            ClaimedAmount = 3_400.00m,
            IncidentDate = new DateOnly(2026, 10, 11)
        };
    }
}