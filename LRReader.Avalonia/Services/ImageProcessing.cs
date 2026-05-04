using Avalonia.Media.Imaging;
using LRReader.Shared.Services;

namespace LRReader.Avalonia.Services
{
	public class AvaloniaImageProcessingService : ImageProcessingService
	{
		private readonly IDispatcherService Dispatcher;

		private double scaling;

		public AvaloniaImageProcessingService(PlatformService platform, IDispatcherService dispatcher)
		{
			Dispatcher = dispatcher;
			scaling = TopLevel.GetTopLevel(((AvaloniaPlatformService)platform).Root)!.RenderScaling;
		}

		public override async Task<object?> ByteToBitmap(byte[]? bytes, int decodeWidth = 0, int decodeHeight = 0, object? image = default, CancellationToken cancellationToken = default)
		{
			await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
			try
			{
				if (bytes == null)
					return null;
				using var stream = new MemoryStream(bytes, 0, bytes.Length, false, true);
				if (decodeWidth != 0)
					return Bitmap.DecodeToWidth(stream, (int)Math.Ceiling(decodeWidth * scaling));
				if (decodeHeight != 0)
					return Bitmap.DecodeToHeight(stream, (int)Math.Ceiling(decodeHeight * scaling));
				return new Bitmap(stream);
			}
			finally
			{
				if (image is Bitmap bitmap)
					Dispatcher.Run(bitmap.Dispose);
			}
		}

	}
}
