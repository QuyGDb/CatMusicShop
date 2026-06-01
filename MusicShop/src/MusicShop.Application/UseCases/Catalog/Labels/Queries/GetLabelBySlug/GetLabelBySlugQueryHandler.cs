using MediatR;
using MusicShop.Application.DTOs.Catalog;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Domain.Errors;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.Common.Mappings;

namespace MusicShop.Application.UseCases.Catalog.Labels.Queries.GetLabelBySlug;

public sealed class GetLabelBySlugQueryHandler(ILabelRepository labelRepository)
    : IRequestHandler<GetLabelBySlugQuery, Result<LabelResponse>>
{
    public async Task<Result<LabelResponse>> Handle(
        GetLabelBySlugQuery request, 
        CancellationToken cancellationToken)
    {
        Label? label = await labelRepository.GetBySlugAsync(request.Slug, cancellationToken);

        if (label == null)
        {
            return Result<LabelResponse>.Failure(LabelErrors.NotFound);
        }

        return Result<LabelResponse>.Success(label.ToResponse());
    }
}

