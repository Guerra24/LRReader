#nullable enable
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ImageMagick;
using LRReader.Shared.Services;
using SixLabors.ImageSharp;
using Windows.Graphics.Imaging;
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

			IImageInfo? info = null;
			try
			{
				info = await Image.IdentifyAsync(ms);
			}
			catch
			{
			}
			ms.Seek(0, SeekOrigin.Begin);
			if (transcode || info == null)
			{
				await semaphore.WaitAsync();
				try
				{
					using var converted = await Task.Run(() =>
					{
						using var magick = new MagickImage();
						magick.Read(ms);
						magick.Format = MagickFormat.Png00;
						var converted = new MemoryStream();
						magick.Write(converted);
						converted.Seek(0, SeekOrigin.Begin);
						return converted;

					});
					await image.SetSourceAsync(converted.AsRandomAccessStream());
				}
				catch (Exception)
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
					await image.SetSourceAsync(ms.AsRandomAccessStream());
				}
				catch (Exception)
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
				using (var ms = new MemoryStream(bytes))
				{
					try
					{
						var decoder = await BitmapDecoder.CreateAsync(ms.AsRandomAccessStream());
						return new Size((int)decoder.PixelWidth, (int)decoder.PixelHeight);
					}
					catch
					{
						return new Size(0, 0);
					}
				}
			}
			return size;
		}

	}
}
