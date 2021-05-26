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
	public class ImageProcessingService : IImageProcessingService
	{

		private SemaphoreSlim semaphore;

		public ImageProcessingService()
		{
			semaphore = new SemaphoreSlim(Math.Max(Environment.ProcessorCount / 2, 1));
		}

		public async Task<object> ByteToBitmap(byte[] bytes, object image = null, bool transcode = false)
		{
			if (bytes == null)
				return null;
			if (bytes.Length == 0)
				return null;
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
								if (image == null)
									image = new BitmapImage();
								(image as BitmapImage).SetSource(converted);
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
					if (image == null)
						image = new BitmapImage();
					try
					{
						await (image as BitmapImage).SetSourceAsync(stream);
					}
					catch (Exception)
					{
						return null;
					}
				}
			}
			return image;
		}

		public async Task<Size> GetImageSize(byte[] bytes)
		{
			if (bytes == null)
				return new Size(0, 0);
			if (bytes.Length == 0)
				return new Size(0, 0);
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
		}

	}
}
