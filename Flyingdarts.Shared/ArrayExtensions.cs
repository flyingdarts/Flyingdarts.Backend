namespace Flyingdarts.Signalling.Shared;

public static class ArrayExtensions
{
    public static IEnumerable<IEnumerable<T>> Split<T>(this T[] array, int size)
    {
        int counter = array.Length > size
            ? (int)Math.Ceiling((double)(array.Length / size))
            : 1;

        for (var i = 0; i < counter; i++)
            yield return array.Skip(i * size).Take(size);
    }
}