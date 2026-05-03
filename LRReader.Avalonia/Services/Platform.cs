using Avalonia.Input.Platform;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using LRReader.Avalonia.Extensions;
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

		public Root Root { get; private set; } = null!;

		public AvaloniaPlatformService(TabsService tabs)
		{
			Tabs = tabs;

			MapPageToType<FirstRunPage>(Pages.FirstRun);
			MapPageToType<HostTabPage>(Pages.HostTab);
			MapPageToType<LoadingPage>(Pages.Loading);

			MapTransitionToType<FASuppressNavigationTransitionInfo>(PagesTransition.None);
			MapTransitionToType<FADrillInNavigationTransitionInfo>(PagesTransition.DrillIn);
		}

		public override void Init()
		{
			Tabs.MapTabToType<ArchivesTab>(Tab.Archives);
			Tabs.MapTabToType<SettingsTab>(Tab.Settings);
			Tabs.MapTabToType<ArchiveTab>(Tab.Archive);
			Tabs.MapTabToType<SearchResultsTab>(Tab.SearchResults);

			MapDialogToType<ServerProfileDialog>(Dialog.ServerProfile);
			MapDialogToType<MarkdownDialog>(Dialog.Markdown);

			MapSymbolToSymbol(Symbol.Favorite, new FASymbolIconSource { Symbol = FASymbol.Favorite });
			MapSymbolToSymbol(Symbol.Pictures, new FASymbolIconSource { Symbol = FASymbol.Pictures });
		}

		public override Version Version => new Version(1, 9, 6, 0);

		public override bool AnimationsEnabled => true;

		public override uint HoverTime => 500;

		public override bool DualScreen => false;

		public override double DualScreenWidth => 0;

		public override void ChangeTheme(AppTheme theme)
		{
			Application.Current!.RequestedThemeVariant = (Theme = theme).ToXamlTheme();
		}

		public override async void CopyToClipboard(string text)
		{
			await TopLevel.GetTopLevel(Root)!.Clipboard!.SetTextAsync(text);
		}

		public override string GetLocalizedString(string key)
		{
			var split = key.Split(['/'], 2);
			return ResourceLoader.GetForCurrentView(split[0]).GetString(split[1]);
		}

		public override void GoToPage(Pages page, PagesTransition transition, object? parameter = null)
		{
			Root.FrameContent.Navigate(GetPage(page), parameter, CreateTransition<FANavigationTransitionInfo>(transition));
		}

		public override async Task<IDialogResult> OpenDialog<D>(Dialog dialog, params object?[]? args)
		{
			await DialogSemaphore.WaitAsync();
			try
			{
				var newDialog = CreateDialog<D>(dialog, args);
				if (newDialog == null)
					return IDialogResult.None;
				return await newDialog.ShowAsync(TopLevel.GetTopLevel(Root)!);
			}
			finally
			{
				DialogSemaphore.Release();
			}
		}

		public override async Task<IDialogResult> ShowDialog(IDialog dialog)
		{
			await DialogSemaphore.WaitAsync();
			try
			{
				return await dialog.ShowAsync(TopLevel.GetTopLevel(Root)!);
			}
			finally
			{
				DialogSemaphore.Release();
			}
		}

		public override async Task<IDialogResult> OpenGenericDialog(string title = "", string primarybutton = "", string secondarybutton = "", string closebutton = "", object? content = null)
		{
			await DialogSemaphore.WaitAsync();
			try
			{
				var dialog = new GenericDialog { Title = title, PrimaryButtonText = primarybutton, SecondaryButtonText = secondarybutton, CloseButtonText = closebutton, Content = content };
				return (IDialogResult)(int)await dialog.ShowAsync(TopLevel.GetTopLevel(Root)!);
			}
			finally
			{
				DialogSemaphore.Release();
			}
		}

		public override Task<bool> OpenInBrowser(Uri uri)
		{
			return TopLevel.GetTopLevel(Root)!.Launcher.LaunchUriAsync(uri);
		}

		public override Task<bool> CheckAppInstalled(string package)
		{
			throw new NotImplementedException();
		}

		public void SetRoot(Root root) => this.Root = root;
	}
}
