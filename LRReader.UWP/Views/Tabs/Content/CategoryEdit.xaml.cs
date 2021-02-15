using LRReader.Internal;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.UWP.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using RefreshContainer = Microsoft.UI.Xaml.Controls.RefreshContainer;
using RefreshRequestedEventArgs = Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs;

namespace LRReader.UWP.Views.Tabs.Content
{

	internal class CustomSearchViewModel : SearchResultsViewModel
	{
		internal Category category;

		protected override bool CustomArchiveCheck(Archive archive)
		{
			if (category != null)
				return !category.archives.Contains(archive.arcid);
			else
				return true;
		}
	}

	public sealed partial class CategoryEdit : UserControl
	{

		public CategoryEditViewModel ViewModel;
		private CustomSearchViewModel Data;

		public CategoryEdit()
		{
			this.InitializeComponent();
			ViewModel = new CategoryEditViewModel();
			ArchiveList.Data = Data = new CustomSearchViewModel();
		}

		internal void SetCategoryInternal(Category category) => Data.category = category;

		public void LoadCategory(Category category)
		{
			ViewModel.LoadCategory(category);
			Data.category = ViewModel.category;
		}

		public async Task Refesh() => await ViewModel.Refresh();

		private async void Refresh_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			args.Handled = true;
			await ViewModel.Refresh();
			await Data.ReloadSearch();
		}

		private void CategoryName_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
		{
			bool allow = true;
			CategoryError.Text = "";
			if (string.IsNullOrEmpty(sender.Text))
			{
				CategoryError.Text = "Empty Category Name";
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
				e.DragUIOverride.Caption = "Add to category";
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
				e.DragUIOverride.Caption = "Remove from category";
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
