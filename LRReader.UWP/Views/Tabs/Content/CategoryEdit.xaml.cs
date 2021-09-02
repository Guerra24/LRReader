using LRReader.Shared.Models.Main;
using LRReader.Shared.ViewModels;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace LRReader.UWP.Views.Tabs.Content
{

	public sealed partial class CategoryEdit : UserControl
	{

		public CategoryEditViewModel ViewModel;

		private ResourceLoader lang;

		public CategoryEdit()
		{
			this.InitializeComponent();
			ViewModel = DataContext as CategoryEditViewModel;
			ArchiveList.Data.CustomArchiveCheckEvent = CustomArchiveCheck;
			lang = ResourceLoader.GetForCurrentView("Tabs");
			VisualStateManager.GoToState(this, "Selected", false);
		}

		private bool CustomArchiveCheck(Archive archive)
		{
			if (ViewModel.category != null)
				return !ViewModel.category.archives.Contains(archive.arcid);
			else
				return true;
		}

		public async void LoadCategory(Category category) => await ViewModel.LoadCategory(category);

		public async Task Refresh()
		{
			await ViewModel.Refresh();
			await ArchiveList.Data.ReloadSearch();
		}

		private async void Refresh_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			args.Handled = true;
			await Refresh();
		}

		private void CategoryName_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
		{
			bool allow = true;
			CategoryError.Text = "";
			if (string.IsNullOrEmpty(sender.Text))
			{
				CategoryError.Text = ResourceLoader.GetForCurrentView("Dialogs").GetString("CreateCategory/ErrorName");
				allow = false;
			}
			ViewModel.CanSave = allow;
		}

		private async void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			await ViewModel.SaveCategory();
		}

		// Add
		private void CategoryArchives_DragOver(object sender, DragEventArgs e)
		{
			if (e.DataView.Properties.ContainsKey("archivesAdd"))
			{
				e.AcceptedOperation = DataPackageOperation.Link;
				e.DragUIOverride.Caption = lang.GetString("CategoriesEdit/DragAdd");
			}
		}

		private async void CategoryArchives_Drop(object sender, DragEventArgs e)
		{
			var deferral = e.GetDeferral();
			if (e.DataView.Properties.TryGetValue("archivesAdd", out object value) && value is string data)
				foreach (var c in JsonConvert.DeserializeObject<List<Archive>>(data))
					await ViewModel.AddToCategory(c.arcid);
			deferral.Complete();
		}

		private void ArchivesGrid_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
		{
			e.Data.RequestedOperation = DataPackageOperation.Link;
			if (e.Items.Any())
				e.Data.Properties.Add("archivesAdd", JsonConvert.SerializeObject(e.Items.ToList()));
		}

		// Remove
		private void ArchivesGrid_DragOver(object sender, DragEventArgs e)
		{
			if (e.DataView.Properties.ContainsKey("archivesRemove"))
			{
				e.AcceptedOperation = DataPackageOperation.Move;
				e.DragUIOverride.Caption = lang.GetString("CategoriesEdit/DragRemove");
			}
		}

		private async void ArchivesGrid_Drop(object sender, DragEventArgs e)
		{
			var deferral = e.GetDeferral();
			if (e.DataView.Properties.TryGetValue("archivesRemove", out object value) && value is string data)
				foreach (var c in JsonConvert.DeserializeObject<List<Archive>>(data))
					await ViewModel.RemoveFromCategory(c.arcid);
			deferral.Complete();
		}

		private void CategoryArchives_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
		{
			e.Data.RequestedOperation = DataPackageOperation.Move;
			if (e.Items.Any())
				e.Data.Properties.Add("archivesRemove", JsonConvert.SerializeObject(e.Items.ToList()));
		}

	}
}
