using FluentValidation;

namespace MusicShop.Application.UseCases.Catalog.Genres.Commands.DeleteGenre;

public sealed class DeleteGenreCommandValidator : AbstractValidator<DeleteGenreCommand>
{
    public DeleteGenreCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Genre ID is required.");
    }
}
