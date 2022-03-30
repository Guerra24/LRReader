﻿using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using LRReader.Avalonia.Views.Dialogs;
using LRReader.Avalonia.Views.Main;
using LRReader.Avalonia.Views.Tabs;
using LRReader.Shared.Models;
using LRReader.Shared.Services;
using System;
using System.Threading.Tasks;

namespace LRReader.Avalonia.Services
{
	public class AvaloniaPlatformService : PlatformService
	{
		private readonly TabsService Tabs;

		public AvaloniaPlatformService(TabsService tabs)
		{
			Tabs = tabs;

			MapPageToType(Pages.FirstRun, typeof(FirstRunPage));
			MapPageToType(Pages.HostTab, typeof(HostTabPage));
			MapPageToType(Pages.Loading, typeof(LoadingPage));
		}

		public override void Init()
		{
			Tabs.MapTabToType(Tab.Archives, typeof(ArchivesTab));
		}

		public override Version Version => new Version(1, 7, 6, 0);

		public override bool AnimationsEnabled => true;

		public override uint HoverTime => 500;

		public override void ChangeTheme(AppTheme theme)
		{
			throw new NotImplementedException();
		}

		public override async void CopyToClipboard(string text)
		{
			var clipboard = Application.Current?.Clipboard;
			if (clipboard != null)
				await clipboard.SetTextAsync(text);
		}

		public override string GetLocalizedString(string key)
		{
			var split = key.Split(new[] { '/' }, 2);
			return ResourceLoader.GetForCurrentView(split[0]).GetString(split[1]);
		}

		public override void GoToPage(Pages page, PagesTransition transition, object? parameter = null)
		{
			(Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow.Content = Activator.CreateInstance(GetPage(page));
		}

		public override async Task<IDialogResult> OpenGenericDialog(string title = "", string primarybutton = "", string secondarybutton = "", string closebutton = "", object? content = null)
		{
			var dialog = new GenericDialog();
			dialog.SetData(title, primarybutton, secondarybutton, closebutton, content);
			return await dialog.ShowAsync();
		}

		public override Task<bool> OpenInBrowser(Uri uri)
		{
			throw new NotImplementedException();
		}
	}
}
