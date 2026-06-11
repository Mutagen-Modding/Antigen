namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class ArrayExtensions
{
    /// <summary>
    /// Get the nth row of a row-major 2D array
    /// </summary>
    public static IEnumerable<T> GetRow<T>(this T[,] data, int rowIndex)
    {
        for (var i = 0; i < data.GetLength(0); i++)
        {
            yield return data[i, rowIndex];
        }
    }

    /// <summary>
    /// Get the nth column of a row-major 2D array
    /// </summary>
    public static IEnumerable<T> GetColumn<T>(this T[,] data, int columnIndex)
    {
        for (var i = 0; i < data.GetLength(0); i++)
        {
            yield return data[columnIndex, i];
        }
    }
}
