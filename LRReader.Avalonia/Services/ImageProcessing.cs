using LRReader.Avalonia.Views.Controls;
using LRReader.Shared.Services;

namespace LRReader.Avalonia.Services
{
	public class AvaloniaImageProcessingService : ImageProcessingService
	{

		public override async Task<object?> ByteToBitmap(byte[]? bytes, int decodeWidth = 0, int decodeHeight = 0, object? img = default, CancellationToken cancellationToken = default)
		{
			var image = img as VirtualImage ?? new VirtualImage();

			image.Source = null;

			if (bytes == null)
				return null;
			if (bytes.Length == 0)
				return null;

			if (decodeWidth > 0)
				image.DecodePixelWidth = decodeWidth;
			if (decodeHeight > 0)
				image.DecodePixelHeight = decodeHeight;

			await image.SetSourceAsync(bytes);

			return image;
		}

	}
}
