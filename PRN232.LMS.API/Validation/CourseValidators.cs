using FluentValidation;
using PRN232.LMS.API.Models.Requests;

namespace PRN232.LMS.API.Validation;

public class CourseCreateRequestValidator : AbstractValidator<CourseCreateRequest>
{
    public CourseCreateRequestValidator()
    {
        RuleFor(x => x.CourseName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SemesterId).GreaterThan(0);
    }
}

public class CourseUpdateRequestValidator : AbstractValidator<CourseUpdateRequest>
{
    public CourseUpdateRequestValidator()
    {
        RuleFor(x => x.CourseName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SemesterId).GreaterThan(0);
    }
}