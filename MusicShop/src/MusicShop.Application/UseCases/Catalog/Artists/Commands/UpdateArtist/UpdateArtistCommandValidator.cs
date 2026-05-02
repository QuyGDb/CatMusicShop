using FluentValidation;

namespace MusicShop.Application.UseCases.Catalog.Artists.Commands.UpdateArtist;

public sealed class UpdateArtistCommandValidator : AbstractValidator<UpdateArtistCommand>
{
    public UpdateArtistCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Artist ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Artist name is required.")
            .MaximumLength(200).WithMessage("Artist name must not exceed 200 characters.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MaximumLength(200).WithMessage("Slug must not exceed 200 characters.");

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters.");

        RuleFor(x => x.GenreIds)
            .Must(x => x == null || x.Distinct().Count() == x.Count)
            .WithMessage("Genre IDs must be unique.");
    }
}
