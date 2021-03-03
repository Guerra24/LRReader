using GalaSoft.MvvmLight;
using LRReader.Internal;
using LRReader.Shared.Internal;
using LRReader.Shared.Models.Main;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

namespace LRReader.UWP.Views.Items
{
	public sealed partial class ArchiveImage : UserControl
	{
		private ImagePageSet _oldUrl = new ImagePageSet("", 0);
		private Container Data = new Container();
		private bool _loading;

		public ArchiveImage()
		{
			this.InitializeComponent();
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

		private async void Reload_Click(object sender, RoutedEventArgs e) => await ReloadImage(_oldUrl);

		private async Task ReloadImage(ImagePageSet n)
		{
			if (_loading)
				return;
			_loading = true;
			Image.Opacity = 0;
			Image.Source = null;
			Ring.IsActive = true;
			Data.MissingImage = false;
			PageCount.Text = n.Page.ToString();

			var image = new BitmapImage();
			image.DecodePixelType = DecodePixelType.Logical;
			image.DecodePixelHeight = 275;
			image = await Global.ImageProcessing.ByteToBitmap(await SharedGlobal.ImagesManager.GetImageCached(n.Image), image, n.Image.EndsWith("avif"));
			Ring.IsActive = false;
			Image.Source = image;

			if (image != null)
				Image.Fade(value: 1.0f, duration: 250, easingMode: EasingMode.EaseIn).Start();
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
					RaisePropertyChanged("MissingImage");
				}
			}
		}

		private void UserControl_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) => PageCountGrid.Margin = new Thickness(0);

		private void UserControl_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) => PageCountGrid.Margin = new Thickness(0, 0, 0, -26);
	}

}
