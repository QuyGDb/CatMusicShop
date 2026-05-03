using MusicShop.Application.DTOs.Shop;
using MusicShop.Domain.Entities.Shop;
using MusicShop.Domain.Enums;

namespace MusicShop.Application.Common.Mappings;

public static class ProductMapping
{
    public static ProductListItemDto ToListItemDto(this Product product)
    {
        return new ProductListItemDto
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            ArtistName = product.ReleaseVersion?.Release?.Artist?.Name,
            Format = product.ReleaseVersion != null ? product.ReleaseVersion.Format : ReleaseFormat.Vinyl,
            IsLimited = product.IsLimited,
            IsPreorder = product.IsPreorder,
            CoverUrl = product.CoverUrl,
            Price = product.Price,
            StockQty = product.StockQty,
            InStock = product.StockQty > 0 && product.IsAvailable
        };
    }

    /// <summary>
    /// Maps a Product entity to ProductDetailDto
    /// </summary>
    public static ProductDetailDto ToDetailDto(this Product product)
    {
        return new ProductDetailDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Format = product.ReleaseVersion != null ? product.ReleaseVersion.Format : ReleaseFormat.Vinyl,
            IsLimited = product.IsLimited,
            LimitedQty = product.LimitedQty,
            IsPreorder = product.IsPreorder,
            PreorderReleaseDate = product.PreorderReleaseDate,
            CoverUrl = product.CoverUrl,
            Artist = new ArtistShortDto
            {
                Id = product.ReleaseVersion?.Release?.ArtistId ?? Guid.Empty,
                Name = product.ReleaseVersion?.Release?.Artist?.Name ?? string.Empty
            },
            Price = product.Price,
            StockQty = product.StockQty,
            IsAvailable = product.IsAvailable,
            IsSigned = product.IsSigned,
            VinylAttributes = product.VinylAttributes != null ? new VinylAttributesDto(
                product.VinylAttributes.DiscColor,
                product.VinylAttributes.WeightGrams,
                product.VinylAttributes.SpeedRpm,
                product.VinylAttributes.DiscCount,
                product.VinylAttributes.SleeveType) : null,
            CdAttributes = product.CdAttributes != null ? new CdAttributesDto(
                product.CdAttributes.Edition,
                product.CdAttributes.IsJapanEdition) : null,
            CassetteAttributes = product.CassetteAttributes != null ? new CassetteAttributesDto(
                product.CassetteAttributes.TapeColor,
                product.CassetteAttributes.Edition) : null
        };
    }
}
