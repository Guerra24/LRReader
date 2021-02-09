using GalaSoft.MvvmLight;
using LRReader.UWP.Internal;
using LRReader.Shared.Internal;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using LRReader.Internal;

namespace LRReader.UWP.Views.Items
{
	public sealed partial class ArchiveImage : UserControl
	{
		private string _oldUrl = "";
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
			string n = args.NewValue as string;
			if (!_oldUrl.Equals(n))
			{
				await ReloadImage(n);
				_oldUrl = n;
			}
		}

		private async void Reload_Click(object sender, RoutedEventArgs e) => await ReloadImage(_oldUrl);

		private async Task ReloadImage(string n)
		{
			if (_loading)
				return;
			_loading = true;
			Image.Opacity = 0;
			Image.Source = null;
			Ring.IsActive = true;
			Data.MissingImage = false;

			var image = new BitmapImage();
			image.DecodePixelType = DecodePixelType.Logical;
			image.DecodePixelHeight = 275;
			image = await Global.ImageProcessing.ByteToBitmap(await SharedGlobal.ImagesManager.GetImageCached(n), image, n.EndsWith("avif"));
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

	}

}
