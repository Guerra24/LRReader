using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;

namespace LRReader.Avalonia.Views.Items;

public class GenericArchiveItem : TemplatedControl
{
	public ArchiveItemViewModel ViewModel { get; }

	// Internal
	public List<Archive> Group = null!;

	private MenuItem? openTab, download;

	public GenericArchiveItem()
	{
		ViewModel = Service.Services.GetRequiredService<ArchiveItemViewModel>();
		DataContextChanged += Control_DataContextChanged;
	}

	protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
	{
		// This is running AFTER context changed
		base.OnApplyTemplate(e);
		openTab?.Click -= MenuFlyoutItem_Click;
		download?.Click += DownloadMenuItem_Click;

		openTab = e.NameScope.Get<MenuItem>("OpenTabMenuItem");
		download = e.NameScope.Get<MenuItem>("DownloadMenuItem");

		openTab.Click += MenuFlyoutItem_Click;
		download.Click += DownloadMenuItem_Click;
	}

	private async void Control_DataContextChanged(object? sender, EventArgs e)
	{
		if (DataContext is Archive archive)
			await ViewModel.Load(archive, DecodePixelWidth, DecodePixelHeight);
	}

	public int DecodePixelWidth
	{
		get => GetValue(DecodePixelWidthProperty);
		set => SetValue(DecodePixelWidthProperty, value);
	}
	public int DecodePixelHeight
	{
		get => GetValue(DecodePixelHeightProperty);
		set => SetValue(DecodePixelHeightProperty, value);
	}

	public async void MenuFlyoutItem_Click(object? sender, RoutedEventArgs e) => await ViewModel.OpenTab(Group);

	public async void DownloadMenuItem_Click(object? sender, RoutedEventArgs e)
	{
		ViewModel.Downloading = true;
		var download = await ViewModel.DownloadArchive();
		if (download == null)
		{
			ViewModel.Downloading = false;
			return;
		}

		var storage = TopLevel.GetTopLevel(this)!.StorageProvider;

		var savePicker = new FilePickerSaveOptions
		{
			SuggestedStartLocation = await storage.TryGetWellKnownFolderAsync(WellKnownFolder.Downloads),
			DefaultExtension = download.Type,
			SuggestedFileName = download.Name
		};

		var file = await storage.SaveFilePickerAsync(savePicker);
		ViewModel.Downloading = false;
		if (file != null)
		{
			using var stream = await file.OpenWriteAsync();
			await stream.WriteAsync(download.Data);
		}

	}

	public static readonly StyledProperty<int> DecodePixelWidthProperty = AvaloniaProperty.Register<GenericArchiveItem, int>("DecodePixelWidth");
	public static readonly StyledProperty<int> DecodePixelHeightProperty = AvaloniaProperty.Register<GenericArchiveItem, int>("DecodePixelHeight");
}