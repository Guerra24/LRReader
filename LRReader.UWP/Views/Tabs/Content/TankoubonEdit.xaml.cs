using LRReader.Shared;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace LRReader.UWP.Views.Tabs.Content
{

	public sealed partial class TankoubonEdit : UserControl
	{

		public TankoubonEditViewModel ViewModel;

		private ResourceLoader lang;

		public TankoubonEdit()
		{
			this.InitializeComponent();
			ViewModel = Service.Services.GetRequiredService<TankoubonEditViewModel>();
			ArchiveList.Data.CustomArchiveCheckEvent = CustomArchiveCheck;
			lang = ResourceLoader.GetForCurrentView("Tabs");
			VisualStateManager.GoToState(this, "Selected", false);
		}

		private bool CustomArchiveCheck(Archive archive)
		{
			return !ViewModel.Tankoubon.archives.Contains(archive.arcid);
		}

		public async void Load(Shared.Models.Main.Tankoubon tankoubon) => await ViewModel.Load(tankoubon);

		public async Task Refresh()
		{
			await Task.WhenAll(ViewModel.Refresh(), ArchiveList.Data.ReloadSearch());
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
				CategoryError.Text = ResourceLoader.GetForCurrentView("Dialogs").GetString("CreateTankoubon/ErrorName");
				allow = false;
			}
			ViewModel.CanSave = allow;
		}

		private async void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			await ViewModel.Save();
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

		[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026")]
		[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050")]
		private async void CategoryArchives_Drop(object sender, DragEventArgs e)
		{
			var deferral = e.GetDeferral();
			if (e.DataView.Properties.TryGetValue("archivesAdd", out object value) && value is string data)
				foreach (var c in JsonSerializer.Deserialize<List<Archive>>(data, JsonSettings.Options)!)
					await ViewModel.AddToTankoubon(c.arcid);
			deferral.Complete();
		}

		[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026")]
		[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050")]
		private void ArchivesGrid_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
		{
			e.Data.RequestedOperation = DataPackageOperation.Link;
			if (e.Items.Any())
				e.Data.Properties.Add("archivesAdd", JsonSerializer.Serialize(e.Items.ToList(), JsonSettings.Options));
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

		[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026")]
		[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050")]
		private async void ArchivesGrid_Drop(object sender, DragEventArgs e)
		{
			var deferral = e.GetDeferral();
			if (e.DataView.Properties.TryGetValue("archivesRemove", out object value) && value is string data)
				foreach (var c in JsonSerializer.Deserialize<List<Archive>>(data, JsonSettings.Options)!)
					await ViewModel.RemoveFromTankoubon(c.arcid);
			deferral.Complete();
		}

		[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026")]
		[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050")]
		private void CategoryArchives_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
		{
			e.Data.RequestedOperation = DataPackageOperation.Move;
			if (e.Items.Any())
				e.Data.Properties.Add("archivesRemove", JsonSerializer.Serialize(e.Items.ToList(), JsonSettings.Options));
		}

	}
}
