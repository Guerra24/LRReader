using LRReader.Internal;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.UWP.Extensions;
using LRReader.UWP.ViewModels.Items;
using LRReader.UWP.Views.Tabs;
using System;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace LRReader.UWP.Views.Items
{
	public sealed partial class CategoryEditArchive : UserControl
	{

		private ArchiveItemViewModel ViewModel;

		private string _oldID = "";

		private bool _open;

		public CategoryEditArchive()
		{
			this.InitializeComponent();
			ViewModel = new ArchiveItemViewModel();
		}

		private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (args.NewValue == null)
				return;
			ViewModel.Archive = args.NewValue as Archive;

			if (!_oldID.Equals(ViewModel.Archive.arcid))
			{
				Overlay.SetVisualOpacity(0);
				Title.SetVisualOpacity(0);
				TagsGrid.SetVisualOpacity(0);
				Thumbnail.Source = null;
				ViewModel.MissingImage = false;

				var image = new BitmapImage();
				image.DecodePixelType = DecodePixelType.Logical;
				image.DecodePixelHeight = 138;
				image = await Global.ImageProcessing.ByteToBitmap(await Service.Images.GetThumbnailCached(ViewModel.Archive.arcid), image);
				if (image != null)
				{
					if (image.PixelHeight != 0 && image.PixelWidth != 0)
						if (Math.Abs(ActualHeight / ActualWidth - image.PixelHeight / image.PixelWidth) > .65)
							Thumbnail.Stretch = Stretch.Uniform;
					Thumbnail.Source = image;
					Thumbnail.FadeIn();
				}
				else
					ViewModel.MissingImage = true;

				Overlay.FadeIn();
				Title.FadeIn();
				TagsGrid.FadeIn();
				_oldID = ViewModel.Archive.arcid;
			}
		}

		private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
		{
			Service.Events.AddTab(new ArchiveTab(ViewModel.Archive), false);
		}

		private async void TagsGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
		{
			_open = true;
			await Task.Delay(TimeSpan.FromMilliseconds(300));
			if (_open)
			{
				_open = false;
				TagsPopup.IsOpen = true;
				ShowPopup.Begin();
			}
		}

		private void TagsGrid_PointerExited(object sender, PointerRoutedEventArgs e)
		{
			if (_open)
			{
				_open = false;
				TagsPopup.IsOpen = false;
			}
			if (TagsPopup.IsOpen)
				HidePopup.Begin();
		}

		private void HidePopup_Completed(object sender, object e)
		{
			TagsPopup.IsOpen = false;
		}

		private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
		{
			var pointerPoint = e.GetCurrentPoint(this);
			if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
			{
				if (pointerPoint.Properties.IsMiddleButtonPressed)
				{
					Service.Events.AddTab(new ArchiveTab(ViewModel.Archive), false);
					e.Handled = true;
				}
			}
		}

	}
}
