using MediatR;
using MusicShop.Application.Common;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.DTOs.Shop;
using MusicShop.Domain.Common;

namespace MusicShop.Application.UseCases.Shop.CuratedCollections.Queries.GetCuratedCollections;

public sealed class GetCuratedCollectionsQueryHandler(
    ICuratedCollectionRepository repository) : IRequestHandler<GetCuratedCollectionsQuery, Result<PaginatedResult<CuratedCollectionResponse>>>
{
    public async Task<Result<PaginatedResult<CuratedCollectionResponse>>> Handle(GetCuratedCollectionsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Domain.Entities.Shop.CuratedCollection> curatedCollections = request.IncludeUnpublished 
            ? await repository.GetAllAsync(cancellationToken)
            : await repository.GetPublishedAsync(cancellationToken);

        // Apply Search Filter
        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            curatedCollections = curatedCollections
                .Where(c => c.Title.Contains(request.SearchQuery, StringComparison.OrdinalIgnoreCase) || 
                            (c.Description != null && c.Description.Contains(request.SearchQuery, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        int totalCount = curatedCollections.Count;
        
        // Apply Pagination
        var paginatedItems = curatedCollections
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(curatedCollection => new CuratedCollectionResponse(
                curatedCollection.Id,
                curatedCollection.Title,
                curatedCollection.Description,
                curatedCollection.IsPublished,
                curatedCollection.Items.Count))
            .ToList();

        var result = new PaginatedResult<CuratedCollectionResponse>(
            paginatedItems,
            request.PageNumber,
            request.PageSize,
            totalCount);

        return Result<PaginatedResult<CuratedCollectionResponse>>.Success(result);
    }
}

