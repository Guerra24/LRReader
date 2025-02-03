using SixLabors.ImageSharp;

namespace LRReader.Shared.Formats.JpegXL
{
	public sealed class JpegXLMetadata : IDeepCloneable
	{
		public IDeepCloneable DeepClone()
		{
			return new JpegXLMetadata();
		}
	}
}
