using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace LRReader.Shared.Formats.LibAvif;

public sealed class AvifConfigurationModule : IImageFormatConfigurationModule
{
	public void Configure(Configuration configuration)
	{
		configuration.ImageFormatsManager.SetDecoder(AvifFormat.Instance, new AvifDecoder());
		configuration.ImageFormatsManager.AddImageFormatDetector(new AvifImageFormatDetector());
	}
}
