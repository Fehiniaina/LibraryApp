namespace Library.Domain.ValueObjects;

/// <summary>
/// Représente un prix avec un montant et une devise ISO 4217.
/// </summary>
public sealed record Price
{
    // ─── Constructeurs ────────────────────────────────────────────────────────

    /// <summary>
    /// Initializes a new instance of the <see cref="Price"/> class.
    /// </summary>
    /// <param name="amount">Montant du prix, doit être positif ou nul.</param>
    /// <param name="currency">Code devise ISO 4217 à 3 lettres (ex: EUR, USD).</param>
    /// <exception cref="ArgumentException">Levée si le montant est négatif ou la devise invalide.</exception>
    public Price(decimal amount, string currency)
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

    // ─── Propriétés publiques ─────────────────────────────────────────────────

    /// <summary>
    /// Gets le montant du prix.
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Gets le code devise ISO 4217.
    /// </summary>
    public string Currency { get; init; }

    // ─── Factory methods ──────────────────────────────────────────────────────

    /// <summary>
    /// Crée un prix à zéro pour la devise spécifiée.
    /// </summary>
    /// <param name="currency">Code devise ISO 4217 à 3 lettres.</param>
    /// <returns>Un <see cref="Price"/> avec un montant de zéro.</returns>
    public static Price Zero(string currency) => new (0, currency);

    // ─── Méthodes publiques ───────────────────────────────────────────────────

    /// <summary>
    /// Additionne deux prix de même devise.
    /// </summary>
    /// <param name="other">Le prix à additionner.</param>
    /// <returns>Un nouveau <see cref="Price"/> avec le montant combiné.</returns>
    /// <exception cref="InvalidOperationException">Levée si les devises sont différentes.</exception>
    public Price Add(Price other)
    {
        if (this.Currency != other.Currency)
        {
            throw new InvalidOperationException("Impossible d'additionner des devises différentes.");
        }

        return this with { Amount = this.Amount + other.Amount };
    }
}