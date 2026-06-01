using MediatR;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Common;
using MusicShop.Domain.Errors;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Application.UseCases.Shop.CuratedCollections.Commands.RemoveProductFromCollection;

public sealed class RemoveProductFromCollectionCommandHandler(
    ICuratedCollectionRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RemoveProductFromCollectionCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(RemoveProductFromCollectionCommand request, CancellationToken cancellationToken)
    {
        if (!await repository.AnyAsync(c => c.Id == request.CollectionId, cancellationToken))
            return Result<Unit>.Failure(CuratedCollectionErrors.NotFound);

        if (!await repository.ItemExistsAsync(request.CollectionId, request.ProductId, cancellationToken))
            return Result<Unit>.Failure(CuratedCollectionErrors.ProductNotFound);

        await repository.RemoveItemAsync(request.CollectionId, request.ProductId, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}

