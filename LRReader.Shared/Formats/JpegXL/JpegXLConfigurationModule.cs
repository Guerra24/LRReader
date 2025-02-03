using SixLabors.ImageSharp;

namespace LRReader.Shared.Formats.JpegXL
{
	public sealed class JpegXLConfigurationModule : IConfigurationModule
	{
		public void Configure(Configuration configuration)
		{
			configuration.ImageFormatsManager.SetDecoder(JpegXLFormat.Instance, new JpegXLDecoder());
			configuration.ImageFormatsManager.AddImageFormatDetector(new JpegXLImageFormatDetector());
		}
	}
}
