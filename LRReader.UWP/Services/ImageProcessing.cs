#nullable enable
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LRReader.Shared.Services;
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
			using (var ms = new MemoryStream(bytes))
			{
				var stream = ms.AsRandomAccessStream();
				//stream.Seek(0);
				if (transcode)
				{
					await semaphore.WaitAsync();
					try
					{
						var decoder = await BitmapDecoder.CreateAsync(stream);
						using (var softwareBitmap = await decoder.GetSoftwareBitmapAsync())
						{
							SoftwareBitmap? newSource = null;
							if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
								newSource = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
							using (var converted = new InMemoryRandomAccessStream())
							{
								var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, converted);
								encoder.SetSoftwareBitmap(newSource ?? softwareBitmap);
								await encoder.FlushAsync();
								await image.SetSourceAsync(converted);
							}
							newSource?.Dispose();
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
						await image.SetSourceAsync(stream);
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
				using (var ms = new MemoryStream(bytes))
				{
					var stream = ms.AsRandomAccessStream();
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
