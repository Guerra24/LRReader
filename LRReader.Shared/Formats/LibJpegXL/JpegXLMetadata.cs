using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;

namespace LRReader.Shared.Formats.LibJpegXL;

public sealed class JpegXLMetadata : IFormatMetadata<JpegXLMetadata>
{
	public JpegXLMetadata() { }

	public JpegXLMetadata(JpegXLMetadata other)
	{
		BitsPerSample = other.BitsPerSample;
	}

	public int BitsPerSample { get; set; }

	public static JpegXLMetadata FromFormatConnectingMetadata(FormatConnectingMetadata metadata)
	{
		return new JpegXLMetadata();
	}

	public void AfterImageApply<TPixel>(Image<TPixel> destination, Matrix4x4 matrix) where TPixel : unmanaged, IPixel<TPixel>
	{
	}

	public PixelTypeInfo GetPixelTypeInfo()
	{
		return new PixelTypeInfo(BitsPerSample);
	}

	public FormatConnectingMetadata ToFormatConnectingMetadata() => new()
	{
		PixelTypeInfo = this.GetPixelTypeInfo()
	};

	IDeepCloneable IDeepCloneable.DeepClone() => this.DeepClone();

	public JpegXLMetadata DeepClone() => new(this);
}