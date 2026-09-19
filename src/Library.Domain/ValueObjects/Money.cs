namespace Library.Domain.ValueObjects;

public sealed record Money
{
    public Money(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Le montant ne peut pas être négatif.", nameof(amount));
        }

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
        {
            throw new ArgumentException("La devise doit être un code ISO à 3 lettres (ex: EUR).", nameof(currency));
        }

        this.Amount = amount;
        this.Currency = currency.ToUpperInvariant();
    }

    public decimal Amount { get; init; }

    public string Currency { get; init; }
}
