using FluentValidation;
using LibraryManagement.Api.DTOs.Authors;

namespace LibraryManagement.Api.Validators.Authors;

public class CreateAuthorDtoValidator : AbstractValidator<CreateAuthorDto>
{
    public CreateAuthorDtoValidator()
    {
        RuleFor(dto => dto.FirstName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(dto => dto.LastName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(dto => dto.YearOfBirth)
            .InclusiveBetween(-2000, 2026)
            .When(dto => dto.YearOfBirth.HasValue);
    }
}
