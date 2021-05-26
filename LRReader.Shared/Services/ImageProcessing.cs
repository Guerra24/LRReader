using System.Drawing;
using System.Threading.Tasks;

namespace LRReader.Shared.Services
{
	public interface IImageProcessingService
	{

		Task<object> ByteToBitmap(byte[] bytes, object image = default, bool transcode = false);

		Task<Size> GetImageSize(byte[] bytes);
	}
}
