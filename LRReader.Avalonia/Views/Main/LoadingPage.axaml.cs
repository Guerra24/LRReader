using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LRReader.Shared.ViewModels;

namespace LRReader.Avalonia.Views.Main
{
	public class LoadingPage : UserControl
	{
		private LoadingPageViewModel ViewModel;

		public LoadingPage()
		{
			InitializeComponent();
			ViewModel = DataContext as LoadingPageViewModel;
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		private async void LoadingPage_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
		{
			if (Design.IsDesignMode)
				return;
			await ViewModel.Startup();
		}
	}
}
