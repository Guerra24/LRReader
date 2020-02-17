using LRReader.Internal;
using LRReader.Shared.Internal;
using LRReader.Shared.Models.Main;
using LRReader.ViewModels;
using LRReader.Views.Items;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LRReader.Views.Tabs.Content
{
	public sealed partial class ArchiveTabContent : UserControl
	{
		public ArchivePageViewModel Data;

		private int i;
		private bool _wasNew;
		private bool _opened;
		private bool _focus = true;

		public ArchiveTabContent()
		{
			this.InitializeComponent();
			Data = new ArchivePageViewModel();
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
			i = Data.ArchiveImages.IndexOf(e.ClickedItem as string);
			OpenReader();
		}

		private void Continue_Click(object sender, RoutedEventArgs e)
		{
			i = Data.BookmarkProgress;
			OpenReader();
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			CloseReader();
		}

		private void Escape_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			CloseReader();
		}

		private void ReaderControl_KeyUp(object sender, KeyRoutedEventArgs e)
		{
			if (!Data.ShowReader)
				return;
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
				case VirtualKey.Space:
					double offset = ScrollViewer.VerticalOffset;
					if (offset >= ScrollViewer.ScrollableHeight)
					{
						if (Global.SettingsManager.ReadRTL)
							PrevPage();
						else
							NextPage();
					}
					else
					{
						ScrollViewer.ChangeView(null, offset + Global.SettingsManager.SpacebarScroll, null, false);
					}
					e.Handled = true;
					break;
			}
		}

		private void ReaderControl_KeyDown(object sender, KeyRoutedEventArgs e)
		{
			if (e.Key == VirtualKey.Left || e.Key == VirtualKey.Right)
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

		private void ScrollViewer_Tapped(object sender, TappedRoutedEventArgs e)
		{
			var point = e.GetPosition(ScrollViewer);
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

		private void ScrollViewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
		{
			if (ReaderControl.ActualWidth == 0 || ReaderControl.ActualHeight == 0)
				return;
			var point = e.GetPosition(ScrollViewer);
			var ttv = ReaderControl.TransformToVisual(this);
			var center = ttv.TransformPoint(new Point(0, 0));
			var zoomFactor = (float)Math.Min(ScrollViewer.ViewportWidth / ReaderControl.ActualWidth, ScrollViewer.ViewportHeight / ReaderControl.ActualHeight);
			if (Math.Abs(ScrollViewer.ZoomFactor - zoomFactor * Global.SettingsManager.BaseZoom) > 0.01)
				FitImages(false);
			else
				ScrollViewer.ChangeView(point.X - center.X * 2.0, point.Y - center.Y * 2.0, zoomFactor * Global.SettingsManager.ZoomedFactor);
		}

		private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			FitImages(false);
		}

		private void ReaderControl_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			FitImages(true);
		}

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
			System.Diagnostics.Debug.WriteLine(zoomFactor);
			ScrollViewer.ChangeView(0, 0, zoomFactor * Global.SettingsManager.BaseZoom, disableAnim);
		}

		private async void EditButton_Click(object sender, RoutedEventArgs e)
		{
			await Util.OpenInBrowser(new Uri(Global.SettingsManager.Profile.ServerAddressBrowser + "/edit?id=" + Data.Archive.arcid));
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

		private async void RefreshButton_Click(object sender, RoutedEventArgs e)
		{
			await Data.Reload(true);
		}

		public void RemoveEvent()
		{
			Global.EventManager.RebuildReaderImagesSetEvent -= Data.CreateImageSets;
		}


	}
}
