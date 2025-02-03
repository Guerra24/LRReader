using System;
using SixLabors.ImageSharp.Formats;

namespace LRReader.Shared.Formats.JpegXL
{
	public sealed class JpegXLImageFormatDetector : IImageFormatDetector
	{
		public int HeaderSize => 12;

		public IImageFormat DetectFormat(ReadOnlySpan<byte> header)
		{
			var jxlCodestream = header[0] == 0xff && header[1] == 0x0A;
			var jxlContainer = header[0] == 0 && header[1] == 0 && header[2] == 0 && header[3] == 0xC && header[4] == 'J' && header[5] == 'X' && header[6] == 'L' && header[7] == ' ' && header[8] == 0xD && header[9] == 0xA && header[10] == 0x87 && header[11] == 0xA;

			return (jxlCodestream || jxlContainer) ? JpegXLFormat.Instance : null!;
		}
	}
}
