using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Metadata;

namespace LRReader.Shared.Formats.JpegXL
{
	internal sealed class JpegXLImageInfo : IImageInfo
	{
		public PixelTypeInfo? PixelType { get; set; }

		public int Width { get; set; }

		public int Height { get; set; }

		public ImageMetadata? Metadata { get; set; }
	}
}
