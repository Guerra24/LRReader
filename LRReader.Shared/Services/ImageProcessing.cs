using SixLabors.ImageSharp;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Size = System.Drawing.Size;

namespace LRReader.Shared.Services
{
	public abstract class ImageProcessingService : IService
	{

		public abstract Task Init();

		public abstract Task<object?> ByteToBitmap(byte[]? bytes, int decodeWidth = 0, int decodeHeight = 0, object? image = default, CancellationToken cancellationToken = default);

		public virtual async Task<Size> GetImageSize(byte[]? bytes)
		{
			if (bytes == null)
				return Size.Empty;
			using var ms = new MemoryStream(bytes, false);
			try
			{
				var info = await Image.IdentifyAsync(ms).ConfigureAwait(false);
				if (info != null)
					return new Size(info.Width, info.Height);
			}
			catch
			{
			}
			return Size.Empty;
		}
	}
}
