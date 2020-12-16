using LRReader.Internal;
using LRReader.Shared.Internal;
using LRReader.Shared.Models.Main;
using LRReader.UWP.ViewModels;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Input;
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
		public ArchivePageViewModel Data;

		private int i;
		private bool _wasNew;
		private bool _opened;
		private bool _focus = true;

		private ResourceLoader lang = ResourceLoader.GetForCurrentView("Tabs");

		public ArchiveTabContent()
		{
			this.InitializeComponent();
			Data = new ArchivePageViewModel();
			Data.ZoomChangedEvent += FitImages;
			Global.EventManager.RebuildReaderImagesSetEvent += Data.CreateImageSets;
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			Data.ReloadBookmarkedObject();
			FocusReader();
		}

		public async void LoadArchive(Archive archive)
		{
			Data.Archive = archive;
			await Data.Reload(true);
			if (!_opened)
			{
				if (Global.SettingsManager.OpenReader)
				{
					if (Data.Bookmarked)
						i = Data.BookmarkProgress;
					OpenReader();
				}
				_opened = true;
			}
		}

		private async void OpenReader()
		{
			Data.ShowReader = true;
			int count = Data.Pages;
			if (Global.SettingsManager.TwoPages)
			{
				if (i != 0)
				{
					--i; i /= 2; ++i;
				}
				if (Global.SettingsManager.ReadRTL)
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
				if (Global.SettingsManager.ReadRTL)
					Data.ReaderIndex = count - i - 1;
				else
					Data.ReaderIndex = i;
			}

			if (Data.Archive.IsNewArchive())
			{
				_wasNew = true;
			}
			await ImagesGrid.Fade(value: 0.0f, duration: 200).StartAsync();
			await ScrollViewer.Fade(value: 1.0f, duration: 200, easingMode: EasingMode.EaseIn).StartAsync();
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
			await ScrollViewer.Fade(value: 0.0f, duration: 200).StartAsync();
			Data.ShowReader = false;
			int conv = Data.ReaderIndex;
			int count = Data.Pages;
			if (Global.SettingsManager.TwoPages)
			{
				if (conv != 0)
				{
					conv *= 2;
				}
				if (Global.SettingsManager.ReadRTL)
				{
					conv = count - conv - (count % 2);
				}
			}
			else
			{
				if (Global.SettingsManager.ReadRTL)
					conv = count - conv - 1;
			}
			conv = Math.Clamp(conv, 0, count - 1);

			/*int leftTarget = conv;
			if (Global.SettingsManager.TwoPages)
			{
				leftTarget = Math.Max(conv - 1, 0);
				if (conv == Data.Pages - 1 && Data.Pages % 2 == 0)
					leftTarget++;
			}
			int rightTarget = conv;

			if (Global.SettingsManager.ReadRTL)
			{
				int tmp = leftTarget;
				leftTarget = rightTarget;
				rightTarget = tmp;
			}

			if (Data.ReaderContent.LeftImage != null)
				await ImagesGrid.TryStartConnectedAnimationAsync(animLeft, Data.ArchiveImages.ElementAt(leftTarget), "Image");
			if (Data.ReaderContent.RightImage != null)
				await ImagesGrid.TryStartConnectedAnimationAsync(animRight, Data.ArchiveImages.ElementAt(rightTarget), "Image");*/

			await ImagesGrid.Fade(value: 1.0f, duration: 200, easingMode: EasingMode.EaseIn).StartAsync();

			if (conv >= count - Math.Min(10, Math.Ceiling(count * 0.1)))
			{
				if (Data.Archive.IsNewArchive())
				{
					await Data.ClearNew();
					Data.Archive.isnew = "false";
				}
				if (Data.Bookmarked && Global.SettingsManager.RemoveBookmark)
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
				var mode = Global.SettingsManager.BookmarkReminderMode;
				if (Global.SettingsManager.BookmarkReminder &&
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
				Data.BookmarkProgress = conv;
		}

		private void ImagesGrid_ItemClick(object sender, ItemClickEventArgs e)
		{
			if (!Data.ControlsEnabled)
				return;
			i = Data.ArchiveImages.IndexOf(e.ClickedItem as string);
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
					NextPage();
					e.Handled = true;
					break;
				case VirtualKey.Left:
					PrevPage();
					e.Handled = true;
					break;
				case VirtualKey.Up:
					if (Math.Floor(offset) <= 0)
					{
						if (Global.SettingsManager.ReadRTL)
							NextPage();
						else
							PrevPage();
					}
					else
					{
						ScrollViewer.ChangeView(null, offset - Global.SettingsManager.KeyboardScroll, null, false);
					}
					e.Handled = true;
					break;
				case VirtualKey.Down:
				case VirtualKey.Space:
					if (Math.Ceiling(offset) >= ScrollViewer.ScrollableHeight)
					{
						if (Global.SettingsManager.ReadRTL)
							PrevPage();
						else
							NextPage();
					}
					else
					{
						ScrollViewer.ChangeView(null, offset + Global.SettingsManager.KeyboardScroll, null, false);
					}
					e.Handled = true;
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
							if (Global.SettingsManager.ReadRTL)
								PrevPage();
							else
								NextPage();
							e.Handled = true;
						}
						else if (Math.Floor(ScrollViewer.VerticalOffset) <= 0 && delta > 0)
						{
							if (Global.SettingsManager.ReadRTL)
								NextPage();
							else
								PrevPage();
							e.Handled = true;
						}
						break;
					case VirtualKeyModifiers.Control:
						Data.ZoomValue += (int)(delta * 0.1);
						FitImages();
						e.Handled = true;
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
					PrevPage();
					return;
				}
				else if (pointerPoint.Properties.IsXButton2Pressed)
				{
					NextPage();
					return;
				}
			}
			var point = pointerPoint.Position;
			double distance = ScrollViewer.ActualWidth / 6.0;
			if (point.X < distance)
			{
				PrevPage();
			}
			else if (point.X > ScrollViewer.ActualWidth - distance)
			{
				NextPage();
			}
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

		private void ReaderControl_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
		{
			double vertical = ScrollViewer.VerticalOffset;
			double horizontal = ScrollViewer.HorizontalOffset;
			ScrollViewer.ChangeView(horizontal - e.Delta.Translation.X, vertical - e.Delta.Translation.Y, null, true);
		}

		private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e) => FitImages(false);

		private void ReaderControl_SizeChanged(object sender, SizeChangedEventArgs e) => FitImages(true);

		private void FitImages() => FitImages(false);

		private void FitImages(bool disableAnim = false)
		{
			if (ReaderControl.ActualWidth == 0 || ReaderControl.ActualHeight == 0)
				return;
			float zoomFactor;
			if (Global.SettingsManager.FitToWidth)
			{
				zoomFactor = (float)Math.Min(ScrollViewer.ViewportWidth / ReaderControl.ActualWidth, Global.SettingsManager.FitScaleLimit * 0.01);
			}
			else
			{
				zoomFactor = (float)Math.Min(ScrollViewer.ViewportWidth / ReaderControl.ActualWidth, ScrollViewer.ViewportHeight / ReaderControl.ActualHeight);
			}
			ScrollViewer.ChangeView(null, null, zoomFactor * (Data.ZoomValue * 0.01f), disableAnim);
		}

		private void EditButton_Click(object sender, RoutedEventArgs e)
		{
			Global.EventManager.AddTab(new ArchiveEditTab(Data.Archive));
		}

		private async void DonwloadButton_Click(object sender, RoutedEventArgs e)
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

		private void Tags_ItemClick(object sender, ItemClickEventArgs e)
		{
			Global.EventManager.AddTab(new SearchResultsTab((e.ClickedItem as ArchiveTagsGroupTag).FullTag));
		}

		private string GetOpenTarget()
		{
			var target = "openL";
			if (Global.SettingsManager.TwoPages)
			{
				if (Global.SettingsManager.ReadRTL)
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
	}
}
