using LRReader.Shared;
using LRReader.Shared.Internal;
using LRReader.UWP.Internal;

namespace LRReader.Internal
{
	public class Global : SharedGlobal
	{

		public static ImageProcessing ImageProcessing { get; set; }

		private static bool Loaded;

		public static void Init()
		{
			if (Loaded)
				return;
			ImageProcessing = new ImageProcessing();
			Loaded = true;
		}

	}
}
