using MediatR;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Shop;
using MusicShop.Domain.Errors;
using MusicShop.Domain.Interfaces;
namespace MusicShop.Application.UseCases.Shop.CuratedCollections.Commands.AddProductToCollection;

public sealed class AddProductToCollectionCommandHandler(
    ICuratedCollectionRepository repository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddProductToCollectionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddProductToCollectionCommand request, CancellationToken cancellationToken)
    {
        if (!await repository.AnyAsync(c => c.Id == request.CollectionId, cancellationToken))
            return Result<Guid>.Failure(CuratedCollectionErrors.NotFound);

        if (!await productRepository.AnyAsync(product => product.Id == request.ProductId, cancellationToken))
            return Result<Guid>.Failure(CuratedCollectionErrors.ProductNotFound);

        if (await repository.ItemExistsAsync(request.CollectionId, request.ProductId, cancellationToken))
            return Result<Guid>.Failure(CuratedCollectionErrors.AlreadyInCollection);

        int sortOrder = request.SortOrder ?? await repository.GetNextSortOrderAsync(request.CollectionId, cancellationToken);

        CuratedCollectionItem curatedCollectionItem = new CuratedCollectionItem
        {
            CollectionId = request.CollectionId,
            ProductId = request.ProductId,
            SortOrder = sortOrder
        };

        await repository.AddItemAsync(curatedCollectionItem, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(curatedCollectionItem.Id);
    }
}

