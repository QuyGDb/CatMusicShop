using MediatR;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Common;
using MusicShop.Domain.Errors;

using MusicShop.Domain.Interfaces;
namespace MusicShop.Application.UseCases.Shop.CuratedCollections.Commands.UpdateCuratedCollection;

public sealed class UpdateCuratedCollectionCommandHandler(
    ICuratedCollectionRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCuratedCollectionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateCuratedCollectionCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Shop.CuratedCollection? curatedCollection = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (curatedCollection == null)
            return Result<Guid>.Failure(CuratedCollectionErrors.NotFound);

        if (request.Title != null && request.Title != curatedCollection.Title)
        {
            if (await repository.AnyAsync(c => c.Title == request.Title && c.Id != request.Id, cancellationToken))
                return Result<Guid>.Failure(CuratedCollectionErrors.TitleAlreadyExists);

            curatedCollection.Title = request.Title;
        }

        if (request.Description != null)
            curatedCollection.Description = request.Description;

        if (request.IsPublished.HasValue)
            curatedCollection.IsPublished = request.IsPublished.Value;

        repository.Update(curatedCollection);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(curatedCollection.Id);
    }
}

