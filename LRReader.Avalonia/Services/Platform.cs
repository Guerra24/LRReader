using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using LRReader.Avalonia.Views.Main;
using LRReader.Avalonia.Views.Tabs;
using LRReader.Shared.Models;
using LRReader.Shared.Services;
using System;
using System.Threading.Tasks;

namespace LRReader.Avalonia.Services
{
	class AvaloniaPlatformService : PlatformService
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

		public override Version Version => throw new NotImplementedException();

		public override bool AnimationsEnabled => throw new NotImplementedException();

		public override uint HoverTime => throw new NotImplementedException();

		public override void ChangeTheme(AppTheme theme)
		{
			throw new NotImplementedException();
		}

		public override void CopyToClipboard(string text)
		{
			throw new NotImplementedException();
		}

		public override string GetLocalizedString(string key)
		{
			var split = key.Split(new[] { '/' }, 2);
			return ResourceLoader.GetForCurrentView(split[0]).GetString(split[1]);
		}

		public override void GoToPage(Pages page, PagesTransition transition, object parameter = null)
		{
			(Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow.Content = Activator.CreateInstance(GetPage(page));
		}

		public override Task<IDialogResult> OpenGenericDialog(string title = "", string primarybutton = "", string secondarybutton = "", string closebutton = "", object content = null)
		{
			throw new NotImplementedException();
		}

		public override Task<bool> OpenInBrowser(Uri uri)
		{
			throw new NotImplementedException();
		}
	}
}
