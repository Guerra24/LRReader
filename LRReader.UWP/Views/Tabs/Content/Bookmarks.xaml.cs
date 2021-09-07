using LRReader.Shared.Extensions;
using LRReader.Shared.Messages;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using LRReader.UWP.Views.Items;
using LRReader.UWP.Views.Main;
using Microsoft.Toolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Archive = LRReader.Shared.Models.Main.Archive;
using RefreshContainer = Microsoft.UI.Xaml.Controls.RefreshContainer;
using RefreshRequestedEventArgs = Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs;

namespace LRReader.UWP.Views.Tabs.Content
{
	public sealed partial class Bookmarks : UserControl
	{
		private BookmarksTabViewModel Data;

		private ResourceLoader lang;

		public Bookmarks()
		{
			this.InitializeComponent();
			Data = DataContext as BookmarksTabViewModel;
			lang = ResourceLoader.GetForCurrentView("Tabs");
		}

		private async void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			await Data.Refresh();
		}

		private void ArchivesGrid_ItemClick(object sender, ItemClickEventArgs e) => Service.Archives.OpenTab(e.ClickedItem as Archive);

		private async void Button_Click(object sender, RoutedEventArgs e) => await Data.Refresh();

		private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
		{
			using (var deferral = args.GetDeferral())
			{
				await Data.Refresh(false);
			}
		}

		private async void Refresh_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			args.Handled = true;
			await Data.Refresh();
		}

		private void ArchivesGrid_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			if (args.ItemContainer.ContentTemplateRoot is GenericArchiveItem item)
				if (item.Parallax != null && item.Parallax.Source == null)
					item.Parallax.Source = ArchivesGrid;
		}

		public async void Refresh() => await Data.Refresh();

		public async void ExportBookmarks()
		{
			var savePicker = new FileSavePicker();
			savePicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
			savePicker.FileTypeChoices.Add("JSON File", new List<string>() { ".json" });
			var fileName = "bookmarks " + Service.Settings.Profile.Name;
			var validate = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))));
			savePicker.SuggestedFileName = validate.Replace(fileName, "");

			StorageFile file = await savePicker.PickSaveFileAsync();
			if (file != null)
			{
				CachedFileManager.DeferUpdates(file);
				await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(Service.Settings.Profile.Bookmarks));
				FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
				if (status == FileUpdateStatus.Complete)
				{
					WeakReferenceMessenger.Default.Send(new ShowNotification(lang.GetString("Bookmarks/ExportComplete"), lang.GetString("Bookmarks/ExportCompleteFile").AsFormat(file.Path), 8000));
				}
				else
				{
					WeakReferenceMessenger.Default.Send(new ShowNotification(lang.GetString("Bookmarks/ExportError"), lang.GetString("Bookmarks/ExportErrorFile").AsFormat(file.Path), 8000));
				}
			}
			else
			{
				//cancel
			}
		}

		public async void ImportBookmarks()
		{
			var openPicker = new FileOpenPicker();
			openPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
			openPicker.FileTypeFilter.Add(".json");

			StorageFile file = await openPicker.PickSingleFileAsync();
			if (file != null)
			{
				List<Shared.Models.Main.BookmarkedArchive> bookmarks = null;
				try
				{
					bookmarks = JsonConvert.DeserializeObject<List<Shared.Models.Main.BookmarkedArchive>>(await FileIO.ReadTextAsync(file));
				}
				catch (Exception e)
				{
					WeakReferenceMessenger.Default.Send(new ShowNotification(lang.GetString("Bookmarks/ImportError"), e.Message, 0));
					return;
				}
				var dialog = new ContentDialog()
				{
					Title = lang.GetString("Bookmarks/ImportDialog/Title"),
					Content = lang.GetString("Bookmarks/ImportDialog/Content").AsFormat("\n"),
					PrimaryButtonText = lang.GetString("Bookmarks/ImportDialog/PrimaryButtonText"),
					CloseButtonText = lang.GetString("Bookmarks/ImportDialog/CloseButtonText")
				};
				var result = await dialog.ShowAsync();
				if (result == ContentDialogResult.Primary)
				{
					Service.Settings.Profile.Bookmarks = bookmarks;
					Service.Settings.SaveProfiles();
					Service.Tabs.CloseAllTabs();
					(Window.Current.Content as Frame).Navigate(typeof(LoadingPage), null, new DrillInNavigationTransitionInfo());
				}
			}
		}
	}

	public class BookmarkTemplateSelector : DataTemplateSelector
	{
		public DataTemplate CompactTemplate { get; set; }
		public DataTemplate FullTemplate { get; set; }

		protected override DataTemplate SelectTemplateCore(object item)
		{
			if (Service.Settings.CompactBookmarks)
				return CompactTemplate;
			else
				return FullTemplate;
		}
		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			return SelectTemplateCore(item);
		}
	}
}
