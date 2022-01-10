using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using LRReader.Shared;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Items;
using LRReader.UWP.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.UI.Animations;
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
		private static AnimationBuilder FadeIn = AnimationBuilder.Create().Opacity(to: 1, duration: TimeSpan.FromMilliseconds(150), easingMode: EasingMode.EaseIn);

		public ArchiveItemViewModel ViewModel { get; }

		private string _oldID;

		private bool _open;

		// Default
		private Grid Overlay;
		private TextBlock Title;
		private Grid TagsGrid;
		private Image Thumbnail;
		private Flyout TagsFlyout;

		// Bookmark
		private TextBlock Progress;
		public ParallaxView Parallax;

		// Internal
		public IList<Archive> Group;

		public GenericArchiveItem()
		{
			this.DefaultStyleKey = typeof(GenericArchiveItem);
			// TODO: Proper fix
			ViewModel = Service.Services.GetRequiredService<ArchiveItemViewModel>();
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
			TagsFlyout = GetTemplateChild("TagsFlyout") as Flyout;

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

				var image = await Service.ImageProcessing.ByteToBitmap(await Service.Images.GetThumbnailCached(ViewModel.Archive.arcid), DecodePixelWidth, DecodePixelHeight) as BitmapImage;

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
					Overlay?.Start(FadeIn);
					Title?.Start(FadeIn);
					Progress?.Start(FadeIn);
					TagsGrid?.Start(FadeIn);
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

		public void MenuFlyoutItem_Click(object sender, RoutedEventArgs e) => Service.Archives.OpenTab(ViewModel.Archive, false, Group);

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
			if (_open && !TagsFlyout.IsOpen)
			{
				_open = false;
				var placement = (FlyoutPlacementMode)typeof(TagsPopupLocation).GetMember(Service.Settings.TagsPopup.ToString())[0].GetCustomAttribute<IntAttribute>(false).Value;
				TagsFlyout.ShowAt(TagsGrid, new FlyoutShowOptions
				{
					Position = e.GetCurrentPoint(TagsGrid).Position,
					Placement = placement,
					ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway
				});
			}
		}

		public void TagsGrid_PointerExited(object sender, PointerRoutedEventArgs e)
		{
			if (_open)
			{
				_open = false;
			}
		}

		private void Control_PointerPressed(object sender, PointerRoutedEventArgs e)
		{
			var pointerPoint = e.GetCurrentPoint(this);
			if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
			{
				if (pointerPoint.Properties.IsMiddleButtonPressed)
				{
					Service.Archives.OpenTab(ViewModel.Archive, false, Group);
					e.Handled = true;
				}
			}
		}

		public static readonly DependencyProperty PopupOffsetProperty = DependencyProperty.Register("PopupOffset", typeof(double), typeof(GenericArchiveItem), new PropertyMetadata(null));
		public static readonly DependencyProperty DecodePixelWidthProperty = DependencyProperty.Register("DecodePixelWidth", typeof(int), typeof(GenericArchiveItem), new PropertyMetadata(0));
		public static readonly DependencyProperty DecodePixelHeightProperty = DependencyProperty.Register("DecodePixelHeight", typeof(int), typeof(GenericArchiveItem), new PropertyMetadata(0));
	}
}
