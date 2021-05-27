using LRReader.Shared.Extensions;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using LRReader.UWP.Extensions;
using LRReader.UWP.Views.Dialogs;
using LRReader.UWP.Views.Items;
using Microsoft.Toolkit.Uwp.UI;
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
		private static AnimationBuilder FadeOut = AnimationBuilder.Create().Opacity(to: 0, duration: TimeSpan.FromMilliseconds(200), easingMode: EasingMode.EaseIn);

		public ArchivePageViewModel Data;

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

			Data = DataContext as ArchivePageViewModel;
			Data.ZoomChangedEvent += FitImages;

			flags = Service.Api.ControlFlags;

			resizePixel.Throttle(TimeSpan.FromMilliseconds(250))
				.Subscribe(async (height) =>
				await Service.Dispatcher.RunAsync(() =>
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
				if (Service.Api.ControlFlags.V077 && Data.Bookmarked && Data.BookmarkProgress + 1 != Data.Archive.progress && Data.Archive.progress > 0)
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
					var page = 0;
					if (Data.Bookmarked)
						page = Data.BookmarkProgress;
					OpenReader(page);
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

		private async void OpenReader(int page, object item = null)
		{
			var readerSet = Data.ArchiveImagesReader.First(s => s.Page >= page);
			var index = Data.ArchiveImagesReader.IndexOf(readerSet);

			if (Service.Platform.AnimationsEnabled && item != null)
			{
				var anim = ImagesGrid.PrepareConnectedAnimation(GetOpenTarget(readerSet, page), item, "Image");
				anim.Configuration = new BasicConnectedAnimationConfiguration();
			}

			Data.ShowReader = true;
			Data.ReaderIndex = index;

			if (Data.Archive.IsNewArchive())
				_wasNew = true;
			if (Service.Platform.AnimationsEnabled)
				await FadeIn.StartAsync(ScrollViewer);
			else
				ScrollViewer.SetVisualOpacity(1);
			_focus = true;
			FocusReader();
		}

		public async void CloseReader()
		{
			var animate = Service.Platform.AnimationsEnabled;
			var left = ReaderControl.FindDescendant("LeftImage");
			var right = ReaderControl.FindDescendant("RightImage");
			ConnectedAnimation animLeft = null, animRight = null;
			if (animate)
			{
				if (Data.ReaderContent.LeftImage != null)
				{
					animLeft = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("closeL", left);
					animLeft.Configuration = new BasicConnectedAnimationConfiguration();
				}
				if (Data.ReaderContent.RightImage != null)
				{
					animRight = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("closeR", right);
					animRight.Configuration = new BasicConnectedAnimationConfiguration();
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
				if (Service.Settings.ReadRTL)
				{
					int tmp = leftTarget;
					leftTarget = rightTarget;
					rightTarget = tmp;
				}
			}
			if (animate)
			{
				if (Data.ReaderContent.LeftImage != null && animLeft != null)
					await ImagesGrid.TryStartConnectedAnimationAsync(animLeft, Data.ArchiveImages.ElementAt(leftTarget), "Image");
				if (Data.ReaderContent.RightImage != null & animRight != null)
					await ImagesGrid.TryStartConnectedAnimationAsync(animRight, Data.ArchiveImages.ElementAt(rightTarget), "Image");
				await FadeOut.StartAsync(ScrollViewer);
			}
			else
			{
				ScrollViewer.SetVisualOpacity(0);
			}
			Data.ShowReader = false;

			if (currentPage >= count - Math.Min(10, Math.Ceiling(count * 0.1)))
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
				Data.BookmarkProgress = currentPage;
				if (Service.Api.ControlFlags.V077)
					await Data.SetProgress(currentPage + 1);
			}
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
						PrevPage();
					else
						ScrollViewer.ChangeView(null, offset - Service.Settings.KeyboardScroll, null, false);
					break;
				case VirtualKey.Down:
				case VirtualKey.Space:
					e.Handled = true;
					if (Math.Ceiling(offset) >= ScrollViewer.ScrollableHeight)
						NextPage();
					else
						ScrollViewer.ChangeView(null, offset + Service.Settings.KeyboardScroll, null, false);
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
							NextPage();
						}
						else if (Math.Floor(ScrollViewer.VerticalOffset) <= 0 && delta > 0)
						{
							e.Handled = true;
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
			if (Service.Settings.ReadRTL)
				GoLeft();
			else
				GoRight();
		}

		private void PrevPage()
		{
			if (Service.Settings.ReadRTL)
				GoRight();
			else
				GoLeft();
		}

		private void GoRight()
		{
			if (Data.ReaderIndex < Data.ArchiveImagesReader.Count() - 1)
			{
				++Data.ReaderIndex;
				ScrollViewer.ChangeView(null, 0, null, true);
			}
		}

		private void GoLeft()
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
			ScrollViewer.ChangeView(null, null, zoomFactor * (Data.ZoomValue * 0.01f), disableAnim || !Service.Platform.AnimationsEnabled);
		}

		private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e) => resizePixel.OnNext(ScrollViewer.ExtentHeight);

		private void EditButton_Click(object sender, RoutedEventArgs e) => Service.Tabs.OpenTab(Tab.ArchiveEdit, Data.Archive);

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
			Data.UnHook();
		}

		private void Tags_ItemClick(object sender, ItemClickEventArgs e) => Service.Tabs.OpenTab(Tab.SearchResults, (e.ClickedItem as ArchiveTagsGroupTag).FullTag);

		private string GetOpenTarget(ReaderImageSet target, int item)
		{
			var targetAnim = "openL";
			if (target.TwoPages)
			{
				if (Service.Settings.ReadRTL)
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
