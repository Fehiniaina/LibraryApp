using System.ComponentModel.DataAnnotations;

namespace Library.Api.Options;

#pragma warning disable CA1515 // Consider making public types internal
public class OAuthClientOptions
#pragma warning restore CA1515 // Consider making public types internal
{
    public const string SectionName = "OpenIdDict";

    [Required]
    public required string ClientId { get; set; }

    [Required]
    public required string ClientSecret { get; set; }
}
