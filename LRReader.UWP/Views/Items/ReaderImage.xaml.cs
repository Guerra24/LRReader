using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.UWP.Extensions;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

namespace LRReader.UWP.Views.Items
{
	public sealed partial class ReaderImage : UserControl
	{

		private static AnimationBuilder FadeIn = AnimationBuilder.Create().Opacity(to: 1, duration: TimeSpan.FromMilliseconds(80), easingMode: EasingMode.EaseIn);
		private static AnimationBuilder FadeOut = AnimationBuilder.Create().Opacity(to: 0, duration: TimeSpan.FromMilliseconds(80), easingMode: EasingMode.EaseOut);

		public bool disableAnimation = true;
		private string _current = "";
		private int _height;
		private ImageProcessingService imageProcessing = Service.ImageProcessing;

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
			var animate = Service.Platform.AnimationsEnabled;
			var n = args.NewValue as ReaderImageSet;
			/*if (_current.Equals(n.LeftImage + n.RightImage))
			{
				Animate();
				return;
			}*/
			_current = n.LeftImage + n.RightImage;
			Task animTask = null;
			if (animate)
				if (disableAnimation)
				{
					ImagesRoot.SetVisualOpacity(0);
					disableAnimation = false;
				}
				else
				{
					animTask = FadeOut.StartAsync(ImagesRoot);
				}
			var lImage = Service.Images.GetImageCached(n.LeftImage);
			var rImage = Service.Images.GetImageCached(n.RightImage);
			var imageL = new BitmapImage();
			var imageR = new BitmapImage();
			imageR.DecodePixelType = imageL.DecodePixelType = DecodePixelType.Logical;
			if (_height != 0)
				imageR.DecodePixelHeight = imageL.DecodePixelHeight = _height;
			if (animTask != null)
				await animTask;
			LeftImage.Source = await imageProcessing.ByteToBitmap(await lImage, imageL) as BitmapImage;
			RightImage.Source = await imageProcessing.ByteToBitmap(await rImage, imageR) as BitmapImage;
			var lSize = await Service.Images.GetImageSizeCached(n.LeftImage);
			var rSize = await Service.Images.GetImageSizeCached(n.RightImage);
			var size = new Size(Math.Max(lSize.Width, rSize.Width), Math.Max(lSize.Height, rSize.Height));
			LeftImage.Width = LeftImage.Height = RightImage.Width = RightImage.Height = 0;
			if (LeftImage.Source != null)
			{
				LeftImage.Width = size.Width;
				LeftImage.Height = size.Height;
			}
			if (RightImage.Source != null)
			{
				RightImage.Width = size.Width;
				RightImage.Height = size.Height;
			}
			if (animate)
				Animate();
		}

		private void Animate()
		{
			var openLeft = ConnectedAnimationService.GetForCurrentView().GetAnimation("openL");
			var openRight = ConnectedAnimationService.GetForCurrentView().GetAnimation("openR");
			if (openLeft != null || openRight != null)
			{
				ImagesRoot.SetVisualOpacity(1);
				// UWP Image.Source is async, right now the layout hasn't updated yet
				// which causes animation fade to go black.
				// Wait around 100ms to update layout.
				//await Task.Delay(100);
				/*if (_fixLayout)
				{
					_fixLayout = false;
				}*/
			}
			else
				FadeIn.Start(ImagesRoot);
			openLeft?.TryStart(LeftImage);
			openRight?.TryStart(RightImage);
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
