namespace JPEG.Images;
public class PixelRgb
{
    public double R { get; }
    public double G { get; }
    public double B { get; }

    public PixelRgb(double r, double g, double b)
    {
        R = r;
        G = g;
        B = b;
    }
}

public class PixelYCbCr
{
    public double Y { get; }
    public double Cb { get; }
    public double Cr { get; }

    public PixelYCbCr(double y, double cb, double cr)
    {
        Y = y;
        Cb = cb;
        Cr = cr;
    }
}