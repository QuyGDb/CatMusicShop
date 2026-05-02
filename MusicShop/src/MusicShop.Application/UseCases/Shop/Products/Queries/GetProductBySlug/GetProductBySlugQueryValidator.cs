using FluentValidation;

namespace MusicShop.Application.UseCases.Shop.Products.Queries.GetProductBySlug;

public sealed class GetProductBySlugQueryValidator : AbstractValidator<GetProductBySlugQuery>
{
    public GetProductBySlugQueryValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .Matches(@"^[a-z0-9-]+$").WithMessage("Invalid slug format.");
    }
}
