#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using ImageMagick;
using LRReader.Shared.Services;
using SixLabors.ImageSharp;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Size = System.Drawing.Size;

namespace LRReader.UWP.Services
{
	public class UWPImageProcessingService : ImageProcessingService
	{
		private SemaphoreSlim semaphore;

		public UWPImageProcessingService()
		{
			semaphore = new SemaphoreSlim(Math.Max(Environment.ProcessorCount / 2, 1));
		}

		public override async Task<object?> ByteToBitmap(byte[]? bytes, int decodeWidth = 0, int decodeHeight = 0, bool transcode = false, object? img = default)
		{
			if (bytes == null)
				return null;
			if (bytes.Length == 0)
				return null;

			var image = img as BitmapImage ?? new BitmapImage();

			image.DecodePixelType = DecodePixelType.Logical;
			if (decodeWidth > 0)
				image.DecodePixelWidth = decodeWidth;
			if (decodeHeight > 0)
				image.DecodePixelHeight = decodeHeight;

			using var ms = new MemoryStream(bytes);
			var ras = ms.AsRandomAccessStream();
			var requiresIM = RequiresIM(bytes);
			if (transcode || requiresIM)
			{
				await semaphore.WaitAsync();
				try
				{
					if (requiresIM)
					{
						using var magick = new MagickImage();
						magick.Read(ms);
						using var pixels = magick.GetPixels();
						using (var converted = new InMemoryRandomAccessStream())
						{
							var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, converted);
							encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, magick.Width, magick.Height, 96, 96, pixels.ToByteArray(PixelMapping.BGRA));
							await encoder.FlushAsync();
							await image.SetSourceAsync(converted);
						}
						/*var wb = new WriteableBitmap((int)magick.Width, (int)magick.Height);
						var pixelsArray = pixels.ToByteArray(PixelMapping.BGRA)!;
						using (var stream = wb.PixelBuffer.AsStream())
							await stream.WriteAsync(pixelsArray, 0, pixelsArray.Length);
						return wb;*/
					}
					else
					{
						var decoder = await BitmapDecoder.CreateAsync(ras);
						using var softwareBitmap = await decoder.GetSoftwareBitmapAsync();
						SoftwareBitmap? newSource = null;
						if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
							newSource = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
						using (var converted = new InMemoryRandomAccessStream())
						{
							var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, converted);
							encoder.SetSoftwareBitmap(newSource ?? softwareBitmap);
							await encoder.FlushAsync();
							await image.SetSourceAsync(converted);
						}
						newSource?.Dispose();
					}
				}
				catch
				{
					return null;
				}
				finally
				{
					semaphore.Release();
				}
			}
			else
			{
				try
				{
					await image.SetSourceAsync(ras);
				}
				catch
				{
					return null;
				}
			}
			return image;
		}

		public override async Task<Size> GetImageSize(byte[]? bytes)
		{
			if (bytes == null)
				return Size.Empty;
			if (bytes.Length == 0)
				return Size.Empty;
			var size = await base.GetImageSize(bytes);
			if (size.IsEmpty)
			{
				using var ms = new MemoryStream(bytes);
				try
				{
					if (RequiresIM(bytes))
					{
						using var magick = new MagickImage();
						magick.Ping(ms);
						return new Size((int)magick.Width, (int)magick.Height);
					}
					else
					{
						var decoder = await BitmapDecoder.CreateAsync(ms.AsRandomAccessStream());
						return new Size((int)decoder.PixelWidth, (int)decoder.PixelHeight);
					}
				}
				catch
				{
					return new Size(0, 0);
				}
			}
			return size;
		}

		private bool RequiresIM(byte[] bytes)
		{
			var jxlCodestream = bytes[0] == 0xff && bytes[1] == 0x0A;
			var jxlContainer = bytes[0] == 0 && bytes[1] == 0 && bytes[2] == 0 && bytes[3] == 0xC && bytes[4] == 'J' && bytes[5] == 'X' && bytes[6] == 'L' && bytes[7] == ' ' && bytes[8] == 0xD && bytes[9] == 0xA && bytes[10] == 0x87 && bytes[11] == 0xA;

			return jxlCodestream || jxlContainer;
		}

	}
}
