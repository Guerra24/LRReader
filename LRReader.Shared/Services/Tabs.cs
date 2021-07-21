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

	public class TabsService : ObservableObject
	{
		private readonly ApiService Api;
		private readonly EventsService Events;
		private readonly IDispatcherService Dispatcher;

		public ObservableCollection<ICustomTab> TabItems { get; } = new ObservableCollection<ICustomTab>();

		private ICustomTab _currentTab;
		public ICustomTab CurrentTab
		{
			get => _currentTab;
			set => SetProperty(ref _currentTab, value);
		}
		private bool _fullscreen = false;
		public bool FullScreen
		{
			get => _fullscreen;
			set
			{
				SetProperty(ref _fullscreen, value);
				OnPropertyChanged("Windowed");
			}
		}
		public bool Windowed => !_fullscreen;
		public ControlFlags ControlFlags => Api.ControlFlags;

		private Dictionary<Tab, Type> Tabs = new Dictionary<Tab, Type>();

		public TabsService(ApiService api, EventsService events, IDispatcherService dispatcher)
		{
			Api = api;
			Events = events;
			Dispatcher = dispatcher;
		}

		public void Hook()
		{
			Events.DeleteArchiveEvent += DeleteArchive;
		}

		public void UnHook()
		{
			Events.DeleteArchiveEvent -= DeleteArchive;
		}

		public void MapTabToType(Tab tab, Type type) => Tabs.Add(tab, type);

		public void OpenTab(Tab tab, params object[] args) => OpenTab(tab, true, args);

		public async void OpenTab(Tab tab, bool switchToTab = true, params object[] args)
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

		public async void AddTab(ICustomTab tab, bool switchToTab = true)
		{
			var current = GetTabFromId(tab.CustomTabId);
			if (current != null)
			{
				if (switchToTab)
					CurrentTab = current;
			}
			else
			{
				TabItems.Add(tab);
				if (switchToTab)
					await Dispatcher.RunAsync(() => CurrentTab = tab);
			}
		}

		private ICustomTab GetTabFromId(string id) => TabItems.FirstOrDefault(t => t.CustomTabId.Equals(id));

		public void CloseCurrentTab()
		{
			if (!CurrentTab.IsClosable)
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

		public void DeleteArchive(string id)
		{
			CloseTabWithId("Edit_" + id);
			CloseTabWithId("Archive_" + id);
		}

	}
}
