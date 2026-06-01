using MediatR;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Common;
using System.Threading;
using System.Threading.Tasks;

namespace MusicShop.Application.UseCases.System.Uploads.Commands.UploadImage;

public sealed class UploadImageCommandHandler(IImageService imageService) 
    : IRequestHandler<UploadImageCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UploadImageCommand request, CancellationToken cancellationToken)
    {
        string url = await imageService.UploadImageAsync(
            request.FileStream,
            request.FileName,
            request.ContentType,
            request.Folder,
            cancellationToken);

        return Result<string>.Success(url);
    }
}

