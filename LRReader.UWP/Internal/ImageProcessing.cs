using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace LRReader.UWP.Internal
{
	public class ImageProcessing
	{

		private SemaphoreSlim semaphore;

		public ImageProcessing()
		{
			semaphore = new SemaphoreSlim(Math.Max(Environment.ProcessorCount / 2, 1));
		}

		public async Task<BitmapImage> ByteToBitmap(byte[] bytes, BitmapImage image = null, bool transcode = false)
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
								image.SetSource(converted);
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
					await image.SetSourceAsync(stream);
				}
			}
			return image;
		}
	}
}
