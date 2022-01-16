#nullable enable
using LRReader.Shared.Services;
using System;
using System.Drawing;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

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
			using (var stream = new InMemoryRandomAccessStream())
			{
				await stream.WriteAsync(bytes.AsBuffer());
				stream.Seek(0);
				if (transcode)
				{
					await semaphore.WaitAsync();
					try
					{
						SoftwareBitmap softwareBitmap;
						var decoder = await BitmapDecoder.CreateAsync(stream);
						using (softwareBitmap = await decoder.GetSoftwareBitmapAsync())
						{
							if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
								softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
							using (var converted = new InMemoryRandomAccessStream())
							{
								var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, converted);
								encoder.SetSoftwareBitmap(softwareBitmap);
								await encoder.FlushAsync();
								await image.SetSourceAsync(converted);
							}
						}
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
						await (image as BitmapImage)?.SetSourceAsync(stream);
					}
					catch (Exception)
					{
						return null;
					}
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
				using (var stream = new InMemoryRandomAccessStream())
				{
					await stream.WriteAsync(bytes.AsBuffer());
					stream.Seek(0);
					try
					{
						var decoder = await BitmapDecoder.CreateAsync(stream);
						return new Size((int)decoder.PixelWidth, (int)decoder.PixelHeight);
					}
					catch (Exception)
					{
						return new Size(0, 0);
					}
				}
			return size;
		}

	}
}
