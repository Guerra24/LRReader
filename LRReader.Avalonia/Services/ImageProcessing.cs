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
		public override Task<object> ByteToBitmap(byte[] bytes, object image = null, bool transcode = false)
		{
			return Task.Run(() =>
			{
				using (var stream = new MemoryStream(bytes))
				{
					return (object)new Bitmap(stream);
				}
			});
		}
	}
}
