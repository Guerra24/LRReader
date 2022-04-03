using Avalonia;
using Avalonia.Controls;
using LRReader.Shared.ViewModels;

namespace LRReader.Avalonia.Views.Main
{
	public partial class LoadingPage : UserControl
	{
		private LoadingPageViewModel ViewModel;

		public LoadingPage()
		{
			InitializeComponent();
			ViewModel = DataContext as LoadingPageViewModel;
		}

		private async void LoadingPage_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
		{
			if (Design.IsDesignMode)
				return;
			await ViewModel.Startup();
		}
	}
}
