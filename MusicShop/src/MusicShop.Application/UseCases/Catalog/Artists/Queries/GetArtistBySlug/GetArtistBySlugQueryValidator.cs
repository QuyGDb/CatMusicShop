using FluentValidation;

namespace MusicShop.Application.UseCases.Catalog.Artists.Queries.GetArtistBySlug;

public sealed class GetArtistBySlugQueryValidator : AbstractValidator<GetArtistBySlugQuery>
{
    public GetArtistBySlugQueryValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .Matches(@"^[a-z0-9-]+$").WithMessage("Invalid slug format.");
    }
}
