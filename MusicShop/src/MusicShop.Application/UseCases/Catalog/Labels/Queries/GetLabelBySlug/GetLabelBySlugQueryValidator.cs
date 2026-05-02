using FluentValidation;

namespace MusicShop.Application.UseCases.Catalog.Labels.Queries.GetLabelBySlug;

public sealed class GetLabelBySlugQueryValidator : AbstractValidator<GetLabelBySlugQuery>
{
    public GetLabelBySlugQueryValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .Matches(@"^[a-z0-9-]+$").WithMessage("Invalid slug format.");
    }
}
