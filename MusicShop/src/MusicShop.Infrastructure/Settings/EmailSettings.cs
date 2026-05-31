namespace MusicShop.Infrastructure.Settings;

public sealed record EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string Host { get; init; } = string.Empty;
    public int Port { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FromName { get; init; } = string.Empty;
}
