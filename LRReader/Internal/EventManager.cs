using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace LRReader.Internal
{
	public delegate void ShowError(string title, string content);
	public delegate void ShowHeader(bool visible);
	public delegate void SearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args);
	public delegate void SearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args);
	public delegate void ShowSearchBox(bool visible);

	public delegate void AddTab(TabViewItem tab);
	public delegate void CloseAllTabs();

	public class EventManager
	{
		public event ShowError ShowErrorEvent;
		public event ShowHeader ShowHeaderEvent;
		public event SearchTextChanged SearchTextChangedEvent;
		public event SearchQuerySubmitted SearchQuerySubmittedEvent;
		public event ShowSearchBox ShowSearchBoxEvent;

		public event AddTab AddTabEvent;
		public event CloseAllTabs CloseAllTabsEvent;

		public void ShowError(string title, string content)
		{
			ShowErrorEvent?.Invoke(title, content);
		}

		public void ShowHeader(bool visible)
		{
			ShowHeaderEvent?.Invoke(visible);
		}

		public void SearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			SearchTextChangedEvent?.Invoke(sender, args);
		}

		public void SearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{
			SearchQuerySubmittedEvent?.Invoke(sender, args);
		}

		public void ShowSearchBox(bool visible)
		{
			ShowSearchBoxEvent?.Invoke(visible);
		}

		public void AddTab(TabViewItem tab)
		{
			AddTabEvent?.Invoke(tab);
		}

		public void CloseAllTabs()
		{
			CloseAllTabsEvent?.Invoke();
		}
	}
}
