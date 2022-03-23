using System;
using System.Collections.Generic;

namespace JPEG;

public class Uncompressor
{
    private readonly double[][] channels;
    private readonly double[] coefficients;
    private readonly byte[] decoded;
    private readonly int quality;
    private const int Size = 8;
    private const int SquareSize = Size * Size;
    private readonly byte[] zigZag;
    
    public double[] Y { get; }
    public double[] Cb { get; }
    public double[] Cr { get; }

    public Uncompressor(int size, int quality, byte[] decoded)
    {
        this.quality = quality;
        this.decoded = decoded;
        Y = new double[size * size];
        Cb = new double[size * size];
        Cr = new double[size * size];
        channels = new[] { Y, Cb, Cr };
        coefficients = new double[size * size];
        zigZag = new byte[size * size];
    }
    
    public void Uncompress(int offset)
    {
        for (var index = channels.Length - 1; index >= 0; index--)
        {
            var channel = channels[index];
            var start = index * Size * Size + offset;
            var quantizedBytes = new ArraySegment<byte>(decoded, start, Size * Size);
            ZigZagUnScan(quantizedBytes, zigZag);
            DeQuantize(zigZag, quality, coefficients);
            DCT.IDCT2D(coefficients, channel);
            ShiftMatrixValues(channel, 128);
        }
    }

    private static void ShiftMatrixValues(double[] subMatrix, int shiftValue)
    {
        for (var i = 0; i < SquareSize; i++)
            subMatrix[i] += shiftValue;
    }

    private void DeQuantize(byte[] quantizedBytes, int quality, double[] buffer)
    {
        var quantizationMatrix = QuantizationMatrix.Get(quality);

        for (var i = 0; i < SquareSize; i++)
        {
            buffer[i] = (sbyte)quantizedBytes[i] * quantizationMatrix[i]; //NOTE cast to sbyte not to loose negative numbers
        }
    }

    private void ZigZagUnScan(IReadOnlyList<byte> quantizedBytes, byte[] output)
    {
        #region optimazaNa408Mb

       output[0 * Size +  0] = quantizedBytes[0];
        output[0 * Size +  1] = quantizedBytes[1];
        output[0 * Size +  2] = quantizedBytes[5];
        output[0 * Size +  3] = quantizedBytes[6];
        output[0 * Size +  4] = quantizedBytes[14];
        output[0 * Size +  5] = quantizedBytes[15];
        output[0 * Size +  6] = quantizedBytes[27];
        output[0 * Size +  7] = quantizedBytes[28];
        output[1 * Size + 0] = quantizedBytes[2];
        output[1 * Size + 1] = quantizedBytes[4];
        output[1 * Size + 2] = quantizedBytes[7];
        output[1 * Size + 3] = quantizedBytes[13];
        output[1 * Size + 4] = quantizedBytes[16];
        output[1 * Size + 5] = quantizedBytes[26];
        output[1 * Size + 6] = quantizedBytes[29];
        output[1 * Size + 7] = quantizedBytes[42];
        output[2 * Size + 0] = quantizedBytes[3];
        output[2 * Size + 1] = quantizedBytes[8];
        output[2 * Size + 2] = quantizedBytes[12];
        output[2 * Size + 3] = quantizedBytes[17];
        output[2 * Size + 4] = quantizedBytes[25];
        output[2 * Size + 5] = quantizedBytes[30];
        output[2 * Size + 6] = quantizedBytes[41];
        output[2 * Size + 7] = quantizedBytes[43];
        output[3 * Size + 0] = quantizedBytes[9];
        output[3 * Size + 1] = quantizedBytes[11];
        output[3 * Size + 2] = quantizedBytes[18];
        output[3 * Size + 3] = quantizedBytes[24];
        output[3 * Size + 4] = quantizedBytes[31];
        output[3 * Size + 5] = quantizedBytes[40];
        output[3 * Size + 6] = quantizedBytes[44];
        output[3 * Size + 7] = quantizedBytes[53];
        output[4 * Size + 0] = quantizedBytes[10];
        output[4 * Size + 1] = quantizedBytes[19];
        output[4 * Size + 2] = quantizedBytes[23];
        output[4 * Size + 3] = quantizedBytes[32];
        output[4 * Size + 4] = quantizedBytes[39];
        output[4 * Size + 5] = quantizedBytes[45];
        output[4 * Size + 6] = quantizedBytes[52];
        output[4 * Size + 7] = quantizedBytes[54];
        output[5 * Size + 0] = quantizedBytes[20];
        output[5 * Size + 1] = quantizedBytes[22];
        output[5 * Size + 2] = quantizedBytes[33];
        output[5 * Size + 3] = quantizedBytes[38];
        output[5 * Size + 4] = quantizedBytes[46];
        output[5 * Size + 5] = quantizedBytes[51];
        output[5 * Size + 6] = quantizedBytes[55];
        output[5 * Size + 7] = quantizedBytes[60];
        output[6 * Size + 0] = quantizedBytes[21];
        output[6 * Size + 1] = quantizedBytes[34];
        output[6 * Size + 2] = quantizedBytes[37];
        output[6 * Size + 3] = quantizedBytes[47];
        output[6 * Size + 4] = quantizedBytes[50];
        output[6 * Size + 5] = quantizedBytes[56];
        output[6 * Size + 6] = quantizedBytes[59];
        output[6 * Size + 7] = quantizedBytes[61];
        output[7 * Size + 0] = quantizedBytes[35];
        output[7 * Size + 1] = quantizedBytes[36];
        output[7 * Size + 2] = quantizedBytes[48];
        output[7 * Size + 3] = quantizedBytes[49];
        output[7 * Size + 4] = quantizedBytes[57];
        output[7 * Size + 5] = quantizedBytes[58];
        output[7 * Size + 6] = quantizedBytes[62];
        output[7 * Size + 7] = quantizedBytes[63];

        #endregion
    }
}