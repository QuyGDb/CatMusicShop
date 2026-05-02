using FluentValidation;

namespace MusicShop.Application.UseCases.Catalog.Labels.Queries.GetLabelById;

public sealed class GetLabelByIdQueryValidator : AbstractValidator<GetLabelByIdQuery>
{
    public GetLabelByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Label ID is required.");
    }
}
