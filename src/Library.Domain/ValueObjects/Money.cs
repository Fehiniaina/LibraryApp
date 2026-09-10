using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Domain.ValueObjects
{
    public sealed record Money(decimal Amount, string Currency)
    {
        public decimal Amount { get; init; } = Amount >= 0
        ? Amount
        : throw new ArgumentException("Le montant ne peut pas être négatif.", nameof(Amount));

        public string Currency { get; init; } = !string.IsNullOrWhiteSpace(Currency) && Currency.Length == 3
            ? Currency
            : throw new ArgumentException("La devise doit être un code ISO à 3 lettres (ex: EUR).", nameof(Currency));
    }
}
