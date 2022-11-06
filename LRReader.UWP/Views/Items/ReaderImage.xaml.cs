#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.UWP.Extensions;
using Microsoft.Toolkit.Uwp.UI.Animations;
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
		private int _height, _width;
		private ImageProcessingService imageProcessing = Service.ImageProcessing;

		private SemaphoreSlim decodePixel = new SemaphoreSlim(1);

		public ReaderImage()
		{
			this.InitializeComponent();
		}

		public async Task ChangePage(ReaderImageSet set)
		{
			await decodePixel.WaitAsync();
			var lImage = Service.Images.GetImageCached(set.LeftImage);
			var rImage = Service.Images.GetImageCached(set.RightImage);
			LeftImage.Source = await imageProcessing.ByteToBitmap(await lImage, _width, _height) as BitmapImage;
			RightImage.Source = await imageProcessing.ByteToBitmap(await rImage, _width, _height) as BitmapImage;
			var lSize = await Service.Images.GetImageSizeCached(set.LeftImage);
			var rSize = await Service.Images.GetImageSizeCached(set.RightImage);
			var size = new Size(Math.Max(lSize.Width, rSize.Width), Math.Max(lSize.Height, rSize.Height));
			LeftImage.Height = RightImage.Height = 0;
			if (LeftImage.Source != null)
			{
				//LeftImage.Width = size.Width;
				LeftImage.Height = set.Height == 0 ? size.Height : set.Height;
			}
			if (RightImage.Source != null)
			{
				//RightImage.Width = size.Width;
				RightImage.Height = size.Height;
			}
			decodePixel.Release();
		}

		public async Task FadeOutPage()
		{
			if (!Service.Platform.AnimationsEnabled)
				return;
			if (disableAnimation)
			{
				ImagesRoot.SetVisualOpacity(0);
				disableAnimation = false;
			}
			else
			{
				await FadeOut.StartAsync(ImagesRoot);
			}
		}

		public void FadeInPage()
		{
			if (!Service.Platform.AnimationsEnabled)
				return;
			var openLeft = ConnectedAnimationService.GetForCurrentView().GetAnimation("openL");
			var openRight = ConnectedAnimationService.GetForCurrentView().GetAnimation("openR");
			if (openLeft != null || openRight != null)
				ImagesRoot.SetVisualOpacity(1);
			else
				FadeIn.Start(ImagesRoot);
			openLeft?.TryStart(LeftImage);
			openRight?.TryStart(RightImage);
		}

		public async Task ResizeHeight(int height)
		{
			if (_height == height)
				return;
			await decodePixel.WaitAsync();
			_height = height;
			if (LeftImage.Source != null)
				((BitmapImage)LeftImage.Source).DecodePixelHeight = height;
			if (RightImage.Source != null)
				((BitmapImage)RightImage.Source).DecodePixelHeight = height;
			decodePixel.Release();
		}

		public async Task ResizeWidth(int width)
		{
			if (_width == width)
				return;
			await decodePixel.WaitAsync();
			_width = width;
			if (LeftImage.Source != null)
				((BitmapImage)LeftImage.Source).DecodePixelWidth = width;
			if (RightImage.Source != null)
				((BitmapImage)RightImage.Source).DecodePixelWidth = width;
			decodePixel.Release();
		}

		private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (args.NewValue is ReaderImageSet set)
				await ChangePage(set);
		}
	}
}
