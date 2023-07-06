using System.IO;
using System.Threading.Tasks;
using ImageMagick;
using SixLabors.ImageSharp;
using Size = System.Drawing.Size;

namespace LRReader.Shared.Services
{
	public abstract class ImageProcessingService
	{

		public abstract Task<object?> ByteToBitmap(byte[]? bytes, int decodeWidth = 0, int decodeHeight = 0, bool transcode = false, object? image = default);

		public virtual Task<Size> GetImageSize(byte[]? bytes)
		{
			return Task.Run(() =>
			{
				if (bytes == null)
					return Size.Empty;
				using var ms = new MemoryStream(bytes);
				try
				{
					var info = Image.Identify(ms);
					if (info != null)
						return new Size(info.Width, info.Height);
				}
				catch
				{
				}
				ms.Seek(0, SeekOrigin.Begin);
				try
				{
					using (var magick = new MagickImage())
					{
						magick.Ping(ms);
						return new Size(magick.Width, magick.Height);
					}
				}
				catch
				{
				}
				return Size.Empty;
			});
		}
	}
}
