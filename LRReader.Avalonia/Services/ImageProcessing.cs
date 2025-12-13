using Avalonia.Media.Imaging;
using LRReader.Shared.Services;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LRReader.Avalonia.Services
{
	public class AvaloniaImageProcessingService : ImageProcessingService
	{

		public override Task<object?> ByteToBitmap(byte[]? bytes, int decodeWidth = 0, int decodeHeight = 0, object? image = default, CancellationToken cancellationToken = default)
		{
			return Task.Run(() =>
			{
				if (image is Bitmap bitmap)
					bitmap.Dispose();
				if (bytes == null)
					return null;
				using (var stream = new MemoryStream(bytes))
				{
					if (decodeWidth != 0)
						return Bitmap.DecodeToWidth(stream, decodeWidth);
					if (decodeHeight != 0)
						return Bitmap.DecodeToHeight(stream, decodeHeight);
					return (object)new Bitmap(stream);
				}
			});
		}

	}
}
