using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;

namespace LRReader.Shared.Formats.LibAvif;

public sealed class AvifMetadata : IFormatMetadata<AvifMetadata>
{
	public AvifMetadata() { }

	public AvifMetadata(AvifMetadata other)
	{
		Depth = other.Depth;
	}

	public int Depth { get; set; }

	public static AvifMetadata FromFormatConnectingMetadata(FormatConnectingMetadata metadata)
	{
		return new AvifMetadata();
	}

	public void AfterImageApply<TPixel>(Image<TPixel> destination, Matrix4x4 matrix) where TPixel : unmanaged, IPixel<TPixel>
	{
	}

	public PixelTypeInfo GetPixelTypeInfo()
	{
		return new PixelTypeInfo(Depth);
	}

	public FormatConnectingMetadata ToFormatConnectingMetadata() => new()
	{
		PixelTypeInfo = this.GetPixelTypeInfo()
	};

	IDeepCloneable IDeepCloneable.DeepClone() => this.DeepClone();

	public AvifMetadata DeepClone() => new(this);
}
