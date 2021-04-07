using LRReader.Internal;
using LRReader.Shared.Internal;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.UWP.Extensions;
using LRReader.UWP.Services;
using LRReader.UWP.ViewModels;
using LRReader.UWP.Views.Dialogs;
using LRReader.UWP.Views.Items;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.System;
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
		private static AnimationBuilder FadeOut = AnimationBuilder.Create().Opacity(to: 0, duration: TimeSpan.FromMilliseconds(200), easingMode: EasingMode.EaseOut);

		public ArchivePageViewModel Data;

		private int i;
		private bool _wasNew;
		private bool _opened;
		private bool _focus = true;
		private bool _changePage;

		private ResourceLoader lang = ResourceLoader.GetForCurrentView("Tabs");

		private Subject<double> resizePixel = new Subject<double>();

		private SemaphoreSlim _loadSemaphore = new SemaphoreSlim(1);

		private ControlFlags flags;

		public ArchiveTabContent()
		{
			this.InitializeComponent();
			ScrollViewer.SetVisualOpacity(0);

			Data = new ArchivePageViewModel();
			Data.ZoomChangedEvent += FitImages;
			Global.EventManager.RebuildReaderImagesSetEvent += Data.CreateImageSets;

			flags = Global.ControlFlags;

			resizePixel.Throttle(TimeSpan.FromMilliseconds(250))
				.Subscribe(async (height) =>
				await DispatcherService.RunAsync(() =>
				(ReaderControl.ContentTemplateRoot as ReaderImage).UpdateDecodedResolution((int)Math.Round(height))));
			_loadSemaphore.Wait();
		}

		private async void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			Data.ReloadBookmarkedObject();
			FocusReader();
			if (!_opened)
			{
				await _loadSemaphore.WaitAsync();
				if (Global.ControlFlags.V077 && Data.Bookmarked && Data.BookmarkProgress + 1 != Data.Archive.progress && Data.Archive.progress > 0)
				{
					var conflictDialog = new ProgressConflict(Data.BookmarkProgress + 1, Data.Archive.progress, Data.Pages);
					await conflictDialog.ShowAsync();
					var result = conflictDialog.Mode;
					switch (result)
					{
						case ConflictMode.Local:
							await Data.SetProgress(Data.BookmarkProgress + 1);
							break;
						case ConflictMode.Remote:
							Data.BookmarkProgress = Data.Archive.progress - 1;
							break;
					}
				}
				_loadSemaphore.Release();
				if (Service.Settings.OpenReader)
				{
					if (Data.Bookmarked)
						i = Data.BookmarkProgress;
					OpenReader();
				}
				_opened = true;
			}
		}

		public async void LoadArchive(Archive archive)
		{
			Data.Archive = archive;
			await Data.Reload(true);
			_loadSemaphore.Release();
		}

		private async void OpenReader()
		{
			Data.ShowReader = true;
			int count = Data.Pages;
			if (Service.Settings.TwoPages)
			{
				if (i != 0)
				{
					--i; i /= 2; ++i;
				}
				if (Service.Settings.ReadRTL)
				{
					int preDiv = count;
					--count; count /= 2; ++count;
					Data.ReaderIndex = count - i - (preDiv % 2);
				}
				else
					Data.ReaderIndex = i;
			}
			else
			{
				if (Service.Settings.ReadRTL)
					Data.ReaderIndex = count - i - 1;
				else
					Data.ReaderIndex = i;
			}

			if (Data.Archive.IsNewArchive())
			{
				_wasNew = true;
			}
			await FadeOut.StartAsync(ImagesGrid);
			await FadeIn.StartAsync(ScrollViewer);
			_focus = true;
			FocusReader();
		}

		public async void CloseReader()
		{
			/*var left = ReaderControl.FindDescendantByName("LeftImage");
			var right = ReaderControl.FindDescendantByName("RightImage");
			ConnectedAnimation animLeft = null, animRight = null;
			if (Data.ReaderContent.LeftImage != null)
			{
				animLeft = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("closeL", left);
				animLeft.Configuration = new BasicConnectedAnimationConfiguration();
			}
			if (Data.ReaderContent.RightImage != null)
			{
				animRight = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("closeR", right);
				animRight.Configuration = new BasicConnectedAnimationConfiguration();
			}*/

			_focus = false;
			await FadeOut.StartAsync(ScrollViewer);
			Data.ShowReader = false;
			int conv = Data.ReaderIndex;
			int count = Data.Pages;
			if (Service.Settings.TwoPages)
			{
				if (conv != 0)
				{
					conv *= 2;
				}
				if (Service.Settings.ReadRTL)
				{
					conv = count - conv - (count % 2);
				}
			}
			else
			{
				if (Service.Settings.ReadRTL)
					conv = count - conv - 1;
			}
			conv = Math.Clamp(conv, 0, count - 1);

			/*int leftTarget = conv;
			if (Service.Settings.TwoPages)
			{
				leftTarget = Math.Max(conv - 1, 0);
				if (conv == Data.Pages - 1 && Data.Pages % 2 == 0)
					leftTarget++;
			}
			int rightTarget = conv;

			if (Service.Settings.ReadRTL)
			{
				int tmp = leftTarget;
				leftTarget = rightTarget;
				rightTarget = tmp;
			}

			if (Data.ReaderContent.LeftImage != null)
				await ImagesGrid.TryStartConnectedAnimationAsync(animLeft, Data.ArchiveImages.ElementAt(leftTarget), "Image");
			if (Data.ReaderContent.RightImage != null)
				await ImagesGrid.TryStartConnectedAnimationAsync(animRight, Data.ArchiveImages.ElementAt(rightTarget), "Image");*/

			await FadeIn.StartAsync(ImagesGrid);

			if (conv >= count - Math.Min(10, Math.Ceiling(count * 0.1)))
			{
				if (Data.Archive.IsNewArchive())
				{
					await Data.ClearNew();
					Data.Archive.isnew = "false";
				}
				if (Data.Bookmarked && Service.Settings.RemoveBookmark)
				{
					var dialog = new ContentDialog
					{
						Title = lang.GetString("Archive/RemoveBookmark/Title"),
						PrimaryButtonText = lang.GetString("Archive/RemoveBookmark/PrimaryButtonText"),
						CloseButtonText = lang.GetString("Archive/RemoveBookmark/CloseButtonText")
					};
					var result = await dialog.ShowAsync();
					if (result == ContentDialogResult.Primary)
						Data.Bookmarked = false;
				}
			}
			else if (!Data.Bookmarked)
			{
				var mode = Service.Settings.BookmarkReminderMode;
				if (Service.Settings.BookmarkReminder &&
					((_wasNew && mode == BookmarkReminderMode.New) || mode == BookmarkReminderMode.All))
				{
					var dialog = new ContentDialog
					{
						Title = lang.GetString("Archive/AddBookmark/Title"),
						PrimaryButtonText = lang.GetString("Archive/AddBookmark/PrimaryButtonText"),
						CloseButtonText = lang.GetString("Archive/AddBookmark/CloseButtonText")
					};
					var result = await dialog.ShowAsync();
					if (result == ContentDialogResult.Primary)
						Data.Bookmarked = true;
					_wasNew = false;
				}
			}
			if (Data.Bookmarked)
			{
				Data.BookmarkProgress = conv;
				if (Global.ControlFlags.V077)
					await Data.SetProgress(conv + 1);
			}
		}

		private void ImagesGrid_ItemClick(object sender, ItemClickEventArgs e)
		{
			if (!Data.ControlsEnabled)
				return;
			i = Data.ArchiveImages.IndexOf(e.ClickedItem as ImagePageSet);
			/*var anim = ImagesGrid.PrepareConnectedAnimation(GetOpenTarget(), e.ClickedItem, "Image");
			anim.Configuration = new BasicConnectedAnimationConfiguration();*/
			OpenReader();
		}

		private void Continue_Click(object sender, RoutedEventArgs e)
		{
			if (!Data.ControlsEnabled)
				return;
			i = Data.BookmarkProgress;
			/*var anim = ImagesGrid.PrepareConnectedAnimation(GetOpenTarget(), Data.ArchiveImages.ElementAt(i), "Image");
			anim.Configuration = new BasicConnectedAnimationConfiguration();*/
			OpenReader();
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			if (!Data.ShowReader)
				return;
			CloseReader();
		}

		private void Escape_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			args.Handled = true;
			if (!Data.ShowReader)
				return;
			CloseReader();
		}

		private void ReaderControl_KeyUp(object sender, KeyRoutedEventArgs e)
		{
			if (!Data.ShowReader)
				return;
			double offset = ScrollViewer.VerticalOffset;
			switch (e.Key)
			{
				case VirtualKey.Right:
					e.Handled = true;
					NextPage();
					break;
				case VirtualKey.Left:
					e.Handled = true;
					PrevPage();
					break;
				case VirtualKey.Up:
					e.Handled = true;
					if (Math.Floor(offset) <= 0)
					{
						if (Service.Settings.ReadRTL)
							NextPage();
						else
							PrevPage();
					}
					else
					{
						ScrollViewer.ChangeView(null, offset - Service.Settings.KeyboardScroll, null, false);
					}
					break;
				case VirtualKey.Down:
				case VirtualKey.Space:
					e.Handled = true;
					if (Math.Ceiling(offset) >= ScrollViewer.ScrollableHeight)
					{
						if (Service.Settings.ReadRTL)
							PrevPage();
						else
							NextPage();
					}
					else
					{
						ScrollViewer.ChangeView(null, offset + Service.Settings.KeyboardScroll, null, false);
					}
					break;
			}
		}

		private void ReaderControl_KeyDown(object sender, KeyRoutedEventArgs e)
		{
			if (e.Key == VirtualKey.Left || e.Key == VirtualKey.Right || e.Key == VirtualKey.Up || e.Key == VirtualKey.Down)
			{
				e.Handled = true;
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
						if (Math.Ceiling(ScrollViewer.VerticalOffset) >= ScrollViewer.ScrollableHeight && delta < 0)
						{
							e.Handled = true;
							if (Service.Settings.ReadRTL)
								PrevPage();
							else
								NextPage();
						}
						else if (Math.Floor(ScrollViewer.VerticalOffset) <= 0 && delta > 0)
						{
							e.Handled = true;
							if (Service.Settings.ReadRTL)
								NextPage();
							else
								PrevPage();
						}
						break;
					case VirtualKeyModifiers.Control:
						e.Handled = true;
						Data.ZoomValue += (int)(delta * 0.1);
						FitImages();
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
					PrevPage();
					return;
				}
				else if (pointerPoint.Properties.IsXButton2Pressed)
				{
					e.Handled = true;
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

		private void NextPage()
		{
			if (Data.ReaderIndex < Data.ArchiveImagesReader.Count() - 1)
			{
				++Data.ReaderIndex;
				ScrollViewer.ChangeView(null, 0, null, true);
			}
		}

		private void PrevPage()
		{
			if (Data.ReaderIndex > 0)
			{
				--Data.ReaderIndex;
				ScrollViewer.ChangeView(null, 0, null, true);
			}
		}

		private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e) => FitImages(false);

		private void ReaderControl_SizeChanged(object sender, SizeChangedEventArgs e) => FitImages(true);

		private void FitImages() => FitImages(false);

		private void FitImages(bool disableAnim = false)
		{
			if (ReaderControl.ActualWidth == 0 || ReaderControl.ActualHeight == 0)
				return;
			float zoomFactor;
			if (Service.Settings.FitToWidth)
			{
				zoomFactor = (float)Math.Min(ScrollViewer.ViewportWidth / ReaderControl.ActualWidth, Service.Settings.FitScaleLimit * 0.01);
			}
			else
			{
				zoomFactor = (float)Math.Min(ScrollViewer.ViewportWidth / ReaderControl.ActualWidth, ScrollViewer.ViewportHeight / ReaderControl.ActualHeight);
			}
			ScrollViewer.ChangeView(null, null, zoomFactor * (Data.ZoomValue * 0.01f), disableAnim);
		}

		private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e) => resizePixel.OnNext(ScrollViewer.ExtentHeight);

		private void EditButton_Click(object sender, RoutedEventArgs e) => Global.EventManager.AddTab(new ArchiveEditTab(Data.Archive));

		private async void CategoriesButton_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new CategoryArchive(Data.Archive.arcid, "Categories");
			await dialog.ShowAsync();
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
			await Data.Reload(true);
			args.Handled = true;
		}

		private async void RefreshButton_Click(object sender, RoutedEventArgs e) => await Data.Reload(true);

		public async void Refresh() => await Data.Reload(true);

		public void RemoveEvent()
		{
			Data.ZoomChangedEvent -= FitImages;
			Global.EventManager.RebuildReaderImagesSetEvent -= Data.CreateImageSets;
		}

		private void Tags_ItemClick(object sender, ItemClickEventArgs e) => Global.EventManager.AddTab(new SearchResultsTab((e.ClickedItem as ArchiveTagsGroupTag).FullTag));

		private string GetOpenTarget()
		{
			var target = "openL";
			if (Service.Settings.TwoPages)
			{
				if (Service.Settings.ReadRTL)
				{
					target = i % 2 == 0 ? "openL" : "openR";
					if (i == Data.Pages - 1)
						target = "openL";
					else if (i == 0)
						target = "openR";
				}
				else
					target = i % 2 == 0 ? "openR" : "openL";
			}
			return target;
		}

		private async void DeleteButton_Click(object sender, RoutedEventArgs e)
		{
			var lang = ResourceLoader.GetForCurrentView("Dialogs");
			var dialog = new ContentDialog()
			{
				Title = lang.GetString("RemoveArchive/Title").AsFormat(Data.Archive.title),
				Content = lang.GetString("RemoveArchive/Content"),
				PrimaryButtonText = lang.GetString("RemoveArchive/PrimaryButtonText"),
				CloseButtonText = lang.GetString("RemoveArchive/CloseButtonText")
			};
			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				await Data.DeleteArchive();
			}
		}
	}
}
