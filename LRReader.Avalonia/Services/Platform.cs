using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using LRReader.Avalonia.Resources;
using LRReader.Avalonia.Views;
using LRReader.Avalonia.Views.Dialogs;
using LRReader.Avalonia.Views.Main;
using LRReader.Avalonia.Views.Tabs;
using LRReader.Shared.Models;
using LRReader.Shared.Services;

namespace LRReader.Avalonia.Services
{
	public class AvaloniaPlatformService : PlatformService
	{
		private readonly TabsService Tabs;

		public AvaloniaPlatformService(TabsService tabs)
		{
			Tabs = tabs;

			MapPageToType<FirstRunPage>(Pages.FirstRun);
			MapPageToType<HostTabPage>(Pages.HostTab);
			MapPageToType<LoadingPage>(Pages.Loading);
		}

		public override void Init()
		{
			Tabs.MapTabToType<ArchivesTab>(Tab.Archives);
			Tabs.MapTabToType<SettingsTab>(Tab.Settings);

			MapDialogToType<ServerProfileDialog>(Dialog.ServerProfile);
		}

		public override Version Version => new Version(1, 7, 6, 0);

		public override bool AnimationsEnabled => true;

		public override uint HoverTime => 500;

		public override bool DualScreen => false;

		public override double DualScreenWidth => 0;

		public override void ChangeTheme(AppTheme theme)
		{
			throw new NotImplementedException();
		}

		public override async void CopyToClipboard(string text)
		{
			/*var clipboard = Application.Current?.Clipboard;
			if (clipboard != null)
				await clipboard.SetTextAsync(text);*/
		}

		public override string GetLocalizedString(string key)
		{
			var split = key.Split(new[] { '/' }, 2);
			return ResourceLoader.GetForCurrentView(split[0]).GetString(split[1]);
		}

		public override void GoToPage(Pages page, PagesTransition transition, object? parameter = null)
		{
			MainView main = null!;
			if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				main = (MainView)desktop!.MainWindow!.Content!;
			}
			else if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleView)
			{
				main = (MainView)singleView.MainView!;
			}
			main.Content = Activator.CreateInstance(GetPage(page));
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

		public override Task<bool> CheckAppInstalled(string package)
		{
			throw new NotImplementedException();
		}
	}
}
