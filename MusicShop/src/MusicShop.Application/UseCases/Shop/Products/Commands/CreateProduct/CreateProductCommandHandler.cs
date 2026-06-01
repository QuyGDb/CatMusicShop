using MediatR;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Shop;
using MusicShop.Domain.Interfaces;
using MusicShop.Domain.Errors;

namespace MusicShop.Application.UseCases.Shop.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // 1. Check for duplicate slug
        bool slugExists = await productRepository.AnyAsync(product => product.Slug == request.Slug, cancellationToken);
        if (slugExists)
        {
            return Result<Guid>.Failure(ProductErrors.DuplicateSlug);
        }

        Product product = new()
        {
            ReleaseVersionId = request.ReleaseVersionId,
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            CoverUrl = request.CoverUrl,
            IsLimited = request.IsLimited,
            LimitedQty = request.LimitedQty,
            IsPreorder = request.IsPreorder,
            PreorderReleaseDate = request.PreorderReleaseDate,
            IsActive = true,
            Price = request.Price,
            StockQty = request.StockQty,
            IsSigned = request.IsSigned,
            VinylAttributes = request.VinylAttributes != null ? new VinylAttributes
            {
                DiscColor = request.VinylAttributes.DiscColor,
                WeightGrams = request.VinylAttributes.WeightGrams,
                SpeedRpm = request.VinylAttributes.SpeedRpm,
                DiscCount = request.VinylAttributes.DiscCount,
                SleeveType = request.VinylAttributes.SleeveType
            } : null,
            CdAttributes = request.CdAttributes != null ? new CdAttributes
            {
                Edition = request.CdAttributes.Edition,
                IsJapanEdition = request.CdAttributes.IsJapanEdition
            } : null,
            CassetteAttributes = request.CassetteAttributes != null ? new CassetteAttributes
            {
                TapeColor = request.CassetteAttributes.TapeColor,
                Edition = request.CassetteAttributes.Edition
            } : null
        };

        productRepository.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(product.Id);
    }
}

