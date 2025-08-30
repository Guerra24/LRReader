using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Tools;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinRT;

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

		[DynamicWindowsRuntimeCast(typeof(Flyout))]
		[DynamicWindowsRuntimeCast(typeof(Button))]
		private void HideFlyout_Click(object sender, RoutedEventArgs e) => ((Flyout)((Button)sender).Tag).Hide();
	}
}
