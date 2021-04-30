using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace LRReader.Avalonia.Views.Items
{
	public class ArchiveItem : UserControl
	{

		private ArchiveItemViewModel ViewModel { get; }

		private string _oldID = "";

		private bool _open;

		private ResourceLoader lang;

		private ControlFlags flags;

		public ArchiveItem()
		{
			InitializeComponent();
			// TODO: Proper fix
			ViewModel = Service.Services.GetRequiredService<ArchiveItemViewModel>();
			lang = ResourceLoader.GetForCurrentView("Dialogs");
			flags = Service.Api.ControlFlags;
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		private async void ArchiveItem_DataContextChanged(object sender, EventArgs e)
		{
			if (DataContext == null)
				return;
			if (DataContext is Archive archive)
			{
				ViewModel.Archive = archive;
				DataContext = ViewModel;
				return;
			}
			if (!_oldID.Equals(ViewModel.Archive.arcid))
			{
				/*Overlay.SetVisualOpacity(0);
				Title.SetVisualOpacity(0);
				TagsGrid.SetVisualOpacity(0);
				Thumbnail.Source = null;
				ViewModel.MissingImage = false;

				var image = new BitmapImage();
				image.DecodePixelType = DecodePixelType.Logical;
				image.DecodePixelHeight = 275;
				image = await Global.ImageProcessing.ByteToBitmap(await Service.Images.GetThumbnailCached(ViewModel.Archive.arcid), image);

				if (image == null)
					ViewModel.MissingImage = true;
				else
				{
					if (image.PixelHeight != 0 && image.PixelWidth != 0)
						if (Math.Abs(ActualHeight / ActualWidth - image.PixelHeight / image.PixelWidth) > .65)
							Thumbnail.Stretch = Stretch.Uniform;
					Thumbnail.Source = image;
				}

				Overlay.FadeIn();
				Title.FadeIn();
				TagsGrid.FadeIn();*/

				var Thumbnail = this.FindControl<Image>("Thumbnail");
				var bytes = await Service.Images.GetThumbnailCached(ViewModel.Archive.arcid);
				if (bytes == null)
					ViewModel.MissingImage = true;
				else
					using (var stream = new MemoryStream(bytes))
					{
						var bitmap = new Bitmap(stream);
						if (bitmap.Size.Width != 0 && bitmap.Size.Height != 0)
							if (Math.Abs(this.Height / this.Width - bitmap.Size.Height / bitmap.Size.Width) > .65)
								Thumbnail.Stretch = Stretch.Uniform;
						Thumbnail.Source = bitmap;
					}

				_oldID = ViewModel.Archive.arcid;
			}
		}

	}
}
