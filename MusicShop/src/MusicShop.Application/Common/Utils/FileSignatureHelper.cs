namespace MusicShop.Application.Common.Utils;

public static class FileSignatureHelper
{
    private static readonly Dictionary<string, List<byte[]>> _fileSignatures = new()
    {
        { ".jpeg", new List<byte[]> { new byte[] { 0xFF, 0xD8 } } },
        { ".jpg", new List<byte[]> { new byte[] { 0xFF, 0xD8 } } },
        { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
        { ".gif", new List<byte[]> { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
        { ".webp", new List<byte[]> { new byte[] { 0x52, 0x49, 0x46, 0x46 } } } // WebP also starts with RIFF
    };

    public static bool IsValidImageSignature(Stream stream, string extension)
    {
        if (stream == null || stream.Length == 0 || string.IsNullOrEmpty(extension))
        {
            return false;
        }

        extension = extension.ToLower();
        if (!_fileSignatures.ContainsKey(extension))
        {
            return false;
        }

        long originalPosition = stream.Position;
        try
        {
            stream.Position = 0;
            using BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.UTF8, true);
            
            List<byte[]> signatures = _fileSignatures[extension];
            int maxSignatureLength = signatures.Max(s => s.Length);
            byte[] headerBytes = reader.ReadBytes(maxSignatureLength);

            return signatures.Any(signature => 
                headerBytes.Take(signature.Length).SequenceEqual(signature));
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }
}
