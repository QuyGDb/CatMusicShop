using FluentValidation;

namespace MusicShop.Application.UseCases.Catalog.Labels.Commands.DeleteLabel;

public sealed class DeleteLabelCommandValidator : AbstractValidator<DeleteLabelCommand>
{
    public DeleteLabelCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Label ID is required.");
    }
}
