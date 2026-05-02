using FluentValidation;

namespace MusicShop.Application.UseCases.Catalog.Releases.Queries.GetReleaseById;

public sealed class GetReleaseByIdQueryValidator : AbstractValidator<GetReleaseByIdQuery>
{
    public GetReleaseByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Release ID is required.");
    }
}
