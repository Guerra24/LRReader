﻿using LRReader.Shared.Internal;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace LRReader.Internal
{
	public delegate void AddTab(TabViewItem tab, bool switchToTab);
	public delegate void CloseAllTabs();
	public delegate void CloseTabWithHeader(string header);

	public class EventManager : SharedEventManager
	{
		public event AddTab AddTabEvent;
		public event CloseAllTabs CloseAllTabsEvent;
		public event CloseTabWithHeader CloseTabWithHeaderEvent;

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

		public void CloseTabWithHeader(string header)
		{
			CloseTabWithHeaderEvent?.Invoke(header);
		}
	}
}
