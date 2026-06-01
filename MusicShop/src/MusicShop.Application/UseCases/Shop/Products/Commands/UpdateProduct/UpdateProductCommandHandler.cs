using MediatR;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Common;
using MusicShop.Domain.Errors;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Application.UseCases.Shop.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateProductCommand, Result>
{
    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Shop.Product? product = await productRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken);

        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound);
        }

        // Business rule: LimitedQty cannot be increased after orders have been placed
        if (request.LimitedQty.HasValue && request.LimitedQty > product.LimitedQty)
        {
            bool hasOrders = await productRepository.HasOrdersAsync(request.Id, cancellationToken);
            if (hasOrders)
            {
                return Result.Failure(ProductErrors.LimitedQtyLocked);
            }
        }

        if (request.Name is not null) product.Name = request.Name;
        
        if (request.Slug is not null && request.Slug != product.Slug)
        {
            bool slugExists = await productRepository.AnyAsync(productItem => productItem.Slug == request.Slug && productItem.Id != request.Id, cancellationToken);
            if (slugExists)
            {
                return Result.Failure(ProductErrors.DuplicateSlug);
            }
            product.Slug = request.Slug;
        }

        if (request.Description is not null) product.Description = request.Description;
        if (request.IsActive.HasValue) product.IsActive = request.IsActive.Value;
        if (request.IsPreorder.HasValue) product.IsPreorder = request.IsPreorder.Value;
        if (request.PreorderReleaseDate.HasValue) product.PreorderReleaseDate = request.PreorderReleaseDate;
        if (request.LimitedQty.HasValue) product.LimitedQty = request.LimitedQty;
        
        if (request.Price.HasValue) product.Price = request.Price.Value;
        if (request.StockQty.HasValue) product.StockQty = request.StockQty.Value;
        if (request.IsSigned.HasValue) product.IsSigned = request.IsSigned.Value;

        // Update Attributes
        if (request.VinylAttributes != null && product.VinylAttributes != null)
        {
            product.VinylAttributes.DiscColor = request.VinylAttributes.DiscColor;
            product.VinylAttributes.WeightGrams = request.VinylAttributes.WeightGrams;
            product.VinylAttributes.SpeedRpm = request.VinylAttributes.SpeedRpm;
            product.VinylAttributes.DiscCount = request.VinylAttributes.DiscCount;
            product.VinylAttributes.SleeveType = request.VinylAttributes.SleeveType;
        }

        if (request.CdAttributes != null && product.CdAttributes != null)
        {
            product.CdAttributes.Edition = request.CdAttributes.Edition;
            product.CdAttributes.IsJapanEdition = request.CdAttributes.IsJapanEdition;
        }

        if (request.CassetteAttributes != null && product.CassetteAttributes != null)
        {
            product.CassetteAttributes.TapeColor = request.CassetteAttributes.TapeColor;
            product.CassetteAttributes.Edition = request.CassetteAttributes.Edition;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

