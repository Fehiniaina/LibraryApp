// src/Library.Domain/ValueObjects/Price.cs
namespace Library.Domain.ValueObjects;

public sealed record Price(decimal Amount, string Currency)
{
    public decimal Amount { get; init; } = Amount >= 0
        ? Amount
        : throw new ArgumentException("Le montant ne peut pas être négatif.", nameof(Amount));

    public string Currency { get; init; } = !string.IsNullOrWhiteSpace(Currency) && Currency.Length == 3
        ? Currency
        : throw new ArgumentException("La devise doit être un code ISO à 3 lettres (ex: EUR).", nameof(Currency));

    public static Price Zero(string currency) => new(0, currency);

    public Price Add(Price other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Impossible d'additionner des devises différentes.");
        return this with { Amount = Amount + other.Amount };
    }
}