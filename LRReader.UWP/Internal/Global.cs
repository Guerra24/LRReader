using LRReader.Shared;
using LRReader.Shared.Internal;
using LRReader.UWP.Internal;

namespace LRReader.Internal
{
	public class Global : SharedGlobal
	{

		public new static EventManager EventManager { get; set; }
		public static ImageProcessing ImageProcessing { get; set; }

		public static void Init()
		{
			ApiConnection = new ApiConnection();
			SharedGlobal.EventManager = EventManager = new EventManager();
			ArchivesManager = new ArchivesManager();
			ImageProcessing = new ImageProcessing();
		}

	}
}
