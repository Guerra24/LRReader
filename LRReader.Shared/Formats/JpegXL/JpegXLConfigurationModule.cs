using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace LRReader.Shared.Formats.JpegXL
{
	public sealed class JpegXLConfigurationModule : IImageFormatConfigurationModule
	{
		public void Configure(Configuration configuration)
		{
			configuration.ImageFormatsManager.SetDecoder(JpegXLFormat.Instance, new JpegXLDecoder());
			configuration.ImageFormatsManager.AddImageFormatDetector(new JpegXLImageFormatDetector());
		}
	}
}
