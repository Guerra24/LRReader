using LRReader.Internal;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Animation;
using LRReader.Shared.Models.Main;
using GalaSoft.MvvmLight.Threading;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LRReader.UWP.Views.Items
{
	public sealed partial class ReaderImage : UserControl
	{

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
			var lImage = Global.ImageManager.DownloadImage(n.LeftImage);
			var rImage = Global.ImageManager.DownloadImage(n.RightImage);
			await animTask;
			LeftImage.Source = await lImage;
			RightImage.Source = await rImage;
			ImagesRoot.Fade(value: 1.0f, duration: 80, easingMode: EasingMode.EaseIn).Start();
		}

	}
}
