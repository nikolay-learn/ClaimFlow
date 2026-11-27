using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClaimFlow.Api.Domain;
using ClaimFlow.Api.Features.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ClaimFlow.Api.Tests.Api;

public class ClaimEndpointsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static DateOnly DaysAgo(int days) => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-days);

    [Fact]
    public async Task Created_claim_can_be_fetched_by_id()
    {
        // Arrange
        var request = new CreateClaimRequest("POL-123456", "Water leak in the bathroom ceiling", 900m, DaysAgo(2));

        // Act
        HttpResponseMessage created = await _client.PostAsJsonAsync("/claims/", request, Ct);
        var body = await created.Content.ReadFromJsonAsync<ClaimResponse>(_json, Ct);
        var fetched = await _client.GetFromJsonAsync<ClaimResponse>($"/claims/{body!.Id}", _json, Ct);

        // Assert
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal($"/claims/{body.Id}", created.Headers.Location?.ToString());
        Assert.Equal(ClaimStatus.Submitted, body.Status);
        Assert.Equal(body, fetched);
    }

    [Fact]
    public async Task Invalid_claim_returns_400_with_field_errors()
    {
        // Arrange
        var request = new CreateClaimRequest("Bad", "short", 0.0m, DaysAgo(0));

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/claims", request, Ct);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(Ct);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("PolicyNumber", problem!.Errors.Keys);
    }

    [Fact]
    public async Task Approving_a_rejected_claim_returns_409()
    {
        var rejected = await _client.GetFromJsonAsync<List<ClaimResponse>>("/claims?status=Rejected", _json, Ct);

        HttpResponseMessage response = await _client.PostAsJsonAsync($"/claims/{rejected![0].Id}/approve", new ReviewClaimRequest("Looks fine"), Ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_claim_returns_404()
    {
        HttpResponseMessage response = await _client.GetAsync($"/claims/{Guid.NewGuid()}", Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1")]
    [InlineData(" 1")]
    [InlineData("      ")]
    public async Task Rejecting_claim_with_empty_or_short_reason_returns_400_with_error_for_Reason(string reason)
    {
        var submitted = await _client.GetFromJsonAsync<List<ClaimResponse>>("/claims?status=Submitted", _json, Ct);

        HttpResponseMessage response = await _client.PostAsJsonAsync($"/claims/{submitted![0].Id}/reject", new RejectClaimRequest(reason), Ct);

        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Reason", problem!.Errors.Keys);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData(" 1234")]
    public async Task Rejecting_claim_is_allowed_only_with_enough_Reason_length(string reason)
    {
        var submitted = await _client.GetFromJsonAsync<List<ClaimResponse>>("/claims?status=Submitted", _json, Ct);

        var response = await _client.PostAsJsonAsync($"/claims/{submitted![0].Id}/reject", new RejectClaimRequest(reason), Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Max_claim_amount_comes_from_configuration()
    {
        using var customFactory = factory.WithWebHostBuilder(builder => builder.UseSetting("ClaimRules:MaxClaimAmount", "1000"));

        var client = customFactory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/claims",
            new CreateClaimRequest("POL-123456", "MaxClaimAmount from config", 1500m, DaysAgo(1)),
            Ct
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("123456789012", false)]
    [InlineData("123456", true)]
    public async Task Rejecting_claim_is_not_allowed_with_too_big_Reason_length(string reason, bool shouldBeValid, int maxLength = 10)
    {
        using var customFactory = factory.WithWebHostBuilder(builder => builder.UseSetting("ClaimRules:MaxNotesLength", maxLength + ""));

        var _client = customFactory.CreateClient();
        var submitted = await _client.GetFromJsonAsync<List<ClaimResponse>>("/claims?status=Submitted", _json, Ct);

        var response = await _client.PostAsJsonAsync($"/claims/{submitted![0].Id}/reject", new RejectClaimRequest(reason), Ct);

        if (shouldBeValid)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        else
        {
            var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(Ct);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Reason", problem!.Errors.Keys);
        }
    }
}
