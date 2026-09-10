namespace Library.Domain.Interfaces;

public interface IExternalStatusClient
{
    Task<bool> CheckStatusAsync(CancellationToken ct);
}