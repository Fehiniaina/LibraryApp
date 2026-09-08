namespace Library.Infrastructure.ExternalServices;

public class ExternalStatusApiOptions
{
    public const string SectionName = "ExternalApis:StatusApi";

    public required string BaseAddress { get; set; }
    public int TimeoutSeconds { get; set; } = 10;
}