using System.ComponentModel.DataAnnotations;

namespace MusicShop.Infrastructure.Security;

public sealed class AdminSettings
{
    public const string SectionName = "AdminSettings";

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;
}

