using LRReader.Shared.Models.Main;
using LRReader.Shared.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Tabs.Content
{
	public sealed partial class ArchiveEdit : UserControl
	{

		public ArchiveEditViewModel Data;

		public ArchiveEdit()
		{
			this.InitializeComponent();
			Data = DataContext as ArchiveEditViewModel;
		}

		public async void LoadArchive(Archive archive) => await Data.LoadArchive(archive);

		public async void Refresh() => await Data.ReloadArchive();

		private async void SaveButton_Click(object sender, RoutedEventArgs e) => await Data.SaveArchive();

		private async void PluginButton_Click(object sender, RoutedEventArgs e) => await Data.UsePlugin();
	}
}
