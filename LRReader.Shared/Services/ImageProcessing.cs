using MetadataExtractor;
using MetadataExtractor.Formats.Gif;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Png;
using MetadataExtractor.Formats.WebP;
using MetadataExtractor.Util;
using Microsoft.AppCenter.Crashes;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.Shared.Services
{
	public abstract class ImageProcessingService
	{

		public abstract Task<object> ByteToBitmap(byte[] bytes, object image = default, bool transcode = false);

		public virtual Task<Size> GetImageSize(byte[] bytes)
		{
			return Task.Run(() =>
			{
				try
				{
					using (var stream = new MemoryStream(bytes))
					{
						var type = FileTypeDetector.DetectFileType(stream);
						stream.Seek(0, SeekOrigin.Begin);
						switch (type)
						{
							case FileType.Jpeg:
								var jpeg = JpegMetadataReader.ReadMetadata(stream);
								var jpegdir = jpeg.OfType<JpegDirectory>().FirstOrDefault();
								return new Size(jpegdir.GetImageWidth(), jpegdir.GetImageHeight());
							case FileType.Png:
								var png = PngMetadataReader.ReadMetadata(stream);
								var pngdir = png.OfType<PngDirectory>().FirstOrDefault();
								return new Size(pngdir.GetInt32(PngDirectory.TagImageWidth), pngdir.GetInt32(PngDirectory.TagImageHeight));
							case FileType.Gif:
								var gif = GifMetadataReader.ReadMetadata(stream);
								var gifdir = gif.OfType<GifHeaderDirectory>().FirstOrDefault();
								return new Size(gifdir.GetInt32(GifHeaderDirectory.TagImageWidth), gifdir.GetInt32(GifHeaderDirectory.TagImageHeight));
							case FileType.WebP:
								var webp = WebPMetadataReader.ReadMetadata(stream);
								var webpdir = webp.OfType<WebPDirectory>().FirstOrDefault();
								return new Size(webpdir.GetInt32(WebPDirectory.TagImageWidth), webpdir.GetInt32(WebPDirectory.TagImageHeight));
							default:
								return new Size(0, 0);
						}
					}
				}
				catch (Exception e)
				{
					Crashes.TrackError(e);
				}
				return new Size(0, 0);
			});
		}
	}
}
