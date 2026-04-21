using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
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
				DisableAvaloniaDataAnnotationValidation();
				var window = new MainWindow();
				desktop.MainWindow = window;
				platform.SetRoot(window.Root);
			}
			else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
			{
				var root = new Root();
				singleView.MainView = root;
				platform.SetRoot(root);
			}

			Platform.GoToPage(Pages.Loading, PagesTransition.None);

			base.OnFrameworkInitializationCompleted();
		}

		private void DisableAvaloniaDataAnnotationValidation()
		{
			// Get an array of plugins to remove
			var dataValidationPluginsToRemove = BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

			// remove each entry found
			foreach (var plugin in dataValidationPluginsToRemove)
			{
				BindingPlugins.DataValidators.Remove(plugin);
			}
		}
	}
}
