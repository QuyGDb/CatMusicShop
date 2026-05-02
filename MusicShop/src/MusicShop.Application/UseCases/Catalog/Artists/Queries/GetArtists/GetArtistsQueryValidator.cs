using FluentValidation;

namespace MusicShop.Application.UseCases.Catalog.Artists.Queries.GetArtists;

public sealed class GetArtistsQueryValidator : AbstractValidator<GetArtistsQuery>
{
    public GetArtistsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Page size must be at least 1.")
            .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100.");
    }
}
