using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using JPEG.Images;

namespace JPEG
{
	class Program
	{
		private const int CompressionQuality = 70;
		private const int DCTSize = 8;

		private static void Main(string[] args)
		{
			try
			{
				Console.WriteLine(IntPtr.Size == 8 ? "64-bit version" : "32-bit version");
				var sw = Stopwatch.StartNew();

				const string fileName = @"sample.BMP";
//				var fileName = "Big_Black_River_Railroad_Bridge.bmp";
				var compressedFileName = fileName + ".compressed." + CompressionQuality;
				var uncompressedFileName = fileName + ".uncompressed." + CompressionQuality + ".bmp";

				using (var fileStream = File.OpenRead(fileName))
				using (var bmp = (Bitmap)Image.FromStream(fileStream, false, false))
				{
					var lmMaxtrix = Matrix.FromBitmap(bmp);
					sw.Stop();

					Console.WriteLine($"{bmp.Width}x{bmp.Height} - {fileStream.Length / (1024.0 * 1024):F2} MB");
					sw.Start();

					var compressionResult = Compress(lmMaxtrix, CompressionQuality);
					compressionResult.Save(compressedFileName);
				}

				#region decompress

				sw.Stop();
				Console.WriteLine("Compression: " + sw.ElapsedMilliseconds);
				sw.Restart();
				var compressedImage = CompressedImage.Load(compressedFileName);
				var uncompressedImage = Uncompress(compressedImage);
				var resultBmp = Matrix.ToBitmap(uncompressedImage);
				//var resultBmp = (Bitmap)uncompressedImage;
				resultBmp.Save(uncompressedFileName, ImageFormat.Bmp);
				Console.WriteLine("Decompression: " + sw.ElapsedMilliseconds);
				Console.WriteLine($"Peak commit size: {MemoryMeter.PeakPrivateBytes() / (1024.0 * 1024):F2} MB");
				Console.WriteLine($"Peak working set: {MemoryMeter.PeakWorkingSet() / (1024.0 * 1024):F2} MB");

				#endregion
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		private static Matrix Uncompress(CompressedImage image)
		{
			var bitmap = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);
			var matrix = new Matrix(bitmap, image.Width, image.Height);
			var uncompressors = new ConcurrentBag<Uncompressor>();
			var decoded = HuffmanCodec.Decode(image.CompressedBytes, image.DecodeTable, image.BitsCount);
			var pWidth = image.Width / DCTSize;
			var pHeight = image.Height / DCTSize;
			const int length = 3 * DCTSize * DCTSize;
			Parallel.For(0, pHeight * pWidth, n =>
			{
				var x = n % pWidth * DCTSize;
				var y = n / pWidth * DCTSize;
				if (!uncompressors.TryTake(out var uncompressor))
					uncompressor = new Uncompressor(DCTSize, image.Quality, decoded);
				uncompressor.Uncompress(n * length);
				SetPixels(matrix, uncompressor.Y, uncompressor.Cb, uncompressor.Cr, y, x);
				uncompressors.Add(uncompressor);
			});

			return matrix;
		}

		private static CompressedImage Compress(Matrix matrix, int quality = 50)
		{
			var matrixHeight = matrix.Height;
			var matrixWidth = matrix.Width;
			var pWidth = matrixWidth / DCTSize;
			var pHeight = matrixHeight / DCTSize;
			const int length = 3 * DCTSize * DCTSize;
			var compressors = new ConcurrentBag<Compressor>();

			var funcs = new Func<PixelRgb, double>[]
			{
				p => (65.738 * p.R + 129.057 * p.G + 24.064 * p.B) / 256.0 - 112,
				p => (-37.945 * p.R - 74.494 * p.G + 112.439 * p.B) / 256.0,
				p => (112.439 * p.R - 94.154 * p.G - 18.285 * p.B) / 256.0
			};
			var allQuantizedBytes = new byte[3 * matrixHeight * matrixWidth];
			Parallel.For(0, pWidth * pHeight, n =>
			{
				var x = n % pWidth * DCTSize;
				var y = n / pWidth * DCTSize;
				if (!compressors.TryTake(out var compressor))
					compressor = new Compressor(DCTSize, quality, allQuantizedBytes);
				compressor.Compress(matrix, y, x, length * n, funcs);
				compressors.Add(compressor);
			});

			#region save

			long bitsCount;
			Dictionary<BitsWithLength, byte> decodeTable;
			var compressedBytes = HuffmanCodec.Encode(
				allQuantizedBytes, out decodeTable, out bitsCount);

			return new CompressedImage
			{
				Quality = quality,
				CompressedBytes = compressedBytes,
				BitsCount = bitsCount,
				DecodeTable = decodeTable,
				Height = matrix.Height,
				Width = matrix.Width
			};

			#endregion
		}

		private static void SetPixels(Matrix matrix, double[] a, double[] b, double[] c, int yOffset,
			int xOffset)
		{
			var height = DCTSize;
			var width = DCTSize;
			for (var y = 0; y < height; y++)
			for (var x = 0; x < width; x++)
			{
				if (x + xOffset >= matrix.Width || y + yOffset >= matrix.Height)
					return;

				var Y = a[y * DCTSize + x];
				var Cb = b[y * DCTSize + x];
				var Cr = c[y * DCTSize + x];

				var R = (298.082 * Y + 408.583 * Cr) / 256.0 - 222.921;
				var G = (298.082 * Y - 100.291 * Cb - 208.120 * Cr) / 256.0 + 135.576;
				var B = (298.082 * Y + 516.412 * Cb) / 256.0 - 276.836;
				var pixelRgb = new PixelRgb(R, G, B);
				matrix[yOffset + y, xOffset + x] = pixelRgb;
			}
		}
	}
}
