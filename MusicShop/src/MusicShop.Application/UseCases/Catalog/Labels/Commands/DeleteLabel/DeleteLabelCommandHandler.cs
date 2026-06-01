using MediatR;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Interfaces;
using MusicShop.Domain.Errors;

namespace MusicShop.Application.UseCases.Catalog.Labels.Commands.DeleteLabel;

public sealed class DeleteLabelCommandHandler(
    ILabelRepository labelRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteLabelCommand, Result>
{
    public async Task<Result> Handle(
        DeleteLabelCommand request, 
        CancellationToken cancellationToken)
    {
        Label? label = await labelRepository.GetWithVersionsAsync(request.Id, cancellationToken);

        if (label is null)
        {
            return Result.Failure(LabelErrors.NotFound);
        }

        if (label.ReleaseVersions.Count > 0)
        {
            return Result.Failure(LabelErrors.HasAssociations);
        }

        labelRepository.Delete(label);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

