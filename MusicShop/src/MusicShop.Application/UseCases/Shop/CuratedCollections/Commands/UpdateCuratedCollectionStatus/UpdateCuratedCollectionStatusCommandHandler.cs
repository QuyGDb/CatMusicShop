using MediatR;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Common;
using MusicShop.Domain.Errors;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Application.UseCases.Shop.CuratedCollections.Commands.UpdateCuratedCollectionStatus;

public sealed class UpdateCuratedCollectionStatusCommandHandler(
    ICuratedCollectionRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCuratedCollectionStatusCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UpdateCuratedCollectionStatusCommand request, CancellationToken cancellationToken)
    {
        var curatedCollection = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (curatedCollection == null)
            return Result<Unit>.Failure(CuratedCollectionErrors.NotFound);

        curatedCollection.IsPublished = request.IsPublished;

        repository.Update(curatedCollection);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}

