using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LRReader.Shared.Models;

namespace LRReader.Shared.Services
{
	public enum Tab
	{
		Archives, Archive, ArchiveEdit, Bookmarks, Categories, CategoryEdit, SearchResults, Settings, Web, Tools, Tankoubons, Tankoubon, TankoubonEdit
	}

	public partial class TabsService : ObservableObject
	{
		private readonly IDispatcherService Dispatcher;

		public ObservableCollection<ICustomTab> TabItems { get; } = new ObservableCollection<ICustomTab>();

		[ObservableProperty]
		private ICustomTab? _currentTab;

		[ObservableProperty]
		[NotifyPropertyChangedFor("Windowed")]
		private bool _fullscreen;
		public bool Windowed => !Fullscreen;

		private Dictionary<Tab, Type> Tabs = new Dictionary<Tab, Type>();

		public TabsService(IDispatcherService dispatcher)
		{
			Dispatcher = dispatcher;
		}

		public void MapTabToType(Tab tab, Type type) => Tabs.Add(tab, type);

		[RelayCommand]
		public void OpenTab(Tab tab) => OpenTab(tab, true);

		public void OpenTab(Tab tab, params object[] args) => OpenTab(tab, true, args);

		public async void OpenTab(Tab tab, bool switchToTab = true, params object?[] args)
		{
			Type type;
			if (!Tabs.TryGetValue(tab, out type))
				return;
			var newTab = (ICustomTab)Activator.CreateInstance(type, args);
			newTab.Tab = tab;
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

		private ICustomTab? GetTabFromId(string? id) => TabItems.FirstOrDefault(t => t.CustomTabId.Equals(id));

		public void CloseCurrentTab()
		{
			if (!(CurrentTab?.IsClosable ?? false))
				return;
			CloseTab(CurrentTab);
		}

		public void CloseTab(ICustomTab tab)
		{
			tab.Dispose();
			TabItems.Remove(tab);
		}

		public void CloseTabWithId(string? id)
		{
			var tab = GetTabFromId(id);
			if (tab != null)
				CloseTab(tab);
		}

		public void CloseAllTabs()
		{
			foreach (var t in TabItems)
				t.Dispose();
			TabItems.Clear();
		}

	}
}
