using FluentValidation;

namespace MusicShop.Application.UseCases.Catalog.Releases.Commands.DeleteRelease;

public sealed class DeleteReleaseCommandValidator : AbstractValidator<DeleteReleaseCommand>
{
    public DeleteReleaseCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Release ID is required.");
    }
}
