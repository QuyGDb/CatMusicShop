using FluentValidation;
using MusicShop.Application.Common.Utils;

namespace MusicShop.Application.UseCases.System.Uploads.Commands.UploadImage;

public sealed class UploadImageCommandValidator : AbstractValidator<UploadImageCommand>
{
    public UploadImageCommandValidator()
    {
        RuleFor(command => command.FileStream)
            .NotNull().WithMessage("File stream is required.")
            .Must(stream => stream != null && stream.Length > 0).WithMessage("The uploaded file is empty.")
            .Must((command, stream) => 
            {
                string extension = Path.GetExtension(command.FileName);
                return FileSignatureHelper.IsValidImageSignature(stream, extension);
            }).WithMessage("File content does not match its image extension.");

        RuleFor(command => command.ContentType)
            .NotEmpty().WithMessage("Content type is required.")
            .Must(contentType => contentType != null && contentType.StartsWith("image/"))
            .WithMessage("File must be an image.");

        RuleFor(command => command.FileName)
            .NotEmpty().WithMessage("File name is required.")
            .Must(fileName =>
            {
                string extension = Path.GetExtension(fileName).ToLower();
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
                return allowedExtensions.Contains(extension);
            })
            .WithMessage("Invalid file extension. Only .jpg, .jpeg, .png, .webp, and .gif are allowed.");

        RuleFor(command => command.Folder)
            .NotEmpty().WithMessage("Folder name is required.");
    }
}
