using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace LRReader.Internal
{
	public delegate void ShowError(string title, string content);
	public delegate void AddTab(TabViewItem tab, bool switchToTab);
	public delegate void CloseAllTabs();
	public delegate void RebuildReaderImagesSet();

	public class EventManager
	{
		public event ShowError ShowErrorEvent;
		public event AddTab AddTabEvent;
		public event CloseAllTabs CloseAllTabsEvent;
		public event RebuildReaderImagesSet RebuildReaderImagesSetEvent;

		public void ShowError(string title, string content)
		{
			ShowErrorEvent?.Invoke(title, content);
		}

		public void AddTab(TabViewItem tab)
		{
			AddTabEvent?.Invoke(tab, true);
		}
		public void AddTab(TabViewItem tab, bool switchToTab)
		{
			AddTabEvent?.Invoke(tab, switchToTab);
		}

		public void CloseAllTabs()
		{
			CloseAllTabsEvent?.Invoke();
		}

		public void RebuildReaderImagesSet()
		{
			RebuildReaderImagesSetEvent?.Invoke();
		}
	}
}
