using SixLabors.ImageSharp;

namespace LRReader.Shared.Formats.LibAvif;

public sealed class AvifMetadata : IDeepCloneable
{
	public IDeepCloneable DeepClone()
	{
		return new AvifMetadata();
	}
}
