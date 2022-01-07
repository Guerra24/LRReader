using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.UWP.Extensions;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace LRReader.UWP.Views.Items
{
	public sealed partial class ArchiveImage : UserControl
	{
		private ImagePageSet _oldUrl = new ImagePageSet("", "", 0);
		private Container Data = new Container();
		private bool _loading;

		public ArchiveImage()
		{
			this.InitializeComponent();
		}

		public bool KeepOverlayOpen
		{
			get => (bool)GetValue(KeepOverlayOpenProperty);
			set
			{
				if (value)
					VisualStateManager.GoToState(this, "PointerOver", false);
				else
					VisualStateManager.GoToState(this, "Normal", false);
				SetValue(KeepOverlayOpenProperty, value);
			}
		}

		public bool HideOverlay
		{
			get => (bool)GetValue(HideOverlayProperty);
			set => SetValue(HideOverlayProperty, value);
		}

		public bool ShowExtraDetails
		{
			get => (bool)GetValue(ShowExtraDetailsProperty);
			set => SetValue(ShowExtraDetailsProperty, value);
		}

		private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (args.NewValue == null)
				return;
			var n = args.NewValue as ImagePageSet;
			if (!_oldUrl.Equals(n))
			{
				await ReloadImage(n);
				_oldUrl = n;
			}
		}

		private async void Reload_Click(object sender, RoutedEventArgs e) => await ReloadImage(_oldUrl, true);

		private async Task ReloadImage(ImagePageSet n, bool forced = false)
		{
			if (_loading)
				return;
			_loading = true;
			Content.SetVisualOpacity(0);
			Image.Source = null;
			Ring.IsActive = true;
			Data.MissingImage = false;
			if (!HideOverlay)
				PageCount.Text = n.Page.ToString();
			if (ShowExtraDetails)
			{
				Format.Text = n.Image.Substring(n.Image.LastIndexOf('.') + 1).ToUpper();
				var size = await Service.Images.GetImageSizeCached(n.Image);
				Resolution.Text = $"{size.Width}x{size.Height}";
			}

			BitmapImage image;
			if (Service.Api.ControlFlags.V084)
				image = await Service.ImageProcessing.ByteToBitmap(await Service.Images.GetThumbnailCached(n.Id, n.Page), decodeHeight: 275) as BitmapImage;
			else
				image = await Service.ImageProcessing.ByteToBitmap(await Service.Images.GetImageCached(n.Image, forced), decodeHeight: 275, transcode: n.Image.EndsWith("avif")) as BitmapImage;
			Ring.IsActive = false;
			/*if (image.PixelHeight != 0 && image.PixelWidth != 0)
			{
				if (Math.Abs(ActualHeight / ActualWidth - image.PixelHeight / image.PixelWidth) > .65)
					Image.Stretch = Stretch.Uniform;
				else
					Image.Stretch = Stretch.UniformToFill;
			}*/
			Image.Source = image;

			if (image != null)
			{
				if (Service.Platform.AnimationsEnabled)
					Content.FadeIn();
				else
					Content.SetVisualOpacity(1);
			}
			else
				Data.MissingImage = true;
			_loading = false;
		}

		private class Container : ObservableObject
		{
			private bool _missingImage;
			public bool MissingImage
			{
				get => _missingImage;
				set
				{
					_missingImage = value;
					OnPropertyChanged("MissingImage");
				}
			}
		}

		private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e)
		{
			if (KeepOverlayOpen || HideOverlay)
				return;
			VisualStateManager.GoToState(this, "PointerOver", true);
		}

		private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
		{
			if (KeepOverlayOpen || HideOverlay)
				return;
			VisualStateManager.GoToState(this, "Normal", true);
		}

		private void UserControl_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
		{
			if (KeepOverlayOpen || HideOverlay)
				return;
			VisualStateManager.GoToState(this, "Normal", true);
		}

		public static readonly DependencyProperty KeepOverlayOpenProperty = DependencyProperty.Register("KeepOverlayOpen", typeof(bool), typeof(ArchiveImage), new PropertyMetadata(false));
		public static readonly DependencyProperty ShowExtraDetailsProperty = DependencyProperty.Register("ShowExtraDetails", typeof(bool), typeof(ArchiveImage), new PropertyMetadata(false));
		public static readonly DependencyProperty HideOverlayProperty = DependencyProperty.Register("HideOverlay", typeof(bool), typeof(ArchiveImage), new PropertyMetadata(false));
	}

}
