using Avalonia.Media.Imaging;
using LRReader.Shared.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Avalonia.Services
{
	public class AvaloniaImageProcessingService : ImageProcessingService
	{
		public override Task<object?> ByteToBitmap(byte[]? bytes, int decodeWidth = 0, int decodeHeight = 0, bool transcode = false, object? image = default)
		{
			return Task.Run(() =>
			{
				if (bytes == null)
					return null;
				using (var stream = new MemoryStream(bytes))
				{
					return (object)new Bitmap(stream);
				}
			});
		}
	}
}
