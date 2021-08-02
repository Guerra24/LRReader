using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Items;
using LRReader.UWP.Extensions;
using Microsoft.Extensions.DependencyInjection;
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
	public sealed partial class ArchiveHitItem : UserControl
	{

		private ArchiveItemViewModel LeftViewModel, RightViewModel;

		private ArchiveHitViewModel Data;

		private ArchiveHit _old = new ArchiveHit { Left = new Archive { arcid = "" }, Right = new Archive { arcid = "" } };

		private bool _open;

		public ArchiveHitItem()
		{
			this.InitializeComponent();
			// TODO: Proper fix
			LeftViewModel = Service.Services.GetRequiredService<ArchiveItemViewModel>();
			RightViewModel = Service.Services.GetRequiredService<ArchiveItemViewModel>();
			Data = Service.Services.GetRequiredService<ArchiveHitViewModel>();
		}

		private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (args.NewValue == null)
				return;
			VisualStateManager.GoToState(this, "Normal", true);
			var hit = args.NewValue as ArchiveHit;

			if (!hit.Equals(_old))
			{
				_old = hit;
				Data.ArchiveHit = hit;
				LeftViewModel.Archive = hit.Left;
				RightViewModel.Archive = hit.Right;

				LeftGrid.SetVisualOpacity(0);
				RightGrid.SetVisualOpacity(0);
				LeftThumbnail.Source = null;
				RightThumbnail.Source = null;
				LeftViewModel.MissingImage = RightViewModel.MissingImage = false;

				var leftImage = new BitmapImage();
				leftImage.DecodePixelType = DecodePixelType.Logical;
				leftImage.DecodePixelHeight = 275;
				leftImage = await Service.ImageProcessing.ByteToBitmap(await Service.Images.GetThumbnailCached(LeftViewModel.Archive.arcid, ignoreCache: true), leftImage) as BitmapImage;

				var rightImage = new BitmapImage();
				rightImage.DecodePixelType = DecodePixelType.Logical;
				rightImage.DecodePixelHeight = 275;
				rightImage = await Service.ImageProcessing.ByteToBitmap(await Service.Images.GetThumbnailCached(RightViewModel.Archive.arcid, ignoreCache: true), rightImage) as BitmapImage;
				if (leftImage != null && rightImage != null)
				{
					if (leftImage.PixelHeight != 0 && leftImage.PixelWidth != 0)
						if (Math.Abs(ActualHeight / ActualWidth - leftImage.PixelHeight / leftImage.PixelWidth) > .65)
							LeftThumbnail.Stretch = Stretch.Uniform;
					if (rightImage.PixelHeight != 0 && rightImage.PixelWidth != 0)
						if (Math.Abs(ActualHeight / ActualWidth - rightImage.PixelHeight / rightImage.PixelWidth) > .65)
							RightThumbnail.Stretch = Stretch.Uniform;
					LeftThumbnail.Source = leftImage;
					RightThumbnail.Source = rightImage;
				}
				else
					LeftViewModel.MissingImage = RightViewModel.MissingImage = true;

				if (Service.Platform.AnimationsEnabled)
				{
					LeftGrid.FadeIn();
					RightGrid.FadeIn();
				}
				else
				{
					LeftGrid.SetVisualOpacity(1);
					RightGrid.SetVisualOpacity(1);
				}
			}
		}

		private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
		{
			Service.Tabs.OpenTab(Tab.Archive, false, (sender as MenuFlyoutItem).Tag);
		}


		private async void LeftTagsGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
		{
			_open = true;
			await Task.Delay(TimeSpan.FromMilliseconds(Service.Platform.HoverTime));
			if (_open)
			{
				_open = false;
				LeftTagsPopup.ClampPopup(-57);
				LeftTagsPopup.IsOpen = true;
				if (Service.Platform.AnimationsEnabled)
					ShowLeftPopup.Begin();
			}
		}

		private void LeftTagsGrid_PointerExited(object sender, PointerRoutedEventArgs e)
		{
			if (_open)
			{
				_open = false;
				LeftTagsPopup.IsOpen = false;
			}
			if (LeftTagsPopup.IsOpen)
			{
				if (Service.Platform.AnimationsEnabled)
					HideLeftPopup.Begin();
				else
					LeftTagsPopup.IsOpen = false;
			}
		}

		private void LeftHidePopup_Completed(object sender, object e)
		{
			LeftTagsPopup.IsOpen = false;
		}

		private async void RightTagsGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
		{
			_open = true;
			await Task.Delay(TimeSpan.FromMilliseconds(Service.Platform.HoverTime));
			if (_open)
			{
				_open = false;
				RightTagsPopup.ClampPopup(-57);
				RightTagsPopup.IsOpen = true;
				if (Service.Platform.AnimationsEnabled)
					ShowRightPopup.Begin();
			}
		}

		private void RightTagsGrid_PointerExited(object sender, PointerRoutedEventArgs e)
		{
			if (_open)
			{
				_open = false;
				RightTagsPopup.IsOpen = false;
			}
			if (RightTagsPopup.IsOpen)
			{
				if (Service.Platform.AnimationsEnabled)
					HideRightPopup.Begin();
				else
					RightTagsPopup.IsOpen = false;
			}
		}

		private void LeftGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
		{
			VisualStateManager.GoToState(this, "PointerOverLeft", true);
		}

		private void LeftGrid_PointerExited(object sender, PointerRoutedEventArgs e)
		{
			VisualStateManager.GoToState(this, "Normal", true);
		}

		private void RightGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
		{
			VisualStateManager.GoToState(this, "PointerOverRight", true);
		}

		private void RightGrid_PointerExited(object sender, PointerRoutedEventArgs e)
		{
			VisualStateManager.GoToState(this, "Normal", true);
		}

		private void LeftDelete_Click(object sender, RoutedEventArgs e)
		{
			VisualStateManager.GoToState(this, "LeftConfirm", true);
		}

		private void RightDelete_Click(object sender, RoutedEventArgs e)
		{
			VisualStateManager.GoToState(this, "RightConfirm", true);
		}

		private void RightCancel_Click(object sender, RoutedEventArgs e)
		{
			VisualStateManager.GoToState(this, "PointerOverRight", true);
		}

		private void LeftCancel_Click(object sender, RoutedEventArgs e)
		{
			VisualStateManager.GoToState(this, "PointerOverLeft", true);
		}

		private void LeftGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
		{
			var pointerPoint = e.GetCurrentPoint(LeftGrid);
			if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
			{
				if (pointerPoint.Properties.IsMiddleButtonPressed)
				{
					Service.Tabs.OpenTab(Tab.Archive, false, LeftViewModel.Archive);
					e.Handled = true;
				}
			}
		}

		private void RightGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
		{
			var pointerPoint = e.GetCurrentPoint(RightGrid);
			if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
			{
				if (pointerPoint.Properties.IsMiddleButtonPressed)
				{
					Service.Tabs.OpenTab(Tab.Archive, false, RightViewModel.Archive);
					e.Handled = true;
				}
			}
		}

		private void RightHidePopup_Completed(object sender, object e)
		{
			RightTagsPopup.IsOpen = false;
		}
	}
}
