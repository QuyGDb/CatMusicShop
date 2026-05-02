using FluentValidation;

namespace MusicShop.Application.UseCases.Catalog.Labels.Commands.UpdateLabel;

public sealed class UpdateLabelCommandValidator : AbstractValidator<UpdateLabelCommand>
{
    public UpdateLabelCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Label ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Label name is required.")
            .MaximumLength(200).WithMessage("Label name must not exceed 200 characters.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MaximumLength(200).WithMessage("Slug must not exceed 200 characters.");

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters.");

        RuleFor(x => x.FoundedYear)
            .LessThanOrEqualTo(DateTime.Now.Year)
            .WithMessage($"Founded year cannot be in the future (>{DateTime.Now.Year}).")
            .When(x => x.FoundedYear.HasValue);

        RuleFor(x => x.Website)
            .MaximumLength(255).WithMessage("Website URL must not exceed 255 characters.");
    }
}
