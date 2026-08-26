using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using CommunityToolkit.Mvvm.Input;

#if WINDOWS_UWP
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Core;
using Windows.Devices.Input;
using LRReader.UWP.Extensions;
using LRReader.UWP.Views.Items;
using PointerEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
using PointerPressedEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
using PointerReleasedEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
using KeyEventArgs = Windows.UI.Xaml.Input.KeyRoutedEventArgs;
using ScrollChangedEventArgs = Windows.UI.Xaml.Controls.ScrollViewerViewChangedEventArgs;
using Key = Windows.System.VirtualKey;
using RefreshContainer = Microsoft.UI.Xaml.Controls.RefreshContainer;
using RefreshRequestedEventArgs = Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs;

namespace LRReader.UWP.Views.Tabs.Content;
#else
using Avalonia.Animation.Easings;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Platform.Storage;
using LRReader.Avalonia.Extensions;
using LRReader.Avalonia.Views.Controls;
using LRReader.Avalonia.Views.Items;
using DoubleTappedRoutedEventArgs = Avalonia.Input.TappedEventArgs;

namespace LRReader.Avalonia.Views.Tabs.Content;
#endif

public partial class ArchiveTabContent : UserControl
{
	public ArchivePageViewModel Data { get; }

	private bool _wasNew;
	private bool _opened;
	private bool _focus = true;
	private bool _changingPage;
	private float _lastZoom;
	private double _fitAgainstFixedWidth;
	private bool _overlayDelayOpen;

	private bool _transition;

	private bool _open;

	private int gcCounter;

	private TimeSpan _previousTime = TimeSpan.Zero;

	private SemaphoreSlim _loadSemaphore = new SemaphoreSlim(1);

	private ArchiveTabState? archiveState;

	private bool Animate => Service.Platform.AnimationsEnabled && Service.Settings.ReaderAnimations;

	private async void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
#if !WINDOWS_UWP
		if (Service.Settings.UseReaderBackground)
			ReaderBackground[!BackgroundProperty] = new DynamicResourceExtension("CustomReaderBackground");
		else
			ReaderBackground[!BackgroundProperty] = new DynamicResourceExtension("ReaderBackground");
#endif

		Data.ReloadBookmarkedObject();
		FocusReader();
		if (!_opened)
		{
			await _loadSemaphore.WaitAsync();
			await Data.HandleConflict();
			_loadSemaphore.Release();
			if (_open)
			{
				var page = 0;
				if (Data.Bookmarked)
					page = Data.BookmarkProgress;
				OpenReader(archiveState?.Page ?? page);
			}
			_opened = true;
		}
		archiveState = null;
	}

	public async void LoadArchive(Archive archive, List<Archive>? next = null, ArchiveTabState? state = null)
	{
		Data.Archive = archive;
		if (next != null)
			Data.Group = next;
		if (state?.Next != null)
			Data.Group = [.. (await Task.WhenAll(state.Next.Select(Service.Archives.GetOrAddArchive).ToList())).Where(a => a != null).Select(a => a!)];
		if (_open = state?.WasOpen ?? false || Service.Settings.OpenReader)
#if WINDOWS_UWP
			RefreshContainer.Visibility = Visibility.Collapsed;
#else
			StackRoot.IsVisible = false;
#endif
		archiveState = state;
		await Data.Reload();
		_loadSemaphore.Release();
	}

	private async void OpenReader(int page, object? item = null)
	{
		var readerSet = Data.ArchiveImagesReader.FirstOrDefault(s => s.Page >= page);
		if (readerSet == null)
			return;
		if (_transition)
			return;
		_transition = true;
		var index = Data.ArchiveImagesReader.IndexOf(readerSet);

		if (Animate && item != null && !Data.UseVerticalReader)
		{
#if WINDOWS_UWP
			var image = ImagesGrid.ContainerFromItem(item).FindDescendant("Thumbnail");
			if (image != null && !(image.ActualWidth == 0 || image.ActualHeight == 0))
			{
				var anim = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(GetOpenTarget(readerSet, page), image);
				anim.Configuration = new BasicConnectedAnimationConfiguration();
			}
#endif
		}

		Data.ShowReader = true;
		Data.ReaderIndex = index;
		if (Data.UseVerticalReader)
		{
			await Task.Delay(100);
			var element = ReaderVertical.GetOrCreateElement(index);
			element.UpdateLayout();
#if WINDOWS_UWP
			element.StartBringIntoView(new BringIntoViewOptions { AnimationDesired = false, VerticalAlignmentRatio = 0f });
#else
			element.BringIntoView();
#endif
		}
		else
			await ChangePage(false);

		if (Data.Archive.isnew)
			_wasNew = true;
		if (Animate)
		{
#if WINDOWS_UWP
			await Task.WhenAll(FadeIn.StartAsync(ReaderBackground), FadeIn.StartAsync(ScrollViewer));
#else
			await Task.WhenAll(ReaderBackground.FadeInAsync(TimeSpan.FromMilliseconds(200), new QuadraticEaseIn()), ScrollViewer.FadeInAsync(TimeSpan.FromMilliseconds(200), new QuadraticEaseIn()));
			ReaderBackground.SetOpacity(1);
			ScrollViewer.SetOpacity(1);
#endif
		}
		else
		{
#if WINDOWS_UWP
			ReaderBackground.SetVisualOpacity(1);
			ScrollViewer.SetVisualOpacity(1);
#else
			ReaderBackground.SetOpacity(1);
			ScrollViewer.SetOpacity(1);
#endif
		}

		_focus = true;
		FocusReader();

		_transition = false;
		await PlayStop(Service.Settings.Autoplay);
	}

#if WINDOWS_UWP
	private async void NextArchive() => await NextArchiveAsync();
#else
	private async void NextArchive(object? sender, RoutedEventArgs e) => await NextArchiveAsync();
#endif

	private async Task NextArchiveAsync()
	{
		if (!Data.CanGoNext)
			return;
		if (_transition)
			return;
		_transition = true;
		await HideReader();
		await Data.PrevNextArchive(1);
		await ShowReader();
		Data.PageCounter = 0;
		_transition = false;
	}

#if WINDOWS_UWP
	private async void PrevArchive() => await PrevArchiveAsync();
#else
	private async void PrevArchive(object? sender, RoutedEventArgs e) => await PrevArchiveAsync();
#endif

	private async Task PrevArchiveAsync()
	{
		if (!Data.CanGoPrev)
			return;
		if (_transition)
			return;
		_transition = true;
		await HideReader();
		await Data.PrevNextArchive(-1);
		await ShowReader(Service.Settings.OpenPrevOrNextLastPage ? Data.Pages - 1 : 0);
		Data.PageCounter = 0;
		_transition = false;
	}

	[RelayCommand]
	private async Task Random(bool newOnly)
	{
		if (_transition)
			return;
		_transition = true;
		var list = Service.Archives.Archives.Where(kv => kv.Value.isnew || !newOnly);
		if (list.Count() <= 1)
			return;
		var random = new Random();
		var item = list.ElementAt(random.Next(list.Count() - 1));

		await HideReader();
		await Data.OpenArchive(item.Value);
		await ShowReader();
		_transition = false;
	}

	private async void RebuildReader()
	{
		// Reentrancy can crash here
		int page = 0;
		if (Data.ShowReader)
		{
			_transition = true;
			page = Data.ReaderContent.Page;
			Data.ClearImageSets();
			await HideReader();
		}
		await Data.CreateImageSets();
		if (Data.ShowReader)
		{
			await ShowReader(page);
			_transition = false;
		}
	}

	private async Task HideReader()
	{
		if (Data.ShowReader)
		{
			_wasNew = await Data.SaveReaderData(_wasNew);
#if WINDOWS_UWP
			if (Animate)
				await FadeOut.StartAsync(ScrollViewer);
			else
				ScrollViewer.SetVisualOpacity(0);
#else
			if (Animate)
				await ScrollViewer.FadeOutAsync(TimeSpan.FromMilliseconds(200), new QuadraticEaseIn());
			else
				ScrollViewer.SetOpacity(0);
#endif
		}
		else
		{
#if WINDOWS_UWP
			if (Animate)
				await ImagesGrid.FadeOutAsync();
			else
				ImagesGrid.SetVisualOpacity(0);
#else
			if (Animate)
				await StackRoot.FadeOutAsync(TimeSpan.FromMilliseconds(250), new QuadraticEaseOut());
			else
				StackRoot.SetOpacity(0);
#endif
		}
	}

	private async Task ShowReader(int page = 0)
	{
		if (Data.ShowReader)
		{
			var readerSet = Data.ArchiveImagesReader.FirstOrDefault(s => s.Page >= page);
			if (readerSet == null)
				return;
			var index = Data.ArchiveImagesReader.IndexOf(readerSet);
			Data.ReaderIndex = index;

			if (Data.UseVerticalReader)
			{
				await Task.Delay(100);
				var element = ReaderVertical.GetOrCreateElement(index);
				element.UpdateLayout();
#if WINDOWS_UWP
				element.StartBringIntoView(new BringIntoViewOptions { AnimationDesired = false, VerticalAlignmentRatio = 0f });
#else
				element.BringIntoView();
#endif
			}
			else
				await ChangePage();

#if WINDOWS_UWP
			if (Animate)
				await FadeIn.StartAsync(ScrollViewer);
			else
				ScrollViewer.SetVisualOpacity(1);
#else
			if (Animate)
				await ScrollViewer.FadeInAsync(TimeSpan.FromMilliseconds(200), new QuadraticEaseIn());
			else
				ScrollViewer.SetOpacity(1);
#endif
			FocusReader();
		}
		else
		{
#if WINDOWS_UWP
			if (Animate)
				await ImagesGrid.FadeInAsync();
			else
				ImagesGrid.SetVisualOpacity(1);
#else
			if (Animate)
				await StackRoot.FadeInAsync(TimeSpan.FromMilliseconds(250), new QuadraticEaseIn());
			else
				StackRoot.SetOpacity(1);
#endif
		}
	}

	private void ImagesGrid_ItemClick(object? sender, ItemClickEventArgs e)
	{
		if (!Data.ControlsEnabled)
			return;
		OpenReader(Data.ArchiveImages.IndexOf((ImagePageSet)e.ClickedItem), e.ClickedItem);
	}

	private void Continue_Click(object? sender, RoutedEventArgs e)
	{
		if (!Data.ControlsEnabled)
			return;
		OpenReader(Data.BookmarkProgress);
	}

	private void CloseButton_Click(object? sender, RoutedEventArgs e)
	{
		if (!Data.ShowReader)
			return;
		CloseReader();
	}

	private void ReaderControl_KeyUp(object? sender, KeyEventArgs e)
	{
		if (!Data.ShowReader)
			return;
		if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Space ||
			 e.Key == Key.Escape || e.Key == Key.D || e.Key == Key.A || e.Key == Key.W || e.Key == Key.S)
			e.Handled = true;
	}

	private void ReaderControl_KeyDown(object? sender, KeyEventArgs e)
	{
		if (!Data.ShowReader || _changingPage)
			return;

#if WINDOWS_UWP
		var ctrl = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control);
		var alt = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Menu);

		if ((ctrl & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down || (alt & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
			return;
#else
		var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
		var alt = e.KeyModifiers.HasFlag(KeyModifiers.Alt);

		if (ctrl || alt)
			return;
#endif

		if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Space ||
			 e.Key == Key.Escape || e.Key == Key.D || e.Key == Key.A || e.Key == Key.W || e.Key == Key.S)
		{
			e.Handled = true;
			FocusReader();
		}
#if WINDOWS_UWP
		double offset = ScrollViewer.VerticalOffset;
#else
		double offset = ScrollViewer.Offset.Y;
#endif
		switch (e.Key)
		{
			case Key.Up:
			case Key.W:
				if (Math.Floor(offset) <= 0 && Service.Settings.ScrollToChangePage)
					PrevPage(true);
				else
#if WINDOWS_UWP
					ScrollViewer.ChangeView(null, offset - Service.Settings.KeyboardScroll, null, false);
#else
					ScrollViewer.Offset = new Vector(ScrollViewer.Offset.X, offset - Service.Settings.KeyboardScroll);
#endif 
				break;
			case Key.Down:
			case Key.Space:
			case Key.S:
#if WINDOWS_UWP
				if ((ScrollViewer.ScrollableHeight - offset) < 5 && Service.Settings.ScrollToChangePage)
#else
				if ((ScrollViewer.Extent.Height - ScrollViewer.Viewport.Height - offset) < 5 && Service.Settings.ScrollToChangePage)
#endif
					NextPage(true);
				else
#if WINDOWS_UWP
					ScrollViewer.ChangeView(null, offset + Service.Settings.KeyboardScroll, null, false);
#else
					ScrollViewer.Offset = new Vector(ScrollViewer.Offset.X, offset + Service.Settings.KeyboardScroll);
#endif
				break;
			case Key.Right:
			case Key.D:
				NextPage();
				break;
			case Key.Left:
			case Key.A:
				PrevPage();
				break;
			case Key.Escape:
				CloseReader();
				break;
#if WINDOWS_UWP
			case (VirtualKey)190:
				NextArchive();
				break;
			case (VirtualKey)188:
				PrevArchive();
				break;
#else
			case Key.OemPeriod:
				NextArchive(null, null!);
				break;
			case Key.OemComma:
				PrevArchive(null, null!);
				break;
#endif
		}
	}

	private void FocusReader()
	{
		if (Data.ShowReader && _focus)
		{
#if WINDOWS_UWP
			ReaderControl.Focus(FocusState.Programmatic);
#else
			ReaderControl.Focus();
#endif
		}
	}

	private void ScrollViewer_PointerPressed(object? sender, PointerPressedEventArgs e)
	{
		e.Handled = true;
	}

	private async void ScrollViewer_PointerRelease(object? sender, PointerReleasedEventArgs e)
	{
		var pointerPoint = e.GetCurrentPoint(ScrollViewer);
		var point = pointerPoint.Position;
#if WINDOWS_UWP
		var width = ScrollViewer.ActualWidth;
#else
		var width = ScrollViewer.Bounds.Width;
#endif
		double distance = width / 6.0;
		if (point.X > distance && point.X < width - distance)
		{
			//_handleDoubleTap = pointerPoint.Properties.IsLeftButtonPressed;
		}
		else
		{
			if (pointerPoint.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased || pointerPoint.Properties.PointerUpdateKind == PointerUpdateKind.RightButtonReleased)
			{
				e.Handled = HandleTapped(point);
			}
		}
#if WINDOWS_UWP
		if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
#else
		if (e.Pointer.Type == PointerType.Mouse)
#endif
		{
			switch (pointerPoint.Properties.PointerUpdateKind)
			{
				case PointerUpdateKind.XButton1Released:
					e.Handled = true;
					FocusReader();
					PrevPage();
					return;
				case PointerUpdateKind.XButton2Released:
					e.Handled = true;
					FocusReader();
					NextPage();
					return;
				case PointerUpdateKind.MiddleButtonReleased:
					if (!Service.Settings.ShowMap)
						break;
					e.Handled = true;
					await OpenOverlay();
					break;
			}
		}
	}

	private void ScrollViewer_DoubleTapped(object? sender, DoubleTappedRoutedEventArgs e)
	{
		var point = e.GetPosition(ScrollViewer);
#if WINDOWS_UWP
		var width = ScrollViewer.ActualWidth;
#else
		var width = ScrollViewer.Bounds.Width;
#endif
		double distance = width / 6.0;
		if (point.X > distance && point.X < width - distance)
		{
			Service.Platform.ToggleFullScreenMode();
			e.Handled = true;
		}
	}

	private async void ScrollViewer_Holding(object? sender, HoldingRoutedEventArgs e)
	{
#if WINDOWS_UWP
		if (!Service.Settings.ShowMap)
			return;
		var point = e.GetPosition(ScrollViewer);
		double distance = ScrollViewer.ActualWidth / 6.0;
		if (point.X > distance && point.X < ScrollViewer.ActualWidth - distance)
		{
			await OpenOverlay();
			e.Handled = true;
		}
#endif
	}

	private bool HandleTapped(Point point)
	{
#if WINDOWS_UWP
		var width = ScrollViewer.ActualWidth;
#else
		var width = ScrollViewer.Bounds.Width;
#endif
		double distance = width / 6.0;
		if (point.X < distance)
		{
			PrevPage();
			return true;
		}
		else if (point.X > width - distance)
		{
			NextPage();
			return true;
		}
		return false;
	}

	private async void NextPage(bool ignore = false)
	{
		if (_transition)
			return;
		_changingPage = true;
		if (Data.UseAutoplay)
			await Task.Delay(TimeSpan.FromMilliseconds(Service.Settings.AutoplayBeforeChangeDelay));
		if (Data.ReadRTL && !ignore)
			await GoLeft();
		else
			await GoRight();
		if (Data.UseAutoplay)
			await Task.Delay(TimeSpan.FromMilliseconds(Service.Settings.AutoplayAfterChangeDelay));
		Data.PageCounter++;
		_changingPage = false;
	}

	private async void PrevPage(bool ignore = false)
	{
		if (_transition)
			return;
		_changingPage = true;
		if (Data.UseAutoplay)
			await Task.Delay(TimeSpan.FromMilliseconds(Service.Settings.AutoplayBeforeChangeDelay));
		if (Data.ReadRTL && !ignore)
			await GoRight();
		else
			await GoLeft();
		if (Data.UseAutoplay)
			await Task.Delay(TimeSpan.FromMilliseconds(Service.Settings.AutoplayAfterChangeDelay));
		Data.PageCounter--;
		_changingPage = false;
	}

	private async Task GoRight()
	{
		if (Data.UseVerticalReader || _transition)
			return;
		if (Service.Settings.OpenPrevOrNext && Data.ReaderContent.Page + 1 >= Data.Pages)
		{
			await NextArchiveAsync();
			return;
		}
		if (Data.ReaderIndex < Data.ArchiveImagesReader.Count() - 1)
		{
			++Data.ReaderIndex;
			await ReaderImage.FadeOutPage();
#if WINDOWS_UWP
			ScrollViewer.ChangeView(null, 0, null, true);
#else
			ScrollViewer.Offset = new Vector(ScrollViewer.Offset.X, 0);
#endif
			await ChangePage();
		}
	}

	private async Task GoLeft()
	{
		if (Data.UseVerticalReader || _transition)
			return;
		if (Service.Settings.OpenPrevOrNext && Data.ReaderContent.Page == 0)
		{
			await PrevArchiveAsync();
			return;
		}
		if (Data.ReaderIndex > 0)
		{
			--Data.ReaderIndex;
			await ReaderImage.FadeOutPage();
#if WINDOWS_UWP
			ScrollViewer.ChangeView(null, 0, null, true);
#else
			ScrollViewer.Offset = new Vector(ScrollViewer.Offset.X, 0);
#endif
			await ChangePage();
		}
	}

	private async Task ChangePage(bool preload = true)
	{
		if (Data.UseVerticalReader)
			return;
		await ReaderImage.ChangePage(Data.ReaderContent);
		ReaderImage.FadeInPage();
		gcCounter++;
		if (gcCounter > 20)
		{
			// Turns out CsWinRT creates a lot of trash in the heap so we need to clear it to prevent stalls
			GC.Collect(0, GCCollectionMode.Forced, false, false);
			gcCounter = 0;
		}

		if (preload)
		{
			await Preload(Data.ArchiveImagesReader.ElementAtOrDefault(Data.ReaderIndex + 1));
			await Preload(Data.ArchiveImagesReader.ElementAtOrDefault(Data.ReaderIndex + 2));
			await Preload(Data.ArchiveImagesReader.ElementAtOrDefault(Data.ReaderIndex + 3));
		}
	}

	private async Task Preload(ReaderImageSet? set)
	{
		if (set == null)
			return;
		await Service.Images.GetImageCached(set.LeftImage);
		await Service.Images.GetImageCached(set.RightImage);
	}

	private void ScrollViewer_SizeChanged(object? sender, SizeChangedEventArgs e)
	{
		FitImages(Data.UseVerticalReader, true);
		ReaderThumbnailOverlay.Width = e.NewSize.Width;
		//LeftHitTargetOverlay.Width = RightHitTargetOverlay.Width = ScrollViewer.ActualWidth / 6.0;
#if WINDOWS_UWP
		ReaderThumbnailOverlayHitArea.Margin = new Thickness(ScrollViewer.ActualWidth / 6.0, 0, ScrollViewer.ActualWidth / 6.0, 0);
#endif
	}

	private void ReaderControl_SizeChanged(object? sender, SizeChangedEventArgs e) => FitImages(true);

	private void FitImages() => FitImages(false);

	private async void ScrollViewer_ViewChanged(object? sender, ScrollChangedEventArgs e)
	{
#if WINDOWS_UWP
		if (e.IsIntermediate)
			return;
#endif
		// Use width instead of height in vertical mode
		if (Data.UseVerticalReader)
		{
			if (ScrollViewer.CurrentAnchor is ReaderImage image)
			{
				var index = ReaderVertical.GetElementIndex(ScrollViewer.CurrentAnchor);
				if (!_transition)
					Data.ReaderIndex = index;

#if WINDOWS_UWP
				var width = (int)Math.Round(ScrollViewer.ExtentWidth);
				await image.ResizeWidth(width);
				(ReaderVertical.TryGetElement(index - 1) as ReaderImage)?.ResizeWidth(width);
				(ReaderVertical.TryGetElement(index + 1) as ReaderImage)?.ResizeWidth(width);
				(ReaderVertical.TryGetElement(index + 2) as ReaderImage)?.ResizeWidth(width);
#else
				_resizerVertical.Invoke((int)Math.Round(ScrollViewer.Extent.Width), image, index);
#endif
			}
		}
		else
#if WINDOWS_UWP
			await ReaderImage.ResizeHeight((int)Math.Round(ScrollViewer.ExtentHeight));
#else
			_resizer.Invoke((int)Math.Round(ScrollViewer.Extent.Height));
#endif
	}

#if WINDOWS_UWP
[DynamicWindowsRuntimeCast(typeof(RenderingEventArgs))]
#endif
	private void CompositionTarget_Rendering(object? sender, object e)
	{
#if WINDOWS_UWP
		var timings = (RenderingEventArgs)e;
		var delta = timings.RenderingTime.TotalSeconds - _previousTime.TotalSeconds;
		if (delta > 0.033)
			delta = 0;
		if (!_changingPage)
		{
			if (ScrollViewer.VerticalOffset >= ScrollViewer.ScrollableHeight)
			{
				NextPage();
			}
			else
			{
				var yOffset = ScrollViewer.VerticalOffset + Service.Settings.AutoplaySpeed * delta * _lastZoom;
				ScrollViewer.ChangeView(null, yOffset, null, true);
			}
		}
		_previousTime = timings.RenderingTime;
#endif
	}

	[RelayCommand]
	private async Task PlayStop(bool state)
	{
#if WINDOWS_UWP
		// Handle user initiated mouse action (disable autoplay)
		Data.UseAutoplay = state;
		if (state)
		{
			ScrollViewer.ChangeView(null, 0, null, true);
			await Task.Delay(TimeSpan.FromMilliseconds(Service.Settings.AutoplayStartDelay));
			CompositionTarget.Rendering += CompositionTarget_Rendering;
		}
		else
			CompositionTarget.Rendering -= CompositionTarget_Rendering;
#endif
	}

	private async void DownloadButton_Click(object? sender, RoutedEventArgs e)
	{
		Data.Downloading = true;
		var download = await Data.DownloadArchive();
		if (download == null)
		{
			Data.Downloading = false;
			return;
		}

#if WINDOWS_UWP
		var savePicker = new FileSavePicker
		{
			SuggestedStartLocation = PickerLocationId.Downloads
		};
		savePicker.FileTypeChoices.Add(download.Type + " File", new List<string>() { download.Type });
		savePicker.SuggestedFileName = download.Name;

		StorageFile file = await savePicker.PickSaveFileAsync();
		Data.Downloading = false;
		if (file != null)
			await FileIO.WriteBytesAsync(file, download.Data);
#else
		var storage = TopLevel.GetTopLevel(this)!.StorageProvider;

		var savePicker = new FilePickerSaveOptions
		{
			SuggestedStartLocation = await storage.TryGetWellKnownFolderAsync(WellKnownFolder.Downloads),
			DefaultExtension = download.Type.Replace(".", ""),
			SuggestedFileName = download.Name
		};

		using var file = await storage.SaveFilePickerAsync(savePicker);
		Data.Downloading = false;
		if (file != null)
		{
			using var stream = await file.OpenWriteAsync();
			await stream.WriteAsync(download.Data);
		}
#endif
	}

#if WINDOWS_UWP
	private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
#else
	private async void RefreshContainer_RefreshRequested(object? sender, RefreshRequestedEventArgs args)
#endif
	{
#if WINDOWS_UWP
		using var deferral = args.GetDeferral();
		await Data.Reload(false);
#else
		var deferral = args.GetDeferral();
		await Data.Reload(false);
		deferral.Complete();
#endif
	}

	public void RemoveEvent()
	{
		Data.ZoomChangedEvent -= FitImages;
		Data.RebuildReader -= RebuildReader;
		Service.Events.RebuildReaderImagesSetEvent -= RebuildReader;
		Data.UnHook();
	}

	private string GetOpenTarget(ReaderImageSet target, int item)
	{
		var targetAnim = "openL";
		if (target.TwoPages)
		{
			if (Data.ReadRTL)
			{
				if (target.Page != item)
					targetAnim = "openR";
			}
			else
			{
				targetAnim = "openR";
				if (target.Page != item)
					targetAnim = "openL";

			}
		}
		return targetAnim;
	}

	private async Task OpenOverlay()
	{
		if (ReaderThumbnailOverlay.IsOpen)
			return;
		ReaderThumbnailOverlay.IsOpen = true;
		await Task.Delay(50);
#if WINDOWS_UWP
		OverlayThumbnails.SelectedIndex = Data.ReaderContent.Page;
		await OverlayThumbnails.SmoothScrollIntoViewWithIndexAsync(Data.ReaderContent.Page, ScrollItemPlacement.Center);
#endif
	}

	private async void Trigger_PointerEntered(object? sender, PointerEventArgs e)
	{
		if (!Data.ShowReader)
			return;
		if (!Service.Settings.ShowMap)
			return;
#if WINDOWS_UWP
		if (e.Pointer.PointerDeviceType == PointerDeviceType.Touch)
#else
		if (e.Pointer.Type == PointerType.Touch)
#endif
			return;
		_overlayDelayOpen = true;
		await Task.Delay(TimeSpan.FromMilliseconds(Service.Platform.HoverTime));
		if (_overlayDelayOpen)
			await OpenOverlay();
	}

	private void Trigger_PointerExited(object? sender, PointerEventArgs e)
	{
		if (!_overlayDelayOpen)
			return;
		_overlayDelayOpen = false;
	}

	public ArchiveTabState GetTabState() => archiveState ?? new ArchiveTabState(Data.Archive.arcid, Data.ReaderContent?.Page, Data.ShowReader, Data.Group.Select(a => a.arcid).ToList());
}
