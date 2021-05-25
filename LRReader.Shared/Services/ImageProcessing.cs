using System.Drawing;
using System.Threading.Tasks;

namespace LRReader.Shared.Services
{
	public interface IImageProcessingService<T>
	{

		Task<T> ByteToBitmap(byte[] bytes, T image = default, bool transcode = false);

		Task<Size> GetImageSize(byte[] bytes);
	}
}
