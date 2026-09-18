namespace Library.Domain.Common;

/// <summary>
/// Méthodes d'extension pour les collections <see cref="IEnumerable{T}"/>.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Retourne l'unique élément de la séquence, ou lève une exception si la séquence
    /// contient zéro ou plusieurs éléments.
    /// </summary>
    /// <typeparam name="T">Le type des éléments de la séquence.</typeparam>
    /// <param name="items">La séquence à inspecter.</param>
    /// <param name="errorMessage">Message de l'exception levée si la séquence ne contient pas exactement un élément.</param>
    /// <returns>L'unique élément de la séquence.</returns>
    /// <exception cref="ArgumentNullException">Levée si <paramref name="items"/> est null.</exception>
    /// <exception cref="InvalidOperationException">Levée si la séquence est vide ou contient plus d'un élément.</exception>
    /// <remarks>
    /// Contrairement à <see cref="Enumerable.Single{TSource}(IEnumerable{TSource})"/>,
    /// cette méthode permet de personnaliser le message d'erreur pour un contexte métier précis.
    /// L'énumérateur est parcouru manuellement pour éviter une double itération de la séquence.
    /// </remarks>
    /// <example>
    /// <code>
    /// var author = authors.GetSingleOrThrow("Aucun auteur trouvé pour cet ISBN.");
    /// </code>
    /// </example>
    public static T GetSingleOrThrow<T>(this IEnumerable<T> items, string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(items);

        using var enumerator = items.GetEnumerator();

        // Séquence vide — aucun élément trouvé
        if (!enumerator.MoveNext())
        {
            throw new InvalidOperationException(errorMessage);
        }

        var result = enumerator.Current;

        // Plus d'un élément — la séquence n'est pas unique
        if (enumerator.MoveNext())
        {
            throw new InvalidOperationException(errorMessage);
        }

        return result;
    }
}