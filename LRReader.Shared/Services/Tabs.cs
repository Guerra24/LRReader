using LRReader.Shared.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LRReader.Shared.Services
{
	public enum Tab
	{
		Archives, Archive, ArchiveEdit, Bookmarks, Categories, CategoryEdit, SearchResults, Settings, Web, Tools
	}

	public partial class TabsService : ObservableObject
	{
		private readonly ApiService Api;
		private readonly IDispatcherService Dispatcher;

		public ObservableCollection<ICustomTab> TabItems { get; } = new ObservableCollection<ICustomTab>();

		[ObservableProperty]
		private ICustomTab? _currentTab;

		[ObservableProperty]
		[AlsoNotifyChangeFor("Windowed")]
		private bool _fullscreen = false;
		public bool Windowed => !_fullscreen;

		public ControlFlags ControlFlags => Api.ControlFlags;

		private Dictionary<Tab, Type> Tabs = new Dictionary<Tab, Type>();

		public TabsService(ApiService api, IDispatcherService dispatcher)
		{
			Api = api;
			Dispatcher = dispatcher;
		}

		public void MapTabToType(Tab tab, Type type) => Tabs.Add(tab, type);

		public void OpenTab(Tab tab, params object[] args) => OpenTab(tab, true, args);

		public async void OpenTab(Tab tab, bool switchToTab = true, params object?[] args)
		{
			Type type;
			if (!Tabs.TryGetValue(tab, out type))
				return;
			var newTab = (ICustomTab)Activator.CreateInstance(type, args);
			var current = GetTabFromId(newTab.CustomTabId);
			if (current != null)
			{
				if (switchToTab)
					CurrentTab = current;
			}
			else
			{
				TabItems.Add(newTab);
				if (switchToTab)
					await Dispatcher.RunAsync(() => CurrentTab = newTab);
			}
		}

		private ICustomTab GetTabFromId(string id) => TabItems.FirstOrDefault(t => t.CustomTabId.Equals(id));

		public void CloseCurrentTab()
		{
			if (!(CurrentTab?.IsClosable ?? false))
				return;
			CloseTab(CurrentTab);
		}

		public void CloseTab(ICustomTab tab)
		{
			tab.Unload();
			TabItems.Remove(tab);
		}

		public void CloseTabWithId(string id)
		{
			var tab = GetTabFromId(id);
			if (tab != null)
				TabItems.Remove(tab);
		}

		public void CloseAllTabs()
		{
			foreach (var t in TabItems)
				t.Unload();
			TabItems.Clear();
		}

	}
}
