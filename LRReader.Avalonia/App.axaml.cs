using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LRReader.Avalonia.Services;
using LRReader.Avalonia.Views;
using LRReader.Shared.Services;
using static LRReader.Shared.Services.Service;

namespace LRReader.Avalonia
{
	public class App : Application
	{

		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public override void OnFrameworkInitializationCompleted()
		{
			var platform = (AvaloniaPlatformService)Platform;
			if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				var window = new MainWindow();
				desktop.MainWindow = window;
				platform.SetRoot(window.Root);
				Platform.GoToPage(Pages.Loading, PagesTransition.None);
			}
			else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime)
			{
				singleViewFactoryApplicationLifetime.MainViewFactory = () =>
				{
					var root = new Root();
					platform.SetRoot(root);
					Platform.GoToPage(Pages.Loading, PagesTransition.None);
					return root;
				};
			}
			else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
			{
				var root = new Root();
				singleView.MainView = root;
				platform.SetRoot(root);
				Platform.GoToPage(Pages.Loading, PagesTransition.None);
			}

			base.OnFrameworkInitializationCompleted();
		}

	}
}
