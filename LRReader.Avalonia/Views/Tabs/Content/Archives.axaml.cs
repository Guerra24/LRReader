using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using LRReader.Shared.ViewModels;

namespace LRReader.Avalonia.Views.Tabs.Content
{
	public partial class Archives : UserControl
	{

		private ArchivesPageViewModel Data;

		private bool loaded, reloading;

		private string query = "";

		public Archives()
		{
			InitializeComponent();
			Data = DataContext as ArchivesPageViewModel;
		}

		private async void Archives_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
		{
			if (Design.IsDesignMode)
				return;
			if (loaded)
				return;
			loaded = true;
			reloading = true;
			//AscFlyoutItem.IsChecked = Service.Settings.OrderByDefault == Order.Ascending;
			//DesFlyoutItem.IsChecked = Service.Settings.OrderByDefault == Order.Descending;
			Data.ControlsEnabled = false; // THIS ---------------
			Data.LoadBookmarks();
			await HandleSearch();
			Data.ControlsEnabled = true;
			reloading = false;
		}

		private async Task HandleSearch()
		{
			Data.Query = query;
			await Data.ReloadSearch();
		}

	}
}
