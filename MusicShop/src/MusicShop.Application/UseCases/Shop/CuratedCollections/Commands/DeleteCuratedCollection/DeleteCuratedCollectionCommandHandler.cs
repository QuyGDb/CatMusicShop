using MediatR;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Common;
using MusicShop.Domain.Errors;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Application.UseCases.Shop.CuratedCollections.Commands.DeleteCuratedCollection;

public sealed class DeleteCuratedCollectionCommandHandler(
    ICuratedCollectionRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCuratedCollectionCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteCuratedCollectionCommand request, CancellationToken cancellationToken)
    {
        var collection = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (collection == null)
            return Result<Unit>.Failure(CuratedCollectionErrors.NotFound);

        repository.Delete(collection);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}

