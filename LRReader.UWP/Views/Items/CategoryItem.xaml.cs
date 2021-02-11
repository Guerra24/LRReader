using LRReader.Internal;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.UWP.ViewModels.Base;
using LRReader.UWP.Views.Tabs;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace LRReader.UWP.Views.Items
{
	public sealed partial class CategoryItem : UserControl
	{
		private CategoryBaseViewModel ViewModel;

		private string _oldID = "";

		public CategoryItem()
		{
			this.InitializeComponent();
			ViewModel = new CategoryBaseViewModel();
		}

		private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (args.NewValue == null)
				return;
			ViewModel.Category = args.NewValue as Category;

			if (!_oldID.Equals(ViewModel.Category.id))
			{
				Overlay.Opacity = 0;
				Title.Opacity = 0;
				Thumbnail.Source = null;
				Ring.IsActive = true;
				ViewModel.MissingImage = false;

				var first = ViewModel.Category.archives.FirstOrDefault();
				if (first != null)
				{
					var image = new BitmapImage();
					image.DecodePixelType = DecodePixelType.Logical;
					image.DecodePixelHeight = 275;
					image = await Global.ImageProcessing.ByteToBitmap(await ArchivesProvider.GetThumbnail(first), image);
					if (image == null)
						ViewModel.MissingImage = true;
					else
					{
						if (image.PixelHeight != 0 && image.PixelWidth != 0)
							if (Math.Abs(ActualHeight / ActualWidth - image.PixelHeight / image.PixelWidth) > .65)
								Thumbnail.Stretch = Stretch.Uniform;
						Thumbnail.Source = image;
					}
				}
				else
				{
					ViewModel.SearchImage = true;
				}

				Ring.IsActive = false;
				Overlay.Fade(value: 1.0f, duration: 250, easingMode: EasingMode.EaseIn).Start();
				Title.Fade(value: 1.0f, duration: 250, easingMode: EasingMode.EaseIn).Start();
				_oldID = ViewModel.Category.id;
			}
		}

		private void Add_Click(object sender, RoutedEventArgs e)
		{
			Global.EventManager.AddTab(new SearchResultsTab(ViewModel.Category), false);
		}

		private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
		{
			var pointerPoint = e.GetCurrentPoint(this);
			if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
			{
				if (pointerPoint.Properties.IsMiddleButtonPressed)
				{
					Global.EventManager.AddTab(new SearchResultsTab(ViewModel.Category), false);
				}
			}
		}

		private void Edit_Click(object sender, RoutedEventArgs e)
		{
			Global.EventManager.AddTab(new CategoryEditTab(ViewModel.Category));
		}

		private async void Remove_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new ContentDialog()
			{
				Title = "Remove Category: " + ViewModel.Category.name,
				Content = "Are you sure you want to remove it?",
				PrimaryButtonText = "Yes",
				CloseButtonText = "No"
			};
			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				await ViewModel.Category.DeleteCategory?.Invoke(ViewModel.Category);
			}
		}
	}
}
