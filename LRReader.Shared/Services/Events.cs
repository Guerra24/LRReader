using LRReader.Shared.Models;

namespace LRReader.Shared.Services
{

	public delegate void ShowNotification(string title, string content, int duration);
	public delegate void RebuildReaderImagesSet();
	public delegate void DeleteArchive(string id);

	public class EventsService
	{
		public event ShowNotification ShowNotificationEvent;
		public event RebuildReaderImagesSet RebuildReaderImagesSetEvent;
		public event DeleteArchive DeleteArchiveEvent;

		public void ShowNotification(string title, string content, int duration = 5000) => ShowNotificationEvent?.Invoke(title, content, duration);

		public void RebuildReaderImagesSet() => RebuildReaderImagesSetEvent?.Invoke();

		public void DeleteArchive(string id) => DeleteArchiveEvent?.Invoke(id);

	}

}
