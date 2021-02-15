using LRReader.UWP.Internal;
using LRReader.Shared.Internal;
using LRReader.Shared.Models.Main;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using LRReader.Internal;

namespace LRReader.UWP.Views.Items
{
	public sealed partial class ReaderImage : UserControl
	{

		//private bool _fixLayout = true;
		private string _current = "";
		private int _height;

		public ReaderImage()
		{
			this.InitializeComponent();
		}

		private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (args.NewValue == null)
			{
				LeftImage.Source = null;
				RightImage.Source = null;
				_current = "";
				return;
			}
			var n = args.NewValue as ArchiveImageSet;
			if (_current.Equals(n.LeftImage + n.RightImage))
			{
				Animate();
				return;
			}
			_current = n.LeftImage + n.RightImage;
			var animTask = ImagesRoot.Fade(value: 0.0f, duration: 80, easingMode: EasingMode.EaseOut).StartAsync();
			var lImage = SharedGlobal.ImagesManager.GetImageCached(n.LeftImage);
			var rImage = SharedGlobal.ImagesManager.GetImageCached(n.RightImage);
			await animTask;
			var imageL = new BitmapImage();
			var imageR = new BitmapImage();
			imageR.DecodePixelType = imageL.DecodePixelType = DecodePixelType.Logical;
			if (_height != 0)
				imageR.DecodePixelHeight = imageL.DecodePixelHeight = _height;
			LeftImage.Source = await Global.ImageProcessing.ByteToBitmap(await lImage, imageL);
			RightImage.Source = await Global.ImageProcessing.ByteToBitmap(await rImage, imageR);
			Animate();
		}

		private void Animate()
		{
			/*var openLeft = ConnectedAnimationService.GetForCurrentView().GetAnimation("openL");
			var openRight = ConnectedAnimationService.GetForCurrentView().GetAnimation("openR");
			if (openLeft != null || openRight != null)
			{
				ImagesRoot.Opacity = 1;
				// UWP Image.Source is async, right now the layout hasn't updated yet
				// which causes animation fade to go black.
				// Wait around 100ms to update layout.
				await Task.Delay(100);
				if (_fixLayout)
				{
					_fixLayout = false;
				}
			}
			else*/
				ImagesRoot.Fade(value: 1.0f, duration: 80, easingMode: EasingMode.EaseIn).Start();
			//openLeft?.TryStart(LeftImage);
			//openRight?.TryStart(RightImage);
		}

		public void UpdateDecodedResolution(int height)
		{
			if (_height == height)
				return;
			_height = height;
			if (LeftImage.Source != null)
				(LeftImage.Source as BitmapImage).DecodePixelHeight = height;
			if (RightImage.Source != null)
				(RightImage.Source as BitmapImage).DecodePixelHeight = height;
		}

	}
}
