using Avalonia.Media.Imaging;
using LRReader.Shared.Services;

namespace LRReader.Avalonia.Services
{
	public class AvaloniaImageProcessingService : ImageProcessingService
	{
		private double scaling;

		public AvaloniaImageProcessingService(PlatformService platform)
		{
			scaling = TopLevel.GetTopLevel(((AvaloniaPlatformService)platform).Root)!.RenderScaling;
		}

		public override Task<object?> ByteToBitmap(byte[]? bytes, int decodeWidth = 0, int decodeHeight = 0, object? image = default, CancellationToken cancellationToken = default)
		{
			return Task.Run(() =>
			{
				// TODO
				//if (image is Bitmap bitmap)
					//bitmap.Dispose();
				if (bytes == null)
					return null;
				using (var stream = new MemoryStream(bytes))
				{
					if (decodeWidth != 0)
						return Bitmap.DecodeToWidth(stream, (int)Math.Ceiling(decodeWidth * scaling));
					if (decodeHeight != 0)
						return Bitmap.DecodeToHeight(stream, (int)Math.Ceiling(decodeHeight * scaling));
					return (object)new Bitmap(stream);
				}
			});
		}

	}
}
