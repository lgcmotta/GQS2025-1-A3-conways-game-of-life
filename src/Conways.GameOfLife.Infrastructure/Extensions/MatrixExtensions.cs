using Conways.GameOfLife.Domain;

namespace Conways.GameOfLife.Infrastructure.Extensions;

public static class MatrixExtensions
{
    public static T[,] ToTwoDimensionalArray<T>(this T[][] array)
    {
        var rows = array.Length;

        var columns = array[0].Length;

        var multiArray = new T[rows, columns];

        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < columns; j++)
            {
                multiArray[i, j] = array[i][j];
            }
        }

        return multiArray;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static T[][] ToMatrix<T>(this T[,] array)
    {
        var rows = array.GetLength(0);

        var columns = array.GetLength(1);

        var matrix = new T[rows][];

        for (var i = 0; i < rows; i++)
        {
            matrix[i] = new T[columns];

            for (var j = 0; j < columns; j++)
            {
                matrix[i][j] = array[i, j];
            }
        }

        return matrix;
    }

    public static bool[][] ToMatrix(this Generation generation)
    {
        return ToMatrix<bool>(generation);
    }
}