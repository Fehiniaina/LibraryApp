namespace Library.Domain.Entities;

public class OutboxMessage
{
    public OutboxMessage(string type, string content)
    {
        Id = Guid.NewGuid();
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        OccurredAt = DateTime.UtcNow;
    }

    private OutboxMessage()
    {
    }

    public Guid Id { get; private set; }

    public string Type { get; private set; } = default!;

    public string Content { get; private set; } = default!;

    public DateTime OccurredAt { get; private set; }

    public DateTime? ProcessedAt { get; private set; }

    public string? Error { get; private set; }

    public void MarkAsProcessed() => ProcessedAt = DateTime.UtcNow;

    public void MarkAsFailed(string error) => Error = error;
}
