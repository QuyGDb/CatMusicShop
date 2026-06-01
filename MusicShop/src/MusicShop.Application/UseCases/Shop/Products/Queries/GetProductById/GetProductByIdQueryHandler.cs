using MediatR;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.Common.Mappings;
using MusicShop.Application.DTOs.Shop;
using MusicShop.Domain.Common;
using MusicShop.Domain.Errors;

namespace MusicShop.Application.UseCases.Shop.Products.Queries.GetProductById;

/// <summary>
/// Handler for GetProductByIdQuery. Uses IProductRepository.
/// </summary>
public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDetailDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductDetailDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.Shop.Product? product = await _productRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken);

        if (product == null)
        {
            return Result<ProductDetailDto>.Failure(ProductErrors.NotFound);
        }

        ProductDetailDto productDetailDto = product.ToDetailDto();

        return Result<ProductDetailDto>.Success(productDetailDto);
    }
}

