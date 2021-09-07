namespace LRReader.Shared.Services
{

	public delegate void RebuildReaderImagesSet();

	public class EventsService
	{
		public event RebuildReaderImagesSet RebuildReaderImagesSetEvent;

		public void RebuildReaderImagesSet() => RebuildReaderImagesSetEvent?.Invoke();

	}

}
