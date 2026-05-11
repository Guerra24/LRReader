using SixLabors.ImageSharp.Formats;

namespace LRReader.Shared.Formats.LibAvif;

public sealed class AvifFormat : IImageFormat<AvifMetadata>
{
	private AvifFormat() { }

	public static AvifFormat Instance { get; } = new();

	public string Name => "AVIF";

	public string DefaultMimeType => "image/avif";

	public IEnumerable<string> MimeTypes => AvifConstants.MimeTypes;

	public IEnumerable<string> FileExtensions => AvifConstants.FileExtensions;

	public AvifMetadata CreateDefaultFormatMetadata() => new AvifMetadata();
}
