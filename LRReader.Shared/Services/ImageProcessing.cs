using System.Threading.Tasks;
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
				var info = Image.Identify(bytes);
				if (info != null)
					return new Size(info.Width, info.Height);
				return Size.Empty;
			});
		}
	}
}
