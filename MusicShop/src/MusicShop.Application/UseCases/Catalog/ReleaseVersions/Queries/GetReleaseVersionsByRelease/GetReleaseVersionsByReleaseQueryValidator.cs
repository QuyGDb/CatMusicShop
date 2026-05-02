using FluentValidation;

namespace MusicShop.Application.UseCases.Catalog.ReleaseVersions.Queries.GetReleaseVersionsByRelease;

public sealed class GetReleaseVersionsByReleaseQueryValidator : AbstractValidator<GetReleaseVersionsByReleaseQuery>
{
    public GetReleaseVersionsByReleaseQueryValidator()
    {
        RuleFor(x => x.ReleaseId)
            .NotEmpty().WithMessage("Release ID is required.");
    }
}
