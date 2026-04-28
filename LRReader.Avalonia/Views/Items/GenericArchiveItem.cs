using Avalonia.Animation.Easings;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Rendering.Composition;
using LRReader.Avalonia.Extensions;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;

namespace LRReader.Avalonia.Views.Items;

public class GenericArchiveItem : TemplatedControl
{
	public ArchiveItemViewModel ViewModel { get; }

	private Grid? Root;

	// Internal
	public List<Archive> Group = null!;

	private MenuItem? openTab, download;
	private bool _loaded;
	private TaskCompletionSource tcs = new();

	public GenericArchiveItem()
	{
		ViewModel = Service.Services.GetRequiredService<ArchiveItemViewModel>();
		ViewModel.Show += Show;
		ViewModel.Hide += Hide;
		DataContextChanged += Control_DataContextChanged;
	}

	protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
	{
		// This is running AFTER context changed
		base.OnApplyTemplate(e);
		openTab?.Click -= MenuFlyoutItem_Click;
		download?.Click += DownloadMenuItem_Click;

		Root = e.NameScope.Get<Grid>("Root");

		openTab = e.NameScope.Get<MenuItem>("OpenTabMenuItem");
		download = e.NameScope.Get<MenuItem>("DownloadMenuItem");

		openTab.Click += MenuFlyoutItem_Click;
		download.Click += DownloadMenuItem_Click;
		if (!_loaded)
			tcs.SetResult();
		_loaded = true;
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

	private Task Show(bool animate)
	{
		if (animate)
		{
			var visual = ElementComposition.GetElementVisual(Root!)!;
			var compositor = visual.Compositor;
			var animation = compositor.CreateScalarKeyFrameAnimation();
			animation.InsertKeyFrame(0.0f, 0.0f);
			animation.InsertKeyFrame(1.0f, 1.0f, new QuadraticEaseIn());
			animation.Duration = TimeSpan.FromMilliseconds(150);

			visual.StartAnimation("Opacity", animation);
		}
		else
			Root?.SetVisualOpacity(1);
		return Task.CompletedTask;
	}

	private Task Hide(bool animate)
	{
		Root?.SetVisualOpacity(0);
		return Task.CompletedTask;
	}

	private async void Control_DataContextChanged(object? sender, EventArgs e)
	{
		if (!_loaded)
			await tcs.Task;
		if (DataContext is Archive archive)
			await ViewModel.Load(archive, DecodePixelWidth, DecodePixelHeight);
	}

	public async void Phase0()
	{
		await ViewModel.Phase0();
	}

	public void Phase1(Archive archive)
	{
		ViewModel.Phase1(archive);
	}

	public async void Phase2()
	{
		await ViewModel.Phase2(DecodePixelWidth, DecodePixelHeight);
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