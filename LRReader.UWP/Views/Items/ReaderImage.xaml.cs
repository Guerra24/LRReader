using LRReader.Internal;
using LRReader.Shared.Internal;
using LRReader.Shared.Models.Main;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace LRReader.UWP.Views.Items
{
	public sealed partial class ReaderImage : UserControl
	{

		private bool _fixLayout = true;

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
				return;
			}
			var animTask = ImagesRoot.Fade(value: 0.0f, duration: 80, easingMode: EasingMode.EaseOut).StartAsync();
			var n = args.NewValue as ArchiveImageSet;
			var lImage = SharedGlobal.ImagesManager.GetImageCached(n.LeftImage);
			var rImage = SharedGlobal.ImagesManager.GetImageCached(n.RightImage);
			await animTask;
			LeftImage.Source = await Util.ByteToBitmap(await lImage);
			RightImage.Source = await Util.ByteToBitmap(await rImage);
			var openLeft = ConnectedAnimationService.GetForCurrentView().GetAnimation("openL");
			var openRight = ConnectedAnimationService.GetForCurrentView().GetAnimation("openR");
			if (openLeft != null || openRight != null)
			{
				ImagesRoot.Opacity = 1;
				// UWP Image.Source is async, right now the layout hasn't updated yet
				// which causes animation fade to go black.
				// Wait around 100ms to update layout.
				if (_fixLayout)
				{
					await Task.Delay(100);
					_fixLayout = false;
				}
			}
			else
				ImagesRoot.Fade(value: 1.0f, duration: 80, easingMode: EasingMode.EaseIn).Start();
			openLeft?.TryStart(LeftImage);
			openRight?.TryStart(RightImage);
		}

	}
}
