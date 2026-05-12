using JxlNet;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace LRReader.Shared.Formats.LibJpegXL;

public sealed class JpegXLConfigurationModule : IImageFormatConfigurationModule
{
	public void Configure(Configuration configuration)
	{
		if (!Jxl.IsAvailable || !JxlThreads.IsAvailable)
			return;
		configuration.ImageFormatsManager.SetDecoder(JpegXLFormat.Instance, new JpegXLDecoder());
		configuration.ImageFormatsManager.AddImageFormatDetector(new JpegXLImageFormatDetector());
	}
}
