using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Shared.Internal
{
	public delegate void ShowNotification(string title, string content, int duration);
	public delegate void RebuildReaderImagesSet();

	public class SharedEventManager
	{
		public event ShowNotification ShowNotificationEvent;
		public event RebuildReaderImagesSet RebuildReaderImagesSetEvent;

		public void ShowError(string title, string content, int duration = 5000)
		{
			ShowNotificationEvent?.Invoke(title, content, duration);
		}

		public void ShowNotification(string title, string content, int duration = 5000)
		{
			ShowNotificationEvent?.Invoke(title, content, duration);
		}

		public void RebuildReaderImagesSet()
		{
			RebuildReaderImagesSetEvent?.Invoke();
		}

	}
}
