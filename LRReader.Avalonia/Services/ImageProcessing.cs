using Avalonia.Media.Imaging;
using Avalonia.Platform;
using LRReader.Shared.Services;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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

				using var img = SixLabors.ImageSharp.Image.Load<Rgba32>(bytes);
				if (decodeWidth != 0 || decodeHeight != 0)
					img.Mutate(p => p.Resize((int)Math.Ceiling(decodeWidth * scaling), (int)Math.Ceiling(decodeHeight * scaling)));

				var raw = new byte[img.Width * img.Height * 4];

				img.CopyPixelDataTo(raw);

				unsafe
				{
					fixed (byte* data = raw)
					{
						return new Bitmap(PixelFormat.Rgba8888, AlphaFormat.Opaque, (nint)data, new PixelSize(img.Width, img.Height), new Vector(96, 96), img.Width * 4);
					}
				};
			}
			finally
			{
				if (image is Bitmap bitmap)
					Dispatcher.Run(bitmap.Dispose);
			}
		}

	}
}
