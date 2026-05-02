using FluentValidation;

namespace MusicShop.Application.UseCases.Catalog.ReleaseVersions.Commands.DeleteReleaseVersion;

public sealed class DeleteReleaseVersionCommandValidator : AbstractValidator<DeleteReleaseVersionCommand>
{
    public DeleteReleaseVersionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Release version ID is required.");
    }
}
