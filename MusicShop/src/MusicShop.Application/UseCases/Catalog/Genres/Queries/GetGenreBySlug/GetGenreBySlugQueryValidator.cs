using FluentValidation;

namespace MusicShop.Application.UseCases.Catalog.Genres.Queries.GetGenreBySlug;

public sealed class GetGenreBySlugQueryValidator : AbstractValidator<GetGenreBySlugQuery>
{
    public GetGenreBySlugQueryValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .Matches(@"^[a-z0-9-]+$").WithMessage("Invalid slug format.");
    }
}
