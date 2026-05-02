using FluentValidation;

namespace MusicShop.Application.UseCases.Catalog.Artists.Queries.GetArtistById;

public sealed class GetArtistByIdQueryValidator : AbstractValidator<GetArtistByIdQuery>
{
    public GetArtistByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Artist ID is required.");
    }
}
