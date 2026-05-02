using FluentValidation;

namespace MusicShop.Application.UseCases.Catalog.CatalogSearch;

public sealed class SearchCatalogQueryValidator : AbstractValidator<SearchCatalogQuery>
{
    public SearchCatalogQueryValidator()
    {
        RuleFor(x => x.Q)
            .NotEmpty().WithMessage("Search query is required.")
            .MinimumLength(2).WithMessage("Search query must be at least 2 characters.");
    }
}
