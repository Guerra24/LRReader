using LRReader.Internal;
using LRReader.Shared.Internal;
using LRReader.Shared.Models.Main;
using LRReader.UWP.Views.Tabs;
using LRReader.UWP.ViewModels;
using LRReader.UWP.Views.Items;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace LRReader.UWP.Views.Tabs.Content
{
	public sealed partial class ArchiveTabContent : UserControl
	{
		public ArchivePageViewModel Data;

		private int i;
		private bool _wasNew;
		private bool _opened;
		private bool _focus = true;
		private bool _fixDoubleCloseCall;

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
			_fixDoubleCloseCall = true;
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
				await Data.ClearNew();
				Data.Archive.isnew = "false";
				_wasNew = true;
			}
			await ImagesGrid.Fade(value: 0.0f, duration: 200).StartAsync();
			await ScrollViewer.Fade(value: 1.0f, duration: 200, easingMode: EasingMode.EaseIn).StartAsync();
			_focus = true;
			FocusReader();
		}

		public async void CloseReader()
		{
			if (!_fixDoubleCloseCall)
				return;
			_fixDoubleCloseCall = false;
			_focus = false;
			await ScrollViewer.Fade(value: 0.0f, duration: 200).StartAsync();
			Data.ShowReader = false;
			await ImagesGrid.Fade(value: 1.0f, duration: 200, easingMode: EasingMode.EaseIn).StartAsync();
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
			if (conv >= count - 1)
			{
				if (Data.Bookmarked && Global.SettingsManager.RemoveBookmark)
				{
					var dialog = new ContentDialog { Title = "Remove bookmark?", PrimaryButtonText = "Yes", CloseButtonText = "No" };
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
					var dialog = new ContentDialog { Title = "Bookmark archive?", PrimaryButtonText = "Yes", CloseButtonText = "No" };
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
			OpenReader();
		}

		private void Continue_Click(object sender, RoutedEventArgs e)
		{
			if (!Data.ControlsEnabled)
				return;
			i = Data.BookmarkProgress;
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
					ScrollViewer.ChangeView(null, offset - Global.SettingsManager.KeyboardScroll, null, false);
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
				if (Math.Ceiling(ScrollViewer.VerticalOffset) >= ScrollViewer.ScrollableHeight && delta < 0)
				{
					if (Global.SettingsManager.ReadRTL)
						PrevPage();
					else
						NextPage();
					e.Handled = true;
				}
				/*else if (Math.Floor(ScrollViewer.VerticalOffset) <= 0 && delta > 0)
				{
					if (Global.SettingsManager.ReadRTL)
						NextPage();
					else
						PrevPage();
					e.Handled = true;
				}*/
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

		private void FitImages(bool disableAnim)
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
			ScrollViewer.ChangeView(0, 0, zoomFactor * (Data.ZoomValue * 0.01f), disableAnim);
		}

		private void EditButton_Click(object sender, RoutedEventArgs e)
		{
			Global.EventManager.AddTab(new WebTab(Global.SettingsManager.Profile.ServerAddressBrowser + "/edit?id=" + Data.Archive.arcid));
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
			Global.EventManager.AddTab(new SearchResultsTab(e.ClickedItem as string));
		}
	}
}
