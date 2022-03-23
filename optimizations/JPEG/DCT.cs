using System;

namespace JPEG;

public class DCT
{
    private const int Size = 8;
    private static readonly double InverseSqrt2 = 1 / Math.Sqrt(2);
    private static readonly double[] CosBuffer = ComputeFastCos();
    
    private static double[] ComputeFastCos()
    {
        var cos = new double[Size * Size];
        for (var x = 0; x < Size; x++)
        for (var y = 0; y < Size; y++)
            cos[x * Size + y] = Math.Cos((2d * x + 1d) * y * Math.PI / (2 * Size));
        return cos;
    }
    
    public static void DCT2D(double[] input, double[] coefficients)
    {
        var beta = Beta(Size, Size);
        for (var v = 0; v < Size; v++)
        for (var u = 0; u < Size; u++)
        {
            var sum = 0d;
            for (var y = 0; y < Size; y++)
            for (var x = 0; x < Size; x++)
            {
                sum += input[x * Size + y] * CosBuffer[x  * Size + u] * CosBuffer[y * Size + v];
            }

            coefficients[u * Size + v] = sum * beta * Alpha(u) * Alpha(v);
        }
    }

    public static void IDCT2D(double[] coefficients, double[] output)
    {
        var width = Size;
        var height = Size;
        var beta = Beta(height, width);
        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            var sum = 0d;
            for (var v = 0; v < height; v++)
            for (var u = 0; u < width; u++)
            {
                sum += coefficients[u * Size + v] * CosBuffer[x *  Size + u] * CosBuffer[y * Size + v] * Alpha(u) * Alpha(v);
            }

            output[x * Size +  y] = sum * beta;
        }
    }

    private static double Alpha(int u) => u == 0 ? InverseSqrt2 : 1;

    private static double Beta(int height, int width) => 1d / width + 1d / height;
}