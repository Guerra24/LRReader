using LRReader.Internal;
using LRReader.Shared.Models.Main;
using LRReader.ViewModels.Items;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LRReader.Views.Items
{
	public sealed partial class BookmarkedArchive : UserControl
	{
		private ArchiveItemViewModel ViewModel;

		private string _oldID = "";

		public BookmarkedArchive()
		{
			this.InitializeComponent();
			ViewModel = new ArchiveItemViewModel();
		}

		private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (args.NewValue == null)
				return;
			ViewModel.Archive = args.NewValue as Archive;

			if (!_oldID.Equals(ViewModel.Archive.arcid))
			{
				Title.Opacity = 0;
				Thumbnail.Source = null;
				Thumbnail.Visibility = Visibility.Collapsed;
				using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
				{
					byte[] bytes = await Global.ImageManager.DownloadThumbnailRuntime(ViewModel.Archive.arcid);
					await stream.WriteAsync(bytes.AsBuffer());
					stream.Seek(0);
					var image = new BitmapImage();
					await image.SetSourceAsync(stream);
					Thumbnail.Source = image;
				}
				Thumbnail.Visibility = Visibility.Visible;
				Title.Opacity = 1;
				_oldID = ViewModel.Archive.arcid;
			}
		}
	}
}
