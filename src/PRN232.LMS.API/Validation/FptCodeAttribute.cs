using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PRN232.LMS.API.Validation;

public class FptCodeAttribute : ValidationAttribute
{
    private static readonly Regex Pattern = new(@"^[A-Z]{2,4}\d{3,6}$", RegexOptions.Compiled);

    public FptCodeAttribute()
        : base("The {0} field must be an FPT-style code: uppercase letters followed by digits, e.g. PRN232.")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
    {
        if (value is null) return ValidationResult.Success;
        var s = value.ToString();
        return (!string.IsNullOrWhiteSpace(s) && Pattern.IsMatch(s!))
            ? ValidationResult.Success
            : new ValidationResult(FormatErrorMessage(ctx.DisplayName));
    }
}