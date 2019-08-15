using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace LRReader.Internal
{
	public delegate void ShowError(string title, string content);
	public delegate void ShowHeader(bool value);
	public delegate void SearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args);
	public delegate void SearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args);

	public class EventManager
	{
		public event ShowError ShowErrorEvent;
		public event ShowHeader ShowHeaderEvent;
		public event SearchTextChanged SearchTextChangedEvent;
		public event SearchQuerySubmitted SearchQuerySubmittedEvent;

		public void ShowError(string title, string content)
		{
			ShowErrorEvent(title, content);
		}

		public void ShowHeader(bool value)
		{
			ShowHeaderEvent(value);
		}

		public void SearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			SearchTextChangedEvent(sender, args);
		}

		public void SearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{
			SearchQuerySubmittedEvent(sender, args);
		}
	}
}
