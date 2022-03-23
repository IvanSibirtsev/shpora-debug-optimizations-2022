using System;
using JPEG.Images;

namespace JPEG;

public class Compressor
{
    private const int CompressionQuality = 70;
    private static readonly int[] QuantMatrix = QuantizationMatrix.Get(CompressionQuality);
    private readonly byte[] allQuantizedBytes;
    private readonly int quality;
    private const int Size = 8;
    private const int SquareSize = Size * Size;
    private readonly byte[] bytes;
    private readonly double[] coefficients;
    private readonly double[] doubles;
    private readonly byte[] zigZag;

    public Compressor(int size, int quality, byte[] allQuantizedBytes)
    {
        this.quality = quality;
        this.allQuantizedBytes = allQuantizedBytes;
        doubles = new double[size * size];
        bytes = new byte[size * size];
        coefficients = new double[size * size];
        zigZag = new byte[size * size];
    }
    
    public void Compress(Matrix matrix, int y, int x, int offset, Func<PixelRgb, double>[] funcs)
    {
        var count = 0;
        foreach (var selector in funcs)
        {
            GetSubMatrix(matrix, y, Size, x, Size, selector, doubles);
            DCT.DCT2D(doubles, coefficients);
            Quantize(coefficients, quality, bytes);
            ZigZagScan(bytes, zigZag);
            var start = count * Size * Size + offset;
            for (var i = 0; i < zigZag.Length; i++)
                allQuantizedBytes[start + i] = zigZag[i];
            count++;
        }
    }

    private void GetSubMatrix(Matrix matrix, int yOffset, int yLength, int xOffset, int xLength,
        Func<PixelRgb, double> componentSelector, double[] buffer)
    {
        for (var j = 0; j < yLength; j++)
        for (var i = 0; i < xLength; i++)
        {
            var y = yOffset + j;
            var x = xOffset + i;
            buffer[j * Size + i] = componentSelector(matrix[y, x]);
        }
    }

    private void Quantize(double[] channelFreqs, int quality, byte[] buffer)
    {
        for (var i = 0; i < SquareSize; i++)
        {
            buffer[i] = (byte)(channelFreqs[i] / QuantMatrix[i]);
        }
    }

    private void ZigZagScan(byte[] channelFreqs, byte[] output)
    {
        #region optimizaNa200Mb

        output[0] = channelFreqs[0 *  Size + 0];
        output[1] = channelFreqs[0 *  Size + 1];
        output[2] = channelFreqs[1 *  Size + 0];
        output[3] = channelFreqs[2 *  Size + 0];
        output[4] = channelFreqs[1 *  Size + 1];
        output[5] = channelFreqs[0 *  Size + 2];
        output[6] = channelFreqs[0 *  Size + 3];
        output[7] = channelFreqs[1 *  Size + 2];
        output[8] = channelFreqs[2 *  Size + 1];
        output[9] = channelFreqs[3 *  Size + 0];
        output[10] = channelFreqs[4 * Size + 0];
        output[11] = channelFreqs[3 * Size + 1];
        output[12] = channelFreqs[2 * Size + 2];
        output[13] = channelFreqs[1 * Size + 3];
        output[14] = channelFreqs[0 * Size + 4];
        output[15] = channelFreqs[0 * Size + 5];
        output[16] = channelFreqs[1 * Size + 4];
        output[17] = channelFreqs[2 * Size + 3];
        output[18] = channelFreqs[3 * Size + 2];
        output[19] = channelFreqs[4 * Size + 1];
        output[20] = channelFreqs[5 * Size + 0];
        output[21] = channelFreqs[6 * Size + 0];
        output[22] = channelFreqs[5 * Size + 1];
        output[23] = channelFreqs[4 * Size + 2];
        output[24] = channelFreqs[3 * Size + 3];
        output[25] = channelFreqs[2 * Size + 4];
        output[26] = channelFreqs[1 * Size + 5];
        output[27] = channelFreqs[0 * Size + 6];
        output[28] = channelFreqs[0 * Size + 7];
        output[29] = channelFreqs[1 * Size + 6];
        output[30] = channelFreqs[2 * Size + 5];
        output[31] = channelFreqs[3 * Size + 4];
        output[32] = channelFreqs[4 * Size + 3];
        output[33] = channelFreqs[5 * Size + 2];
        output[34] = channelFreqs[6 * Size + 1];
        output[35] = channelFreqs[7 * Size + 0];
        output[36] = channelFreqs[7 * Size + 1];
        output[37] = channelFreqs[6 * Size + 2];
        output[38] = channelFreqs[5 * Size + 3];
        output[39] = channelFreqs[4 * Size + 4];
        output[40] = channelFreqs[3 * Size + 5];
        output[41] = channelFreqs[2 * Size + 6];
        output[42] = channelFreqs[1 * Size + 7];
        output[43] = channelFreqs[2 * Size + 7];
        output[44] = channelFreqs[3 * Size + 6];
        output[45] = channelFreqs[4 * Size + 5];
        output[46] = channelFreqs[5 * Size + 4];
        output[47] = channelFreqs[6 * Size + 3];
        output[48] = channelFreqs[7 * Size + 2];
        output[49] = channelFreqs[7 * Size + 3];
        output[50] = channelFreqs[6 * Size + 4];
        output[51] = channelFreqs[5 * Size + 5];
        output[52] = channelFreqs[4 * Size + 6];
        output[53] = channelFreqs[3 * Size + 7];
        output[54] = channelFreqs[4 * Size + 7];
        output[55] = channelFreqs[5 * Size + 6];
        output[56] = channelFreqs[6 * Size + 5];
        output[57] = channelFreqs[7 * Size + 4];
        output[58] = channelFreqs[7 * Size + 5];
        output[59] = channelFreqs[6 * Size + 6];
        output[60] = channelFreqs[5 * Size + 7];
        output[61] = channelFreqs[6 * Size + 7];
        output[62] = channelFreqs[7 * Size + 6];
        output[63] = channelFreqs[7 * Size + 7];

        #endregion
    }
}