namespace LRReader.Shared.Services
{

	public delegate void ShowNotification(string title, string content, int duration);
	public delegate void RebuildReaderImagesSet();
	public delegate void DeleteArchive(string id);

	public delegate void AddTab(object tab, bool switchToTab);
	public delegate void CloseAllTabs();
	public delegate void CloseTabWithId(string id);

	public class EventsService
	{
		public event ShowNotification ShowNotificationEvent;
		public event RebuildReaderImagesSet RebuildReaderImagesSetEvent;
		public event DeleteArchive DeleteArchiveEvent;

		public event AddTab AddTabEvent;
		public event CloseAllTabs CloseAllTabsEvent;
		public event CloseTabWithId CloseTabWithIdEvent;

		public void ShowNotification(string title, string content, int duration = 5000) => ShowNotificationEvent?.Invoke(title, content, duration);

		public void RebuildReaderImagesSet() => RebuildReaderImagesSetEvent?.Invoke();

		public void DeleteArchive(string id) => DeleteArchiveEvent?.Invoke(id);

		public void AddTab(ICustomTab tab, bool switchToTab = true) => AddTabEvent?.Invoke(tab, switchToTab);

		public void CloseAllTabs() => CloseAllTabsEvent?.Invoke();

		public void CloseTabWithId(string id) => CloseTabWithIdEvent?.Invoke(id);

	}

}
