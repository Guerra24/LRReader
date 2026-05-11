using SixLabors.ImageSharp;

namespace LRReader.Shared.Formats.LibJpegXL;

public sealed class JpegXLMetadata : IDeepCloneable
{
	public IDeepCloneable DeepClone()
	{
		return new JpegXLMetadata();
	}
}
