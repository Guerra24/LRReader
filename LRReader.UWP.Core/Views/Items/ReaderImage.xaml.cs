using LRReader.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
			LeftImage.Source = null;
			RightImage.Source = null;
			if (args.NewValue == null)
				return;
			ArchiveImageSet n = args.NewValue as ArchiveImageSet;
			var lImage = await Global.ImageManager.DownloadImage(n.LeftImage);
			var rImage = await Global.ImageManager.DownloadImage(n.RightImage);
			LeftImage.Source = lImage;
			RightImage.Source = rImage;
		}

	}
}
