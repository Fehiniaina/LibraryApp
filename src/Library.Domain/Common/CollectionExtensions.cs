namespace Library.Domain.Common;

public static class CollectionExtensions
{
    public static T GetSingleOrThrow<T>(this IEnumerable<T> items, string errorMessage)
    {
        using var enumerator = items.GetEnumerator();

        if (!enumerator.MoveNext())
            throw new InvalidOperationException(errorMessage);

        var result = enumerator.Current;

        if (enumerator.MoveNext())
            throw new InvalidOperationException(errorMessage);

        return result;
    }
}