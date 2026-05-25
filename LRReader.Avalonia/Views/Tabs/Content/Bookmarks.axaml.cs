using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Messaging;
using LRReader.Avalonia.Resources;
using LRReader.Shared;
using LRReader.Shared.Extensions;
using LRReader.Shared.Messages;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LRReader.Avalonia.Views.Tabs.Content;

public partial class Bookmarks : UserControl
{
	public BookmarksTabViewModel Data { get; }

	private ResourceLoader lang;

	public Bookmarks()
	{
		InitializeComponent();
		DataContext = Data = Service.Services.GetRequiredService<BookmarksTabViewModel>();
		lang = ResourceLoader.GetForCurrentView("Tabs");
	}

	private async void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		await Data.Reload();
	}

	private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
	{
		var deferral = args.GetDeferral();
		await Data.Reload(false);
		deferral.Complete();
	}

	/*private void ArchivesGrid_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
	{
		if (args.ItemContainer.ContentTemplateRoot is GenericArchiveItem item)
			if (item.Parallax != null && item.Parallax.Source == null)
				item.Parallax.Source = ArchivesGrid;
	}*/

	[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026")]
	[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050")]
	public async void ExportBookmarks()
	{
		var storage = TopLevel.GetTopLevel(this)!.StorageProvider;

		var savePicker = new FilePickerSaveOptions
		{
			SuggestedStartLocation = await storage.TryGetWellKnownFolderAsync(WellKnownFolder.Documents),
			DefaultExtension = "json"
		};
		var fileName = "bookmarks " + Service.Settings.Profile.Name;
		var validate = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))));
		savePicker.SuggestedFileName = validate.Replace(fileName, "");

		using var file = await storage.SaveFilePickerAsync(savePicker);
		if (file != null)
		{
			using var stream = await file.OpenWriteAsync();
			using var writer = new StreamWriter(stream);
			await writer.WriteAsync(JsonSerializer.Serialize(Service.Settings.Profile.Bookmarks, JsonSettings.Options));
			WeakReferenceMessenger.Default.Send(new ShowNotification(lang.GetString("Bookmarks/ExportComplete"), lang.GetString("Bookmarks/ExportCompleteFile").AsFormat(file.Path), 8000, NotificationSeverity.Informational));
			//WeakReferenceMessenger.Default.Send(new ShowNotification(lang.GetString("Bookmarks/ExportError"), lang.GetString("Bookmarks/ExportErrorFile").AsFormat(file.Path), 8000, NotificationSeverity.Informational));
		}
	}

	[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026")]
	[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050")]
	public async void ImportBookmarks()
	{
		var storage = TopLevel.GetTopLevel(this)!.StorageProvider;

		var openPicker = new FilePickerOpenOptions
		{
			SuggestedStartLocation = await storage.TryGetWellKnownFolderAsync(WellKnownFolder.Documents),
			FileTypeFilter = [new FilePickerFileType("json")],
			AllowMultiple = false
		};

		var files = await storage.OpenFilePickerAsync(openPicker);
		if (files.Count > 0)
		{
			List<Shared.Models.Main.BookmarkedArchive>? bookmarks;
			try
			{
				using var file = files[0];
				using var stream = await file.OpenReadAsync();
				using var reader = new StreamReader(stream);
				bookmarks = JsonSerializer.Deserialize<List<Shared.Models.Main.BookmarkedArchive>>(await reader.ReadToEndAsync(), JsonSettings.Options)!;
			}
			catch (Exception e)
			{
				WeakReferenceMessenger.Default.Send(new ShowNotification(lang.GetString("Bookmarks/ImportError"), e.Message, 0, NotificationSeverity.Error));
				return;
			}
			var result = await Service.Platform.OpenGenericDialog(lang.GetString("Bookmarks/ImportDialog/Title"), lang.GetString("Bookmarks/ImportDialog/PrimaryButtonText"), closebutton: lang.GetString("Bookmarks/ImportDialog/CloseButtonText"), content: lang.GetString("Bookmarks/ImportDialog/Content").AsFormat("\n"));
			if (result == Shared.Models.IDialogResult.Primary)
			{
				Service.Settings.Profile.Bookmarks = bookmarks;
				Service.Settings.SaveProfiles();
				Service.Tabs.CloseAllTabs();
				Service.Platform.GoToPage(Pages.Loading, PagesTransition.DrillIn);
			}
		}
	}
}