using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace ClaimFlow.Api.Features.Claims;

public sealed class ClaimRulesOptions
{
    public const string SectionName = "ClaimRules";

    [Range(1, 10)]
    public int FilingWindowYears { get; set; } = 3;

    [Range(typeof(decimal), "1", "100000000")]
    public decimal MaxClaimAmount { get; set; } = 1_000_000m;

    public decimal MaxNotesLength { get; set; } = 500;
}
