using FluentValidation;

namespace MusicShop.Application.UseCases.Shop.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(300)
            .When(command => command.Name is not null);

        RuleFor(command => command.Slug)
            .NotEmpty()
            .MaximumLength(300)
            .When(command => command.Slug is not null);

        RuleFor(command => command.LimitedQty)
            .GreaterThan(0)
            .When(command => command.LimitedQty.HasValue);

        RuleFor(command => command.PreorderReleaseDate)
            .GreaterThan(DateTime.UtcNow)
            .When(command => command.IsPreorder == true && command.PreorderReleaseDate.HasValue);
    }
}
