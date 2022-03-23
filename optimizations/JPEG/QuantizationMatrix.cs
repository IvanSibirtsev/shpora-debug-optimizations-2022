using System;
using System.Collections.Concurrent;

namespace JPEG;

public static class QuantizationMatrix
{
    private static readonly ConcurrentDictionary<int, int[]> Cache = new();
    private const int Size = 8;

    public static int[] Get(int quality)
    {
        if (quality < 1 || quality > 99)
            throw new ArgumentException("quality must be in [1,99] interval");
        if (Cache.TryGetValue(quality, out var matrix))
            return matrix;
        Cache[quality] = Calculate(quality);
        return Cache[quality];
    }

    private static int[] Calculate(int quality)
    {
        var multiplier = quality < 50 ? 5000 / quality : 200 - 2 * quality;

        var result = new[]
        { 
            16, 11, 10, 16, 24, 40, 51, 61, 
            12, 12, 14, 19, 26, 58, 60, 55, 
            14, 13, 16, 24, 40, 57, 69, 56,
            14, 17, 22, 29, 51, 87, 80, 62, 
            18, 22, 37, 56, 68, 109, 103, 77, 
            24, 35, 55, 64, 81, 104, 113, 92,
            49, 64, 78, 87, 103, 121, 120, 101, 
            72, 92, 95, 98, 112, 100, 103, 99 
        };

        for (var i = 0; i < Size * Size; i++)
        {
            result[i] = (multiplier * result[i] + 50) / 100;
        }

        return result;
    }
}