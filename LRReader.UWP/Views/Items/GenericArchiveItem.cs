using LRReader.Shared.Extensions;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Items;
using LRReader.UWP.Extensions;
using LRReader.UWP.Views.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using ParallaxView = Microsoft.UI.Xaml.Controls.ParallaxView;

namespace LRReader.UWP.Views.Items
{
	public sealed class GenericArchiveItem : Control
	{
		public ArchiveItemViewModel ViewModel { get; }

		private string _oldID;

		private bool _open;

		private ResourceLoader lang;

		public ControlFlags flags => Service.Api.ControlFlags;

		// Default
		private Grid Overlay;
		private TextBlock Title;
		private Grid TagsGrid;
		private Image Thumbnail;
		private Popup TagsPopup;
		private Storyboard ShowPopup;
		private Storyboard HidePopup;

		// Bookmark
		private TextBlock Progress;
		public ParallaxView Parallax;

		public GenericArchiveItem()
		{
			this.DefaultStyleKey = typeof(GenericArchiveItem);
			// TODO: Proper fix
			ViewModel = Service.Services.GetRequiredService<ArchiveItemViewModel>();
			lang = ResourceLoader.GetForCurrentView("Dialogs");
			DataContextChanged += Control_DataContextChanged;
			PointerPressed += Control_PointerPressed;
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			Overlay = GetTemplateChild("Overlay") as Grid;
			Title = GetTemplateChild("Title") as TextBlock;
			TagsGrid = GetTemplateChild("TagsGrid") as Grid;
			Thumbnail = GetTemplateChild("Thumbnail") as Image;
			TagsPopup = GetTemplateChild("TagsPopup") as Popup;
			ShowPopup = GetTemplateChild("ShowPopup") as Storyboard;
			HidePopup = GetTemplateChild("HidePopup") as Storyboard;

			Progress = GetTemplateChild("Progress") as TextBlock;
			Parallax = GetTemplateChild("Parallax") as ParallaxView;
		}

		public double PopupOffset
		{
			get => (double)GetValue(PopupOffsetProperty);
			set => SetValue(PopupOffsetProperty, value);
		}

		public int DecodePixelWidth
		{
			get => (int)GetValue(DecodePixelWidthProperty);
			set => SetValue(DecodePixelWidthProperty, value);
		}

		public int DecodePixelHeight
		{
			get => (int)GetValue(DecodePixelHeightProperty);
			set => SetValue(DecodePixelHeightProperty, value);
		}

		private async void Control_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (args.NewValue == null)
				return;
			var archive = args.NewValue as Archive;

			if (!archive.arcid.Equals(_oldID))
			{
				_oldID = archive.arcid;
				ViewModel.Archive = archive;

				Overlay?.SetVisualOpacity(0);
				Title?.SetVisualOpacity(0);
				Progress?.SetVisualOpacity(0);
				TagsGrid?.SetVisualOpacity(0);
				Thumbnail.Source = null;
				ViewModel.MissingImage = false;

				var image = new BitmapImage();
				image.DecodePixelType = DecodePixelType.Logical;
				if (DecodePixelHeight > 0)
					image.DecodePixelHeight = DecodePixelHeight;
				if (DecodePixelWidth > 0)
					image.DecodePixelWidth = DecodePixelWidth;
				image = await Service.ImageProcessing.ByteToBitmap(await Service.Images.GetThumbnailCached(ViewModel.Archive.arcid), image) as BitmapImage;

				if (image == null)
					ViewModel.MissingImage = true;
				else
				{
					if (image.PixelHeight != 0 && image.PixelWidth != 0)
						if (Math.Abs(ActualHeight / ActualWidth - image.PixelHeight / image.PixelWidth) > .65)
							Thumbnail.Stretch = Stretch.Uniform;
					Thumbnail.Source = image;
				}

				if (Service.Platform.AnimationsEnabled)
				{
					Overlay?.FadeIn();
					Title?.FadeIn();
					Progress?.FadeIn();
					TagsGrid?.FadeIn();
				}
				else
				{
					Overlay?.SetVisualOpacity(1);
					Title?.SetVisualOpacity(1);
					Progress?.SetVisualOpacity(1);
					TagsGrid?.SetVisualOpacity(1);
				}
			}
		}

		public void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
		{
			Service.Tabs.OpenTab(Tab.Archive, false, ViewModel.Archive);
		}

		public void EditMenuItem_Click(object sender, RoutedEventArgs e)
		{
			Service.Tabs.OpenTab(Tab.ArchiveEdit, ViewModel.Archive);
		}

		public async void DownloadMenuItem_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.Downloading = true;
			var download = await ViewModel.DownloadArchive();
			if (download == null)
			{
				ViewModel.Downloading = false;
				return;
			}

			var savePicker = new FileSavePicker();
			savePicker.SuggestedStartLocation = PickerLocationId.Downloads;
			savePicker.FileTypeChoices.Add(download.Type + " File", new List<string>() { download.Type });
			savePicker.SuggestedFileName = download.Name;

			StorageFile file = await savePicker.PickSaveFileAsync();
			ViewModel.Downloading = false;
			if (file != null)
			{
				CachedFileManager.DeferUpdates(file);
				await FileIO.WriteBytesAsync(file, download.Data);
				FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
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

		public void Add_Click(object sender, RoutedEventArgs e) => ViewModel.Bookmarked = true;

		public void Remove_Click(object sender, RoutedEventArgs e) => ViewModel.Bookmarked = false;

		public async void TagsGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
		{
			_open = true;
			await Task.Delay(TimeSpan.FromMilliseconds(Service.Platform.HoverTime));
			if (_open)
			{
				_open = false;
				TagsPopup.ClampPopup(PopupOffset);
				TagsPopup.IsOpen = true;
				if (Service.Platform.AnimationsEnabled)
					ShowPopup.Begin();
			}
		}

		public void TagsGrid_PointerExited(object sender, PointerRoutedEventArgs e)
		{
			if (_open)
			{
				_open = false;
				TagsPopup.IsOpen = false;
			}
			if (TagsPopup.IsOpen)
			{
				if (Service.Platform.AnimationsEnabled)
					HidePopup.Begin();
				else
					TagsPopup.IsOpen = false;
			}
		}

		public void TagsGrid_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
		{
			if (_open)
			{
				_open = false;
				TagsPopup.IsOpen = false;
			}
			if (TagsPopup.IsOpen)
				HidePopup.Begin();
		}

		public void HidePopup_Completed(object sender, object e)
		{
			TagsPopup.IsOpen = false;
		}

		private void Control_PointerPressed(object sender, PointerRoutedEventArgs e)
		{
			var pointerPoint = e.GetCurrentPoint(this);
			if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
			{
				if (pointerPoint.Properties.IsMiddleButtonPressed)
				{
					Service.Tabs.OpenTab(Tab.Archive, false, ViewModel.Archive);
					e.Handled = true;
				}
			}
		}

		public async void CategoriesButton_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new CategoryArchive(ViewModel.Archive.arcid, ViewModel.Archive.title);
			await dialog.ShowAsync();
		}

		public async void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new ContentDialog()
			{
				Title = lang.GetString("RemoveArchive/Title").AsFormat(ViewModel.Archive.title),
				Content = lang.GetString("RemoveArchive/Content"),
				PrimaryButtonText = lang.GetString("RemoveArchive/PrimaryButtonText"),
				CloseButtonText = lang.GetString("RemoveArchive/CloseButtonText")
			};
			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				await ViewModel.DeleteArchive();
			}
		}

		public static readonly DependencyProperty PopupOffsetProperty = DependencyProperty.Register("PopupOffset", typeof(double), typeof(GenericArchiveItem), new PropertyMetadata(null));
		public static readonly DependencyProperty DecodePixelWidthProperty = DependencyProperty.Register("DecodePixelWidth", typeof(int), typeof(GenericArchiveItem), new PropertyMetadata(0));
		public static readonly DependencyProperty DecodePixelHeightProperty = DependencyProperty.Register("DecodePixelHeight", typeof(int), typeof(GenericArchiveItem), new PropertyMetadata(0));
	}
}
