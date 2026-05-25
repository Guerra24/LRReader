using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using LRReader.Avalonia.Services;
using LRReader.Avalonia.Views;
using LRReader.Shared.Services;
using Sentry.Protocol;
using static LRReader.Shared.Services.Service;

namespace LRReader.Avalonia
{
	public class App : Application
	{

		private bool closing;

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
				window.Closing += Window_Closing;
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

			Dispatcher.UIThread.UnhandledException += App_UnhandledException;

			if (this.TryGetFeature<IActivatableLifetime>() is { } activatableLifetime)
				activatableLifetime.Deactivated += ActivatableLifetime_Deactivated;

			base.OnFrameworkInitializationCompleted();
		}

		private async void Window_Closing(object? sender, WindowClosingEventArgs e)
		{
			if (closing) return;

			e.Cancel = true;
			await Session.Suspend();
			SentrySdk.Flush(TimeSpan.FromSeconds(2));

			closing = true;
			((Window)sender!).Close();
		}

		private async void ActivatableLifetime_Deactivated(object? sender, ActivatedEventArgs e)
		{
			if (e.Kind == ActivationKind.Background)
			{
				await Session.Suspend();
				SentrySdk.Flush(TimeSpan.FromSeconds(2));
			}
		}

		private void App_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			var exception = e.Exception;
			e.Handled = true;
			exception.Data[Mechanism.HandledKey] = false;
			exception.Data[Mechanism.MechanismKey] = "Application.UnhandledException";

			SentrySdk.CaptureException(exception);

			SentrySdk.Flush(TimeSpan.FromSeconds(2));

			Service.Dispatcher.Run(async () => await Platform.OpenGenericDialog("Internal Error", "Continue", content: exception.Message));
		}
	}
}
