using MediatR;
using MusicShop.Application.Common;
using MusicShop.Application.DTOs.Catalog;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.Common.Mappings;

namespace MusicShop.Application.UseCases.Catalog.Labels.Queries.GetLabels;

public sealed class GetLabelsQueryHandler(ILabelRepository labelRepository)
    : IRequestHandler<GetLabelsQuery, Result<PaginatedResult<LabelResponse>>>
{
    public async Task<Result<PaginatedResult<LabelResponse>>> Handle(
        GetLabelsQuery request, 
        CancellationToken cancellationToken)
    {
        (IReadOnlyList<Label> labels, int totalCount) = await labelRepository.GetPagedAsync(request, cancellationToken);

        List<LabelResponse> labelResponses = labels.Select(label => label.ToResponse()).ToList();

        PaginatedResult<LabelResponse> result = new PaginatedResult<LabelResponse>(
            labelResponses, 
            totalCount, 
            request.PageNumber, 
            request.PageSize);

        return Result<PaginatedResult<LabelResponse>>.Success(result);
    }
}

