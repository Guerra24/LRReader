using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using LRReader.Avalonia.Extensions;
using LRReader.Avalonia.Views.Controls;
using LRReader.Shared.Extensions;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;

namespace LRReader.Avalonia.Views.Tabs.Content;

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

	private Action<int> _resizer;

	public ArchiveTabContent()
	{
		InitializeComponent();
		/*
		ElementCompositionPreview.SetIsTranslationEnabled(ReaderThumbnailOverlay, true);
		ElementCompositionPreview.GetElementVisual(ReaderThumbnailOverlay).Properties.InsertVector3("Translation", new Vector3(0, 317, 0));
		*/
		ScrollViewer.AddHandler(PointerReleasedEvent, ScrollViewer_PointerRelease);

		Data = (ArchivePageViewModel)DataContext!;
		Data.ZoomChangedEvent += FitImages;
		Data.RebuildReader += RebuildReader;

		_loadSemaphore.Wait();

		Service.Events.RebuildReaderImagesSetEvent += RebuildReader;

		Action<int> resizer = (param) =>
		{
			Service.Dispatcher.Run(async () =>
			{
				await ReaderImage.ResizeHeight(param);
			});
		};

		_resizer = resizer.Debounce(500);
	}

	private async void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		ReaderBackground.SetVisualOpacity(0);
		ScrollViewer.SetVisualOpacity(0);
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
			RefreshContainer.IsVisible = false;
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

		/*if (Animate && item != null && !Data.UseVerticalReader)
		{
			var image = ImagesGrid.ContainerFromItem(item).FindDescendant("Thumbnail");
			if (image != null && !(image.ActualWidth == 0 || image.ActualHeight == 0))
			{
				var anim = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(GetOpenTarget(readerSet, page), image);
				anim.Configuration = new BasicConnectedAnimationConfiguration();
			}
		}*/

		Data.ShowReader = true;
		Data.ReaderIndex = index;
		if (Data.UseVerticalReader)
		{
			await Task.Delay(100);
			/*var element = ReaderVertical.GetOrCreateElement(index);
			element.UpdateLayout();
			element.StartBringIntoView(new BringIntoViewOptions { AnimationDesired = false, VerticalAlignmentRatio = 0f });*/
		}
		else
			await ChangePage(false);

		if (Data.Archive.isnew)
			_wasNew = true;
		/*if (Animate)
		{
			await Task.WhenAll(FadeIn.StartAsync(ReaderBackground), FadeIn.StartAsync(ScrollViewer));
		}
		else*/
		{
			ReaderBackground.SetVisualOpacity(1);
			ScrollViewer.SetVisualOpacity(1);
		}

		_focus = true;
		FocusReader();

		_transition = false;
		await PlayStop(Service.Settings.Autoplay);
	}

	public async void CloseReader()
	{
		if (_transition)
			return;
		_transition = true;
		if (!RefreshContainer.IsVisible)
		{
			RefreshContainer.IsVisible = true;
			RefreshContainer.UpdateLayout();
			await Task.Delay(100); // Otherwise scrollings into view breaks
		}

		await PlayStop(false);
		//ConnectedAnimation? animLeft = null, animRight = null;

		if (!Data.UseVerticalReader)
		{
			//ReaderImage.disableAnimation = true;

			if (Animate)
			{
				/*var left = ReaderImage.FindDescendant("LeftImage");
				var right = ReaderImage.FindDescendant("RightImage");
				if (Data.ReaderContent.LeftImage != null && left != null && !(left.ActualWidth == 0 || left.ActualHeight == 0))
				{
					animLeft = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("closeL", left);
					animLeft.Configuration = new BasicConnectedAnimationConfiguration();
				}
				if (Data.ReaderContent.RightImage != null && right != null && !(right.ActualWidth == 0 || right.ActualHeight == 0))
				{
					animRight = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("closeR", right);
					animRight.Configuration = new BasicConnectedAnimationConfiguration();
				}*/
			}
		}

		_focus = false;
		int currentPage = Data.ReaderContent.Page;
		int count = Data.Pages;

		int leftTarget = currentPage;
		int rightTarget = currentPage;

		if (Data.ReaderContent.TwoPages)
		{
			leftTarget--;
			if (Data.ReadRTL)
			{
				int tmp = leftTarget;
				leftTarget = rightTarget;
				rightTarget = tmp;
			}
		}
		leftTarget = leftTarget.Clamp(0, count - 1);
		rightTarget = rightTarget.Clamp(0, count - 1);
		//var delay = ImagesGrid.ContainerFromIndex(leftTarget) == null ? 200 : 50; // Man
		//await ImagesGrid.SmoothScrollIntoViewWithIndexAsync(leftTarget, disableAnimation: true);
		//await Task.Delay(delay);
		/*if (Animate)
		{
			var leftThumb = ImagesGrid.ContainerFromIndex(leftTarget)?.FindDescendant("Thumbnail");
			var rightThumb = ImagesGrid.ContainerFromIndex(rightTarget)?.FindDescendant("Thumbnail");
			if (Data.ReaderContent.LeftImage != null && leftThumb != null && Data.ArchiveImages.Count > leftTarget)
				animLeft?.TryStart(leftThumb);
			if (Data.ReaderContent.RightImage != null && rightThumb != null && Data.ArchiveImages.Count > rightTarget)
				animRight?.TryStart(rightThumb);
			await Task.WhenAll(FadeOut.StartAsync(ReaderBackground), FadeOut.StartAsync(ScrollViewer));
			await Task.Delay(200); // Give it a sec
		}
		else*/
		{
			ReaderBackground.SetVisualOpacity(0);
			ScrollViewer.SetVisualOpacity(0);
		}
		Data.ShowReader = false;

		_wasNew = await Data.SaveReaderData(_wasNew);

		_transition = false;
		_open = false;
		gcCounter = 0;
		Data.PageCounter = 0;
	}

	private async void NextArchive(object? sender, RoutedEventArgs e) => await NextArchiveAsync();

	private async Task NextArchiveAsync()
	{
		if (!Data.CanGoNext)
			return;
		if (_transition)
			return;
		_transition = true;
		await HideReader();
		await Data.NextArchive();
		await ShowReader();
		Data.PageCounter = 0;
		_transition = false;
	}

	private async void Random_Clicked(object? sender, RoutedEventArgs e) => await Random(false/*(CoreWindow.GetForCurrentThread().GetKeyState(Key.Shift) & CoreKeyStates.Down) == CoreKeyStates.Down*/);

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
			/*if (Animate)
				await FadeOut.StartAsync(ScrollViewer);
			else*/
			ScrollViewer.SetVisualOpacity(0);
		}
		else
		{
			/*if (Animate)
				await ImagesGrid.FadeOutAsync();
			else*/
			ImagesGrid.SetVisualOpacity(0);
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
				/*await Task.Delay(100);
				var element = ReaderVertical.GetOrCreateElement(index);
				element.UpdateLayout();
				element.StartBringIntoView(new BringIntoViewOptions { AnimationDesired = false, VerticalAlignmentRatio = 0f });*/
			}
			else
				await ChangePage();

			/*if (Animate)
				await FadeIn.StartAsync(ScrollViewer);
			else*/
			ScrollViewer.SetVisualOpacity(1);
			FocusReader();
		}
		else
		{
			/*if (Animate)
				await ImagesGrid.FadeInAsync();
			else*/
			ImagesGrid.SetVisualOpacity(1);
		}
	}

	/*private async void Next_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
	{
		args.Handled = true;
		await NextArchiveAsync();
	}*/

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

	/*private void Close_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
	{
		if (!Data.ShowReader)
			return;
		args.Handled = true;
		CloseReader();
	}*/

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

		var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
		var alt = e.KeyModifiers.HasFlag(KeyModifiers.Alt);

		if (ctrl || alt)
			return;

		if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Space ||
			 e.Key == Key.Escape || e.Key == Key.D || e.Key == Key.A || e.Key == Key.W || e.Key == Key.S)
		{
			e.Handled = true;
			FocusReader();
		}
		double offset = ScrollViewer.Offset.Y;
		switch (e.Key)
		{
			case Key.Up:
			case Key.W:
				if (Math.Floor(offset) <= 0 && Service.Settings.ScrollToChangePage)
					PrevPage(true);
				else
					ScrollViewer.Offset = new Vector(ScrollViewer.Offset.X, offset - Service.Settings.KeyboardScroll);
				break;
			case Key.Down:
			case Key.Space:
			case Key.S:
				if ((ScrollViewer.Extent.Height - ScrollViewer.Viewport.Height - offset) < 5 && Service.Settings.ScrollToChangePage)
					NextPage(true);
				else
					ScrollViewer.Offset = new Vector(ScrollViewer.Offset.X, offset + Service.Settings.KeyboardScroll);
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
		}
	}

	private void FocusReader()
	{
		if (Data.ShowReader && _focus)
		{
			ReaderControl.Focus();
		}
	}

	private void ReaderControl_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
	{
		if (_changingPage)
			return;
		var pointerPoint = e.GetCurrentPoint(ScrollViewer);
		if (e.Pointer.Type == PointerType.Mouse)
		{
			var delta = e.Delta.Y * 120; // UWP's delta is 120, Avalonia is 1
			if (e.KeyModifiers == KeyModifiers.Control || pointerPoint.Properties.IsRightButtonPressed)
			{
				e.Handled = true;
				Data.ZoomValue = Math.Clamp(Data.ZoomValue + (int)(delta * 0.1), Data.UseVerticalReader ? 50 : 100, 400);
			}
			else if (e.KeyModifiers == KeyModifiers.None)
			{
				if (Math.Ceiling(ScrollViewer.Offset.Y) >= ScrollViewer.Extent.Height - ScrollViewer.Viewport.Height && delta < 0 && Service.Settings.ScrollToChangePage)
				{
					e.Handled = true;
					NextPage(true);
				}
				else if (Math.Floor(ScrollViewer.Offset.Y) <= 0 && delta > 0 && Service.Settings.ScrollToChangePage)
				{
					e.Handled = true;
					PrevPage(true);
				}
			}
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
		double distance = ScrollViewer.Bounds.Width / 6.0;
		if (point.X > distance && point.X < ScrollViewer.Bounds.Width - distance)
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
		if (e.Pointer.Type == PointerType.Mouse)
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

	private void ScrollViewer_DoubleTapped(object? sender, TappedEventArgs e)
	{
		var point = e.GetPosition(ScrollViewer);
		double distance = ScrollViewer.Bounds.Width / 6.0;
		if (point.X > distance && point.X < ScrollViewer.Bounds.Width - distance)
		{
			/*var AppView = ApplicationView.GetForCurrentView();
			if (AppView.IsFullScreenMode)
			{
				AppView.ExitFullScreenMode();
			}
			else
			{
				AppView.TryEnterFullScreenMode();
			}*/
			e.Handled = true;
		}
	}

	private async void ScrollViewer_Holding(object? sender, HoldingRoutedEventArgs e)
	{
		/*if (!Service.Settings.ShowMap)
			return;
		var point = e.GetPosition(ScrollViewer);
		double distance = ScrollViewer.ActualWidth / 6.0;
		if (point.X > distance && point.X < ScrollViewer.ActualWidth - distance)
		{
			await OpenOverlay();
			e.Handled = true;
		}*/
	}

	private bool HandleTapped(Point point)
	{
		double distance = ScrollViewer.Bounds.Width / 6.0;
		if (point.X < distance)
		{
			PrevPage();
			return true;
		}
		else if (point.X > ScrollViewer.Bounds.Width - distance)
		{
			NextPage();
			return true;
		}
		return false;
	}

	/*private void ReaderControl_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
	{
		e.Handled = true;
		double vertical = ScrollViewer.VerticalOffset;
		double horizontal = ScrollViewer.HorizontalOffset;
		ScrollViewer.ChangeView(horizontal - e.Delta.Translation.X, vertical - e.Delta.Translation.Y, null, true);
	}*/

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
		if (Service.Settings.OpenNextArchive && Data.ReaderContent.Page + 1 >= Data.Pages)
		{
			await NextArchiveAsync();
			return;
		}
		if (Data.ReaderIndex < Data.ArchiveImagesReader.Count() - 1)
		{
			++Data.ReaderIndex;
			await ReaderImage.FadeOutPage();
			ScrollViewer.Offset = new Vector(ScrollViewer.Offset.X, 0);
			await ChangePage();
		}
	}

	private async Task GoLeft()
	{
		if (Data.UseVerticalReader || _transition)
			return;
		if (Data.ReaderIndex > 0)
		{
			--Data.ReaderIndex;
			await ReaderImage.FadeOutPage();
			ScrollViewer.Offset = new Vector(ScrollViewer.Offset.X, 0);
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
		FitImages(Data.UseVerticalReader);
		ReaderThumbnailOverlay.Width = e.NewSize.Width;
		//LeftHitTargetOverlay.Width = RightHitTargetOverlay.Width = ScrollViewer.ActualWidth / 6.0;
		//ReaderThumbnailOverlayHitArea.Margin = new Thickness(ScrollViewer.ActualWidth / 6.0, 0, ScrollViewer.ActualWidth / 6.0, 0);
	}

	private void ReaderControl_SizeChanged(object? sender, SizeChangedEventArgs e) => FitImages(true);

	private void FitImages() => FitImages(false);

	private void FitImages(bool disableAnim = false)
	{
		if (ReaderControl.Bounds.Width == 0 || ReaderControl.Bounds.Height == 0)
			return;
		float zoomFactor;
		if (Data.UseVerticalReader)
		{
			if (_fitAgainstFixedWidth == 0)
				_fitAgainstFixedWidth = ReaderControl.Bounds.Width;
			zoomFactor = (float)(ScrollViewer.Viewport.Width / _fitAgainstFixedWidth) * 0.5f;
		}
		else if (Data.FitToWidth)
		{
			zoomFactor = (float)Math.Min(ScrollViewer.Viewport.Width / ReaderControl.Bounds.Width, Data.FitScaleLimit * 0.01);
		}
		else
		{
			zoomFactor = (float)Math.Min(ScrollViewer.Viewport.Width / ReaderControl.Bounds.Width, ScrollViewer.Viewport.Height / ReaderControl.Bounds.Height);
		}
		var zoom = zoomFactor * (Data.ZoomValue * 0.01f);
		if (zoom != _lastZoom)
		{
			_lastZoom = zoom;
			var yOffset = ScrollViewer.Offset.Y / Data.ZoomFactor * zoom;
			Data.ZoomFactor = zoom;
			ScrollViewer.Offset = new Point(ScrollViewer.Offset.X, yOffset);
		}
	}

	private async void ScrollViewer_ViewChanged(object? sender, ScrollChangedEventArgs e)
	{
		//if (e.IsIntermediate)
		//return;
		// Use width instead of height in vertical mode
		if (Data.UseVerticalReader)
		{
			/*if (ScrollViewer.CurrentAnchor is ReaderImage image)
			{
				var index = ReaderVertical.GetElementIndex(ScrollViewer.CurrentAnchor);
				Data.ReaderIndex = index;

				var width = (int)Math.Round(ScrollViewer.ExtentWidth);
				await image.ResizeWidth(width);
				(ReaderVertical.TryGetElement(index + 1) as ReaderImage)?.ResizeWidth(width);
				(ReaderVertical.TryGetElement(index + 2) as ReaderImage)?.ResizeWidth(width);
			}*/
		}
		else
			_resizer.Invoke((int)Math.Round(ScrollViewer.Extent.Height));
	}

	private void CompositionTarget_Rendering(object? sender, object e)
	{
		/*var timings = (RenderingEventArgs)e;
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
		_previousTime = timings.RenderingTime;*/
	}

	[RelayCommand]
	private async Task PlayStop(bool state)
	{
		// Handle user initiated mouse action (disable autoplay)
		/*Data.UseAutoplay = state;
		if (state)
		{
			ScrollViewer.ChangeView(null, 0, null, true);
			await Task.Delay(TimeSpan.FromMilliseconds(Service.Settings.AutoplayStartDelay));
			CompositionTarget.Rendering += CompositionTarget_Rendering;
		}
		else
			CompositionTarget.Rendering -= CompositionTarget_Rendering;*/
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

		var storage = TopLevel.GetTopLevel(this)!.StorageProvider;

		var savePicker = new FilePickerSaveOptions
		{
			SuggestedStartLocation = await storage.TryGetWellKnownFolderAsync(WellKnownFolder.Downloads),
			DefaultExtension = download.Type,
			SuggestedFileName = download.Name
		};

		var file = await storage.SaveFilePickerAsync(savePicker);
		Data.Downloading = false;
		if (file != null)
		{
			using var stream = await file.OpenWriteAsync();
			await stream.WriteAsync(download.Data);
		}
	}

	private async void RefreshContainer_RefreshRequested(object? sender, RefreshRequestedEventArgs args)
	{
		var deferral = args.GetDeferral();
		await Data.Reload(false);
		deferral.Complete();
	}

	/*private async void Refresh_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
	{
		await Data.Reload();
		args.Handled = true;
	}*/

	/*private void ImagesGrid_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
	{
		if (!args.InRecycleQueue && args.ItemContainer.ContentTemplateRoot is ArchiveImage item)
		{
			item.Phase0();
			args.RegisterUpdateCallback(Phase1);
		}
		args.Handled = true;
	}

	private void Phase1(ListViewBase sender, ContainerContentChangingEventArgs args)
	{
		if (!args.InRecycleQueue && args.ItemContainer.ContentTemplateRoot is ArchiveImage item)
		{
			item.Phase1((ImagePageSet)args.Item);
			args.RegisterUpdateCallback(Phase2);
		}
	}

	private void Phase2(ListViewBase sender, ContainerContentChangingEventArgs args)
	{
		if (!args.InRecycleQueue && args.ItemContainer.ContentTemplateRoot is ArchiveImage item)
		{
			item.Phase2();
			args.RegisterUpdateCallback(Phase3);
		}
	}

	private void Phase3(ListViewBase sender, ContainerContentChangingEventArgs args)
	{
		if (!args.InRecycleQueue && args.ItemContainer.ContentTemplateRoot is ArchiveImage item)
			item.Phase3();
	}*/

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
		//OverlayThumbnails.SelectedIndex = Data.ReaderContent.Page;
		//await OverlayThumbnails.SmoothScrollIntoViewWithIndexAsync(Data.ReaderContent.Page, ScrollItemPlacement.Center);
	}

	private async void Trigger_PointerEntered(object? sender, PointerEventArgs e)
	{
		if (!Data.ShowReader)
			return;
		if (!Service.Settings.ShowMap)
			return;
		if (e.Pointer.Type == PointerType.Touch)
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

	/*private async void OverlayThumbnails_ItemClick(object sender, ItemClickEventArgs e)
	{
		var readerSet = Data.ArchiveImagesReader.FirstOrDefault(s => s.Page >= Data.ArchiveImages.IndexOf((ImagePageSet)e.ClickedItem));
		if (readerSet == null)
			return;

		int index = Data.ArchiveImagesReader.IndexOf(readerSet);

		if (Data.UseVerticalReader)
		{
			await Task.Delay(100);
			var element = ReaderVertical.GetOrCreateElement(index);
			element.UpdateLayout();
			element.StartBringIntoView(new BringIntoViewOptions { AnimationDesired = true, VerticalAlignmentRatio = 0f });
		}
		else
		{
			_changingPage = true;

			Data.ReaderIndex = index;
			await ReaderImage.FadeOutPage();
			ScrollViewer.ChangeView(null, 0, null, true);
			await ChangePage();

			_changingPage = false;
		}
	}*/

	public ArchiveTabState GetTabState() => archiveState ?? new ArchiveTabState(Data.Archive.arcid, Data.ReaderContent?.Page, Data.ShowReader, Data.Group.Select(a => a.arcid).ToList());
}