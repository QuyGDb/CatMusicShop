using MediatR;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Shop;
using MusicShop.Domain.Errors;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Application.UseCases.Shop.CuratedCollections.Commands.CreateCuratedCollection;

public sealed class CreateCuratedCollectionCommandHandler(
    ICuratedCollectionRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCuratedCollectionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCuratedCollectionCommand request, CancellationToken cancellationToken)
    {
        if (await repository.AnyAsync(c => c.Title == request.Title, cancellationToken))
            return Result<Guid>.Failure(CuratedCollectionErrors.TitleAlreadyExists);

        CuratedCollection curatedCollection = new CuratedCollection
        {
            Title = request.Title,
            Description = request.Description,
            IsPublished = false
        };

        repository.Add(curatedCollection);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(curatedCollection.Id);
    }
}

