using Avalonia.Media;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LRReader.Avalonia.Views.Main
{
	public partial class LoadingPage : UserControl
	{
		private LoadingPageViewModel ViewModel;

		public LoadingPage()
		{
			InitializeComponent();
			DataContext = ViewModel = Service.Services.GetRequiredService<LoadingPageViewModel>();
		}

		private async void LoadingPage_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
		{
			if (Design.IsDesignMode)
				return;
			// Eh??? should it be here?
			await Task.Yield(); // Wait 1 frame to allow the theme to apply correctly
			Service.Platform.ChangeTheme(Service.Settings.Theme);
			((SolidColorBrush)Application.Current!.Resources["CustomReaderBackground"]!).Color = Color.Parse(Service.Settings.ReaderBackground);
			await ViewModel.Startup();
		}
	}
}
