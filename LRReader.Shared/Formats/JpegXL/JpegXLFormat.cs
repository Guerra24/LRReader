using System.Collections.Generic;
using SixLabors.ImageSharp.Formats;

namespace LRReader.Shared.Formats.JpegXL
{
	public sealed class JpegXLFormat : IImageFormat<JpegXLMetadata>
	{
		private JpegXLFormat() { }

		public static JpegXLFormat Instance { get; } = new();

		public string Name => "JPEG XL";

		public string DefaultMimeType => "image/jxl";

		public IEnumerable<string> MimeTypes => JpegXLConstants.MimeTypes;

		public IEnumerable<string> FileExtensions => JpegXLConstants.FileExtensions;

		public JpegXLMetadata CreateDefaultFormatMetadata() => new JpegXLMetadata();
	}
}
