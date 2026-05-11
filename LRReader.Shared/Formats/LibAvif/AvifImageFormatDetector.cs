using AvifNet;
using SixLabors.ImageSharp.Formats;
using System.Diagnostics.CodeAnalysis;

namespace LRReader.Shared.Formats.LibAvif;

public sealed class AvifImageFormatDetector : IImageFormatDetector
{
	public int HeaderSize => 32;

	public bool TryDetectFormat(ReadOnlySpan<byte> header, [NotNullWhen(true)] out IImageFormat? format)
	{
		if (!Avif.IsAvailable)
		{
			format = null;
			return false;
		}
		unsafe
		{
			fixed (byte* data = header)
			{
				var roData = new avifROData
				{
					data = data,
					size = (nuint)header.Length
				};

				var res = Avif.avifPeekCompatibleFileType(&roData);
				if (res == 1)
					format = AvifFormat.Instance;
				else
					format = null;

				return res == 1;
			}
		}
	}
}
