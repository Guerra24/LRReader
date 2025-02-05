using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Tools;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Content.Tools
{

	public sealed partial class BulkEditor : Page
	{
		private BulkEditorViewModel Data;

		public BulkEditor()
		{
			this.InitializeComponent();
			Data = Service.Services.GetRequiredService<BulkEditorViewModel>();
		}

		private void HideFlyout_Click(object sender, RoutedEventArgs e) => ((Flyout)((Button)sender).Tag).Hide();
	}
}
