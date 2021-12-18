using LRReader.Shared.Extensions;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using LRReader.UWP.Extensions;
using LRReader.UWP.Views.Items;
using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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
		private float _lastZoom;
		private double _fitAgainstFixedWidth;

		private bool _transition;

		private SemaphoreSlim _loadSemaphore = new SemaphoreSlim(1);

		public ArchiveTabContent()
		{
			this.InitializeComponent();
			ReaderBackground.SetVisualOpacity(0);
			ScrollViewer.SetVisualOpacity(0);

			Data = DataContext as ArchivePageViewModel;
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
				if (Service.Settings.OpenReader)
				{
					var page = 0;
					if (Data.Bookmarked)
						page = Data.BookmarkProgress;
					OpenReader(page);
				}
				_opened = true;
			}
		}

		public async void LoadArchive(Archive archive, IList<Archive> next)
		{
			Data.Archive = archive;
			if (next != null)
				Data.Group = next;
			await Data.Reload();
			_loadSemaphore.Release();
		}

		private async void OpenReader(int page, object item = null)
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
				var image = ImagesGrid.ContainerFromItem(item).FindDescendant("Image");
				if (!(image.ActualWidth == 0 || image.ActualHeight == 0))
				{
					var anim = ImagesGrid.PrepareConnectedAnimation(GetOpenTarget(readerSet, page), item, "Image");
					anim.Configuration = new BasicConnectedAnimationConfiguration();
				}
			}

			Data.ShowReader = true;
			Data.ReaderIndex = index;
			if (Data.UseVerticalReader)
			{
				var element = ReaderVertical.GetOrCreateElement(index);
				element.UpdateLayout();
				element.StartBringIntoView(new BringIntoViewOptions { AnimationDesired = false });
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
		}

		public async void CloseReader()
		{
			if (_transition)
				return;
			_transition = true;
			var animate = Service.Platform.AnimationsEnabled;
			ConnectedAnimation animLeft = null, animRight = null;

			if (!Data.UseVerticalReader)
			{
				var left = ReaderImage.FindDescendant("LeftImage");
				var right = ReaderImage.FindDescendant("RightImage");
				ReaderImage.disableAnimation = true;

				if (animate)
				{
					if (Data.ReaderContent.LeftImage != null && !(left.ActualWidth == 0 || left.ActualHeight == 0))
					{
						animLeft = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("closeL", left);
						animLeft.Configuration = new BasicConnectedAnimationConfiguration();
					}
					if (Data.ReaderContent.RightImage != null && !(right.ActualWidth == 0 || right.ActualHeight == 0))
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
			if (animate)
			{
				if (Data.ReaderContent.LeftImage != null && animLeft != null && Data.ArchiveImages.Count > leftTarget)
					await ImagesGrid.TryStartConnectedAnimationAsync(animLeft, Data.ArchiveImages.ElementAt(leftTarget), "Image");
				if (Data.ReaderContent.RightImage != null & animRight != null && Data.ArchiveImages.Count > rightTarget)
					await ImagesGrid.TryStartConnectedAnimationAsync(animRight, Data.ArchiveImages.ElementAt(rightTarget), "Image");
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
		}

		private async Task NextArchive()
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

		[Microsoft.Toolkit.Mvvm.Input.ICommand]
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
			await NextArchive();
		}

		private void ImagesGrid_ItemClick(object sender, ItemClickEventArgs e)
		{
			if (!Data.ControlsEnabled)
				return;
			OpenReader(Data.ArchiveImages.IndexOf(e.ClickedItem as ImagePageSet), e.ClickedItem);
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

			double offset = ScrollViewer.VerticalOffset;
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
			if (!Data.ShowReader)
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

		private void ScrollViewer_RightTapped(object sender, RightTappedRoutedEventArgs e) => HandleTapped(e.GetPosition(ScrollViewer));

		private void ScrollViewer_Tapped(object sender, TappedRoutedEventArgs e) => HandleTapped(e.GetPosition(ScrollViewer));

		private void HandleTapped(Point point)
		{
			if (_changePage)
			{
				double distance = ScrollViewer.ActualWidth / 6.0;
				if (point.X < distance)
				{
					PrevPage();
				}
				else if (point.X > ScrollViewer.ActualWidth - distance)
				{
					NextPage();
				}
				_changePage = false;
			}
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
			if (Data.ReadRTL && !ignore)
				await GoLeft();
			else
				await GoRight();
		}

		private async void PrevPage(bool ignore = false)
		{
			if (Data.ReadRTL && !ignore)
				await GoRight();
			else
				await GoLeft();
		}

		private async Task GoRight()
		{
			if (Data.UseVerticalReader)
				return;
			if (Service.Settings.OpenNextArchive && Data.ReaderContent.Page + 1 >= Data.Pages)
			{
				await NextArchive();
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
		}

		private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e) => FitImages(Data.UseVerticalReader);

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
			double? yOffset = null;
			if (zoom != _lastZoom)
			{
				_lastZoom = zoom;
				yOffset = ScrollViewer.VerticalOffset / ScrollViewer.ZoomFactor * zoom;
			}
			ScrollViewer.ChangeView(null, yOffset, zoom, disableAnim || !Service.Platform.AnimationsEnabled);
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

	}
}
