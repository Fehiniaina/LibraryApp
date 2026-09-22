using System.ComponentModel.DataAnnotations;

namespace Playground.Mvc.Options;

public class OAuthClientOptions
{
    public const string SectionName = "OpenIddict";

    [Required]
    public required string ClientId { get; set; }

    [Required]
    public required string ClientSecret { get; set; }
}
