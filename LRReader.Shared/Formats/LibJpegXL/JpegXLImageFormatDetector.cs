using SixLabors.ImageSharp.Formats;
using System.Diagnostics.CodeAnalysis;
using JxlNet;

namespace LRReader.Shared.Formats.LibJpegXL;

public sealed class JpegXLImageFormatDetector : IImageFormatDetector
{
	public int HeaderSize => 16;

	public unsafe bool TryDetectFormat(ReadOnlySpan<byte> header, [NotNullWhen(true)] out IImageFormat? format)
	{
		fixed (byte* data = header)
		{
			var signature = Jxl.JxlSignatureCheck(data, (nuint)header.Length);
			if (signature == JxlSignature.JXL_SIG_CODESTREAM || signature == JxlSignature.JXL_SIG_CONTAINER)
			{
				format = JpegXLFormat.Instance;
				return true;
			}
			format = null;
			return false;
		}
	}
}
