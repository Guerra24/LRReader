using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using LRReader.Avalonia.Views;
using System.Linq;

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
			if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				DisableAvaloniaDataAnnotationValidation();
				desktop.MainWindow = new MainWindow();
			}
			else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
			{
				singleView.MainView = new MainView();
			}

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
