using System.ComponentModel.DataAnnotations;

namespace Playground.Mvc.Options;

public class LibraryApiOptions
{
    public const string SectionName = "LibraryApi";

    [Required, Url]
    public required string BaseAddress { get; set; }

    [Range(1, 60)]
    public int TimeoutSeconds { get; set; } = 10;
}
