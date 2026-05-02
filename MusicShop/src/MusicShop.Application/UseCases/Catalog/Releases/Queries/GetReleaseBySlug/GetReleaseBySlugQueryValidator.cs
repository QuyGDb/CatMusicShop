using FluentValidation;

namespace MusicShop.Application.UseCases.Catalog.Releases.Queries.GetReleaseBySlug;

public sealed class GetReleaseBySlugQueryValidator : AbstractValidator<GetReleaseBySlugQuery>
{
    public GetReleaseBySlugQueryValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .Matches(@"^[a-z0-9-]+$").WithMessage("Invalid slug format.");
    }
}
