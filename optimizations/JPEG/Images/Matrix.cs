using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace JPEG.Images;

public unsafe class Matrix : IDisposable
{
    private readonly Bitmap bitmap;
    private readonly BitmapData bmd;
    private readonly int depth;
    private readonly byte* firstPixelPtr;
    private readonly int stride;

    public Matrix(Bitmap bitmap, int width, int height)
    {
        this.bitmap = bitmap;
        Width = width;
        Height = height;
        bmd = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
        firstPixelPtr = (byte*)bmd.Scan0;
        stride = bmd.Stride;
        depth = Image.GetPixelFormatSize(bmd.PixelFormat) / 8;
    }

    public int Width { get; }
    public int Height { get; }

    public PixelRgb this[int y, int x]
    {
        get
        {
            var ptr = firstPixelPtr + y * stride + x * depth;
            var b = ptr[0];
            var g = ptr[1];
            var r = ptr[2];
            return new PixelRgb(r, g, b);
        }

        set
        {
            var ptr = firstPixelPtr + y * stride + x * depth;
            ptr[0] = ToByte(value.B);
            ptr[1] = ToByte(value.G);
            ptr[2] = ToByte(value.R);
        }
    }

    public void Dispose()
    {
        bitmap.UnlockBits(bmd);
        bitmap.Dispose();
    }

    public static Matrix FromBitmap(Bitmap bitmap)
    {
        var width = bitmap.Width - bitmap.Width % 8;
        var height = bitmap.Height - bitmap.Height % 8;
        return new Matrix(bitmap, width, height);
    }

    public static Bitmap ToBitmap(Matrix matrix) => matrix.bitmap;

    private static byte ToByte(double d)
    {
        var val = (int)d;
        return val switch
        {
            > byte.MaxValue => byte.MaxValue,
            < byte.MinValue => byte.MinValue,
            _ => (byte)val
        };
    }
}