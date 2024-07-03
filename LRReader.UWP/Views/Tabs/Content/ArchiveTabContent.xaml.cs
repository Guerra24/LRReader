#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Animations;
using LRReader.Shared.Extensions;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using LRReader.UWP.Extensions;
using LRReader.UWP.Views.Items;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using RefreshContainer = Microsoft.UI.Xaml.Controls.RefreshContainer;
using RefreshRequestedEventArgs = Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs;

namespace LRReader.UWP.Views.Tabs.Content
{
	public sealed partial class ArchiveTabContent : UserControl
	{
		private static AnimationBuilder FadeIn = AnimationBuilder.Create().Opacity(to: 1, duration: TimeSpan.FromMilliseconds(200), easingMode: EasingMode.EaseIn);
		private static AnimationBuilder FadeOut = AnimationBuilder.Create().Opacity(to: 0, duration: TimeSpan.FromMilliseconds(200), easingMode: EasingMode.EaseIn);

		public ArchivePageViewModel Data;

		private bool _wasNew;
		private bool _opened;
		private bool _focus = true;
		private bool _changePage;
		private bool _changingPage;
		private float _lastZoom;
		private double _fitAgainstFixedWidth;
		private bool _handleDoubleTap;
		private bool _overlayDelayOpen;

		private bool _transition;

		private bool? _forceOpen;
		private int? _forceProgress;

		private TimeSpan _previousTime = TimeSpan.Zero;

		private SemaphoreSlim _loadSemaphore = new SemaphoreSlim(1);

		public ArchiveTabContent()
		{
			this.InitializeComponent();
			ReaderBackground.SetVisualOpacity(0);
			ScrollViewer.SetVisualOpacity(0);
			/*
			ElementCompositionPreview.SetIsTranslationEnabled(ReaderThumbnailOverlay, true);
			ElementCompositionPreview.GetElementVisual(ReaderThumbnailOverlay).Properties.InsertVector3("Translation", new Vector3(0, 317, 0));
			*/

			Data = (ArchivePageViewModel)DataContext;
			Data.ZoomChangedEvent += FitImages;
			Data.RebuildReader += RebuildReader;

			_loadSemaphore.Wait();

			Service.Events.RebuildReaderImagesSetEvent += RebuildReader;
		}

		private async void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			Data.ReloadBookmarkedObject();
			FocusReader();
			if (!_opened)
			{
				await _loadSemaphore.WaitAsync();
				await Data.HandleConflict();
				_loadSemaphore.Release();
				if (_forceOpen ?? Service.Settings.OpenReader)
				{
					var page = 0;
					if (Data.Bookmarked)
						page = Data.BookmarkProgress;
					OpenReader(_forceProgress ?? page);
				}
				_opened = true;
			}
		}

		public async void LoadArchive(Archive archive, IList<Archive>? next = null, int? forceProgress = null, bool? forceOpen = null)
		{
			Data.Archive = archive;
			if (next != null)
				Data.Group = next;
			await Data.Reload();
			_forceProgress = forceProgress;
			_forceOpen = forceOpen;
			_loadSemaphore.Release();
		}

		private async void OpenReader(int page, object? item = null)
		{
			if (_transition)
				return;
			_transition = true;
			var readerSet = Data.ArchiveImagesReader.FirstOrDefault(s => s.Page >= page);
			if (readerSet == null)
				return;
			var index = Data.ArchiveImagesReader.IndexOf(readerSet);

			if (Service.Platform.AnimationsEnabled && item != null && !Data.UseVerticalReader)
			{
				var image = ImagesGrid.ContainerFromItem(item).FindDescendant("Thumbnail");
				if (image != null && !(image.ActualWidth == 0 || image.ActualHeight == 0))
				{
					var anim = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(GetOpenTarget(readerSet, page), image);
					anim.Configuration = new BasicConnectedAnimationConfiguration();
				}
			}

			Data.ShowReader = true;
			Data.ReaderIndex = index;
			if (Data.UseVerticalReader)
			{
				await Task.Delay(100);
				var element = ReaderVertical.GetOrCreateElement(index);
				element.UpdateLayout();
				element.StartBringIntoView(new BringIntoViewOptions { AnimationDesired = false, VerticalAlignmentRatio = 0f });
			}
			else
				await ChangePage();

			if (Data.Archive.isnew)
				_wasNew = true;
			if (Service.Platform.AnimationsEnabled)
			{
				FadeIn.Start(ReaderBackground);
				await FadeIn.StartAsync(ScrollViewer);
			}
			else
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
			await PlayStop(false);
			var animate = Service.Platform.AnimationsEnabled;
			ConnectedAnimation? animLeft = null, animRight = null;

			if (!Data.UseVerticalReader)
			{
				var left = ReaderImage.FindDescendant("LeftImage");
				var right = ReaderImage.FindDescendant("RightImage");
				ReaderImage.disableAnimation = true;

				if (animate)
				{
					if (Data.ReaderContent.LeftImage != null && left != null && !(left.ActualWidth == 0 || left.ActualHeight == 0))
					{
						animLeft = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("closeL", left);
						animLeft.Configuration = new BasicConnectedAnimationConfiguration();
					}
					if (Data.ReaderContent.RightImage != null && right != null && !(right.ActualWidth == 0 || right.ActualHeight == 0))
					{
						animRight = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("closeR", right);
						animRight.Configuration = new BasicConnectedAnimationConfiguration();
					}
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
			await ImagesGrid.SmoothScrollIntoViewWithIndexAsync(leftTarget, disableAnimation: false);
			if (animate)
			{
				var leftThumb = ImagesGrid.ContainerFromIndex(leftTarget).FindDescendant("Thumbnail");
				var rightThumb = ImagesGrid.ContainerFromIndex(rightTarget).FindDescendant("Thumbnail");
				if (Data.ReaderContent.LeftImage != null && leftThumb != null && Data.ArchiveImages.Count > leftTarget)
					animLeft?.TryStart(leftThumb);
				if (Data.ReaderContent.RightImage != null && rightThumb != null && Data.ArchiveImages.Count > rightTarget)
					animRight?.TryStart(rightThumb);
				FadeOut.Start(ReaderBackground);
				await FadeOut.StartAsync(ScrollViewer);
				await Task.Delay(200); // Give it a sec
			}
			else
			{
				ReaderBackground.SetVisualOpacity(0);
				ScrollViewer.SetVisualOpacity(0);
			}
			Data.ShowReader = false;

			_wasNew = await Data.SaveReaderData(_wasNew);

			_transition = false;
			_changePage = false;
		}

		private async void NextArchive() => await NextArchiveAsync();

		private async Task NextArchiveAsync()
		{
			if (!Data.CanGoNext)
				return;
			_transition = true;
			await HideReader();
			await Data.NextArchive();
			await ShowReader();
			_transition = false;
		}

		private async void Random_Clicked() => await Random((CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Shift) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down);

		[RelayCommand]
		private async Task Random(bool newOnly)
		{
			var list = Service.Archives.Archives.Where(kv => kv.Value.isnew || !newOnly);
			if (list.Count() <= 1)
				return;
			var random = new Random();
			var item = list.ElementAt(random.Next(list.Count() - 1));

			_transition = true;
			await HideReader();
			await Data.OpenArchive(item.Value);
			await ShowReader();
			_transition = false;
		}

		private async void RebuildReader()
		{
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
				if (Service.Platform.AnimationsEnabled)
					await FadeOut.StartAsync(ScrollViewer);
				else
					ScrollViewer.SetVisualOpacity(0);
			}
			else
			{
				if (Service.Platform.AnimationsEnabled)
					await ImagesGrid.FadeOutAsync();
				else
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
					await Task.Delay(100);
					var element = ReaderVertical.GetOrCreateElement(index);
					element.UpdateLayout();
					element.StartBringIntoView(new BringIntoViewOptions { AnimationDesired = false, VerticalAlignmentRatio = 0f });
				}
				else
					await ChangePage();

				if (Service.Platform.AnimationsEnabled)
					await FadeIn.StartAsync(ScrollViewer);
				else
					ScrollViewer.SetVisualOpacity(1);
				FocusReader();
			}
			else
			{
				if (Service.Platform.AnimationsEnabled)
					await ImagesGrid.FadeInAsync();
				else
					ImagesGrid.SetVisualOpacity(1);
			}
		}

		private async void Next_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			args.Handled = true;
			await NextArchiveAsync();
		}

		private void ImagesGrid_ItemClick(object sender, ItemClickEventArgs e)
		{
			if (!Data.ControlsEnabled)
				return;
			OpenReader(Data.ArchiveImages.IndexOf((ImagePageSet)e.ClickedItem), e.ClickedItem);
		}

		private void Continue_Click(object sender, RoutedEventArgs e)
		{
			if (!Data.ControlsEnabled)
				return;
			OpenReader(Data.BookmarkProgress);
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			if (!Data.ShowReader)
				return;
			CloseReader();
		}

		private void Close_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			if (!Data.ShowReader)
				return;
			args.Handled = true;
			CloseReader();
		}

		private void ReaderControl_KeyUp(object sender, KeyRoutedEventArgs e)
		{
			if (!Data.ShowReader)
				return;
			if (e.Key == VirtualKey.Left || e.Key == VirtualKey.Right || e.Key == VirtualKey.Up || e.Key == VirtualKey.Down || e.Key == VirtualKey.Space ||
				 e.Key == VirtualKey.Escape || e.Key == VirtualKey.D || e.Key == VirtualKey.A || e.Key == VirtualKey.W || e.Key == VirtualKey.S)
				e.Handled = true;

			switch (e.Key)
			{
				case VirtualKey.Right:
				case VirtualKey.D:
					NextPage();
					break;
				case VirtualKey.Left:
				case VirtualKey.A:
					PrevPage();
					break;
				case VirtualKey.Escape:
					CloseReader();
					break;
			}
		}

		private void ReaderControl_KeyDown(object sender, KeyRoutedEventArgs e)
		{
			if (!Data.ShowReader || _changingPage)
				return;

			var ctrl = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control);
			var alt = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Menu);

			if ((ctrl & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down || (alt & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
				return;

			if (e.Key == VirtualKey.Left || e.Key == VirtualKey.Right || e.Key == VirtualKey.Up || e.Key == VirtualKey.Down || e.Key == VirtualKey.Space ||
				 e.Key == VirtualKey.Escape || e.Key == VirtualKey.D || e.Key == VirtualKey.A || e.Key == VirtualKey.W || e.Key == VirtualKey.S)
			{
				e.Handled = true;
				FocusReader();
			}
			double offset = ScrollViewer.VerticalOffset;
			switch (e.Key)
			{
				case VirtualKey.Up:
				case VirtualKey.W:
					if (Math.Floor(offset) <= 0 && Service.Settings.ScrollToChangePage)
						PrevPage(true);
					else
						ScrollViewer.ChangeView(null, offset - Service.Settings.KeyboardScroll, null, false);
					break;
				case VirtualKey.Down:
				case VirtualKey.Space:
				case VirtualKey.S:
					if (Math.Ceiling(offset) >= ScrollViewer.ScrollableHeight && Service.Settings.ScrollToChangePage)
						NextPage(true);
					else
						ScrollViewer.ChangeView(null, offset + Service.Settings.KeyboardScroll, null, false);
					break;
			}
		}

		private void FocusReader()
		{
			if (Data.ShowReader && _focus)
			{
				ReaderControl.Focus(FocusState.Programmatic);
			}
		}

		private void ReaderControl_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
		{
			if (_changingPage)
				return;
			var pointerPoint = e.GetCurrentPoint(ScrollViewer);
			if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
			{
				var delta = pointerPoint.Properties.MouseWheelDelta;
				switch (e.KeyModifiers)
				{
					case VirtualKeyModifiers.None:
						if (Math.Ceiling(ScrollViewer.VerticalOffset) >= ScrollViewer.ScrollableHeight && delta < 0 && Service.Settings.ScrollToChangePage)
						{
							e.Handled = true;
							NextPage(true);
						}
						else if (Math.Floor(ScrollViewer.VerticalOffset) <= 0 && delta > 0 && Service.Settings.ScrollToChangePage)
						{
							e.Handled = true;
							PrevPage(true);
						}
						break;
					case VirtualKeyModifiers.Control:
						e.Handled = true;
						Data.ZoomValue = Math.Clamp(Data.ZoomValue + (int)(delta * 0.1), Data.UseVerticalReader ? 50 : 100, 400);
						break;
				}
			}
		}

		private void ScrollViewer_PointerPressed(object sender, PointerRoutedEventArgs e)
		{
			var pointerPoint = e.GetCurrentPoint(ScrollViewer);
			_handleDoubleTap = pointerPoint.Properties.IsLeftButtonPressed;
			if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
			{
				if (pointerPoint.Properties.IsXButton1Pressed)
				{
					e.Handled = true;
					FocusReader();
					PrevPage();
					return;
				}
				else if (pointerPoint.Properties.IsXButton2Pressed)
				{
					e.Handled = true;
					FocusReader();
					NextPage();
					return;
				}
			}
			_changePage = pointerPoint.Properties.IsLeftButtonPressed || pointerPoint.Properties.IsRightButtonPressed;
		}

		private void ScrollViewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
		{
			if (!_handleDoubleTap)
				return;
			var point = e.GetPosition(ScrollViewer);
			double distance = ScrollViewer.ActualWidth / 6.0;
			if (point.X > distance && point.X < ScrollViewer.ActualWidth - distance)
			{
				var AppView = ApplicationView.GetForCurrentView();
				if (AppView.IsFullScreenMode)
				{
					AppView.ExitFullScreenMode();
				}
				else
				{
					AppView.TryEnterFullScreenMode();
				}
				e.Handled = true;
			}
		}

		private void ScrollViewer_RightTapped(object sender, RightTappedRoutedEventArgs e) => e.Handled = HandleTapped(e.GetPosition(ScrollViewer));

		private void ScrollViewer_Tapped(object sender, TappedRoutedEventArgs e) => e.Handled = HandleTapped(e.GetPosition(ScrollViewer));

		private bool HandleTapped(Point point)
		{
			if (_changePage)
			{
				double distance = ScrollViewer.ActualWidth / 6.0;
				if (point.X < distance)
				{
					PrevPage();
					return true;
				}
				else if (point.X > ScrollViewer.ActualWidth - distance)
				{
					NextPage();
					return true;
				}
				_changePage = false;
			}
			return false;
		}

		private void ReaderControl_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
		{
			e.Handled = true;
			double vertical = ScrollViewer.VerticalOffset;
			double horizontal = ScrollViewer.HorizontalOffset;
			ScrollViewer.ChangeView(horizontal - e.Delta.Translation.X, vertical - e.Delta.Translation.Y, null, true);
		}

		private async void NextPage(bool ignore = false)
		{
			_changingPage = true;
			if (Data.UseAutoplay)
				await Task.Delay(TimeSpan.FromMilliseconds(Service.Settings.AutoplayBeforeChangeDelay));
			if (Data.ReadRTL && !ignore)
				await GoLeft();
			else
				await GoRight();
			if (Data.UseAutoplay)
				await Task.Delay(TimeSpan.FromMilliseconds(Service.Settings.AutoplayAfterChangeDelay));
			_changingPage = false;
		}

		private async void PrevPage(bool ignore = false)
		{
			_changingPage = true;
			if (Data.UseAutoplay)
				await Task.Delay(TimeSpan.FromMilliseconds(Service.Settings.AutoplayBeforeChangeDelay));
			if (Data.ReadRTL && !ignore)
				await GoRight();
			else
				await GoLeft();
			if (Data.UseAutoplay)
				await Task.Delay(TimeSpan.FromMilliseconds(Service.Settings.AutoplayAfterChangeDelay));
			_changingPage = false;
		}

		private async Task GoRight()
		{
			if (Data.UseVerticalReader)
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
				ScrollViewer.ChangeView(null, 0, null, true);
				await ChangePage();
			}
		}

		private async Task GoLeft()
		{
			if (Data.UseVerticalReader)
				return;
			if (Data.ReaderIndex > 0)
			{
				--Data.ReaderIndex;
				await ReaderImage.FadeOutPage();
				ScrollViewer.ChangeView(null, 0, null, true);
				await ChangePage();
			}
		}

		private async Task ChangePage()
		{
			if (Data.UseVerticalReader)
				return;
			await ReaderImage.ChangePage(Data.ReaderContent);
			ReaderImage.FadeInPage();

			Preload(Data.ArchiveImagesReader.ElementAtOrDefault(Data.ReaderIndex + 1));
			Preload(Data.ArchiveImagesReader.ElementAtOrDefault(Data.ReaderIndex + 2));
			Preload(Data.ArchiveImagesReader.ElementAtOrDefault(Data.ReaderIndex + 3));
		}

		private async void Preload(ReaderImageSet set)
		{
			if (set == null)
				return;
			await Service.Images.GetImageCached(set.LeftImage);
			await Service.Images.GetImageCached(set.RightImage);
		}

		private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			FitImages(Data.UseVerticalReader);
			ReaderThumbnailOverlay.Width = e.NewSize.Width;
			//LeftHitTargetOverlay.Width = RightHitTargetOverlay.Width = ScrollViewer.ActualWidth / 6.0;
			ReaderThumbnailOverlayHitArea.Margin = new Thickness(ScrollViewer.ActualWidth / 6.0, 0, ScrollViewer.ActualWidth / 6.0, 0);
		}

		private void ReaderControl_SizeChanged(object sender, SizeChangedEventArgs e) => FitImages(true);

		private void FitImages() => FitImages(false);

		private void FitImages(bool disableAnim = false)
		{
			if (ReaderControl.ActualWidth == 0 || ReaderControl.ActualHeight == 0)
				return;
			float zoomFactor;
			if (Data.UseVerticalReader)
			{
				if (_fitAgainstFixedWidth == 0)
					_fitAgainstFixedWidth = ReaderControl.ActualWidth;
				zoomFactor = (float)(ScrollViewer.ViewportWidth / _fitAgainstFixedWidth) * 0.5f;
			}
			else if (Data.FitToWidth)
			{
				zoomFactor = (float)Math.Min(ScrollViewer.ViewportWidth / ReaderControl.ActualWidth, Data.FitScaleLimit * 0.01);
			}
			else
			{
				zoomFactor = (float)Math.Min(ScrollViewer.ViewportWidth / ReaderControl.ActualWidth, ScrollViewer.ViewportHeight / ReaderControl.ActualHeight);
			}
			var zoom = zoomFactor * (Data.ZoomValue * 0.01f);
			if (zoom != _lastZoom)
			{
				_lastZoom = zoom;
				var yOffset = ScrollViewer.VerticalOffset / ScrollViewer.ZoomFactor * zoom;
				ScrollViewer.ChangeView(null, yOffset, zoom, disableAnim || !Service.Platform.AnimationsEnabled);
			}
		}

		private async void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		{
			if (e.IsIntermediate)
				return;
			// Use width instead of height in vertical mode
			if (Data.UseVerticalReader)
			{
				if (ScrollViewer.CurrentAnchor is ReaderImage image)
				{
					var index = ReaderVertical.GetElementIndex(ScrollViewer.CurrentAnchor);
					Data.ReaderIndex = index;

					var width = (int)Math.Round(ScrollViewer.ExtentWidth);
					await image.ResizeWidth(width);
					(ReaderVertical.TryGetElement(index + 1) as ReaderImage)?.ResizeWidth(width);
					(ReaderVertical.TryGetElement(index + 2) as ReaderImage)?.ResizeWidth(width);
				}
			}
			else
				await ReaderImage.ResizeHeight((int)Math.Round(ScrollViewer.ExtentHeight));
		}

		private void CompositionTarget_Rendering(object sender, object e)
		{
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
		}

		[RelayCommand]
		private async Task PlayStop(bool state)
		{
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
		}

		private async void DownloadButton_Click(object sender, RoutedEventArgs e)
		{
			Data.Downloading = true;
			var download = await Data.DownloadArchive();
			if (download == null)
			{
				Data.Downloading = false;
				return;
			}

			var savePicker = new FileSavePicker();
			savePicker.SuggestedStartLocation = PickerLocationId.Downloads;
			savePicker.FileTypeChoices.Add(download.Type + " File", new List<string>() { download.Type });
			savePicker.SuggestedFileName = download.Name;

			StorageFile file = await savePicker.PickSaveFileAsync();
			Data.Downloading = false;
			if (file != null)
			{
				CachedFileManager.DeferUpdates(file);
				await FileIO.WriteBytesAsync(file, download.Data);
				FileUpdateStatus status =
					await CachedFileManager.CompleteUpdatesAsync(file);
				if (status == FileUpdateStatus.Complete)
				{
					//save
				}
				else
				{
					// not saved
				}
			}
			else
			{
				//cancel
			}
		}

		private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
		{
			using (var deferral = args.GetDeferral())
			{
				await Data.Reload(false);
			}
		}

		private async void Refresh_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			await Data.Reload();
			args.Handled = true;
		}

		private void ImagesGrid_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
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

		private async void Trigger_PointerEntered(object sender, PointerRoutedEventArgs e)
		{
			if (!Data.ShowReader)
				return;
			if (!Service.Settings.ShowMap)
				return;
			_overlayDelayOpen = true;
			await Task.Delay(TimeSpan.FromMilliseconds(Service.Platform.HoverTime));
			if (_overlayDelayOpen)
			{
				ReaderThumbnailOverlay.IsOpen = true;
				await Task.Delay(50);
				OverlayThumbnails.SelectedIndex = Data.ReaderContent.Page;
				await OverlayThumbnails.SmoothScrollIntoViewWithIndexAsync(Data.ReaderContent.Page, ScrollItemPlacement.Center);
			}
		}

		private void Trigger_PointerExited(object sender, PointerRoutedEventArgs e)
		{
			if (!_overlayDelayOpen)
				return;
			_overlayDelayOpen = false;
		}

		private async void OverlayThumbnails_ItemClick(object sender, ItemClickEventArgs e)
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
		}
	}
}
