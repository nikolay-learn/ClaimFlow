using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ClaimFlow.Api.Infrastructure;

[AttributeUsage(
    AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter,
    AllowMultiple = false
)]
public sealed class NotInFutureAttribute(int MaxPastYears = 3) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not DateOnly date)
        {
            return new ValidationResult(
                "Expected DateOnly value",
                validationContext.MemberName is not null ? [validationContext.MemberName] : []
            );
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (date > today)
        {
            return new ValidationResult(
                "Date can't be in the future",
                validationContext.MemberName is not null ? [validationContext.MemberName] : []
            );
        }

        if (date < today.AddYears(-MaxPastYears))
        {
            return new ValidationResult(
                $"Date should be maximum {MaxPastYears} years till today",
                validationContext.MemberName is not null ? [validationContext.MemberName] : []
            );
        }

        return ValidationResult.Success;
    }
}
