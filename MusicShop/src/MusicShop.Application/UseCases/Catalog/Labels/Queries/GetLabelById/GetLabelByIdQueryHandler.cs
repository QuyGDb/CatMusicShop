using MediatR;
using MusicShop.Application.DTOs.Catalog;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Domain.Errors;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.Common.Mappings;

namespace MusicShop.Application.UseCases.Catalog.Labels.Queries.GetLabelById;

public sealed class GetLabelByIdQueryHandler(ILabelRepository labelRepository)
    : IRequestHandler<GetLabelByIdQuery, Result<LabelResponse>>
{
    public async Task<Result<LabelResponse>> Handle(
        GetLabelByIdQuery request, 
        CancellationToken cancellationToken)
    {
        Label? label = await labelRepository.GetByIdAsync(request.Id, cancellationToken);

        if (label == null)
        {
            return Result<LabelResponse>.Failure(LabelErrors.NotFound);
        }

        return Result<LabelResponse>.Success(label.ToResponse());
    }
}

