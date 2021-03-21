using LRReader.Shared;
using LRReader.Shared.Internal;
using LRReader.UWP.Internal;

namespace LRReader.Internal
{
	public class Global : SharedGlobal
	{

		public new static EventManager EventManager { get; set; }
		public static ImageProcessing ImageProcessing { get; set; }

		private static bool Loaded;

		public static void Init()
		{
			if (Loaded)
				return;
			ApiConnection = new ApiConnection();
			SharedGlobal.EventManager = EventManager = new EventManager();
			ArchivesManager = new ArchivesManager();
			ImageProcessing = new ImageProcessing();
			Loaded = true;
		}

	}
}
