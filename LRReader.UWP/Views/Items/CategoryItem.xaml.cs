﻿using LRReader.Internal;
using LRReader.Shared.Extensions;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Base;
using LRReader.UWP.Extensions;
using LRReader.UWP.Views.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace LRReader.UWP.Views.Items
{
	public sealed partial class CategoryItem : UserControl
	{
		private CategoryBaseViewModel ViewModel;

		private string _oldID = "";

		private ResourceLoader lang;

		public CategoryItem()
		{
			this.InitializeComponent();
			// TODO: Proper fix
			ViewModel = Service.Services.GetRequiredService<CategoryBaseViewModel>();
			lang = ResourceLoader.GetForCurrentView("Dialogs");
		}

		private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (args.NewValue == null)
				return;
			ViewModel.Category = args.NewValue as Category;

			if (!_oldID.Equals(ViewModel.Category.id))
			{
				Overlay.SetVisualOpacity(0);
				Title.SetVisualOpacity(0);
				Thumbnail.Source = null;
				ViewModel.MissingImage = false;

				var first = ViewModel.Category.archives.FirstOrDefault();
				if (first != null)
				{
					var image = new BitmapImage();
					image.DecodePixelType = DecodePixelType.Logical;
					image.DecodePixelHeight = 275;
					image = await Service.ImageProcessing.ByteToBitmap(await Service.Images.GetThumbnailCached(first), image) as BitmapImage;
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

				if (Service.Platform.AnimationsEnabled)
				{
					Overlay.FadeIn();
					Title.FadeIn();
				}
				else
				{
					Overlay.SetVisualOpacity(1);
					Title.SetVisualOpacity(1);
				}
				_oldID = ViewModel.Category.id;
			}
		}

		private void Add_Click(object sender, RoutedEventArgs e)
		{
			Service.Tabs.OpenTab(Tab.SearchResults, false, ViewModel.Category);
		}

		private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
		{
			var pointerPoint = e.GetCurrentPoint(this);
			if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
			{
				if (pointerPoint.Properties.IsMiddleButtonPressed)
				{
					Service.Tabs.OpenTab(Tab.SearchResults, false, ViewModel.Category);
				}
			}
		}

		private async void Edit_Click(object sender, RoutedEventArgs e)
		{
			var listMode = string.IsNullOrEmpty(ViewModel.Category.search);

			if (ViewModel.Category.Unconfigured())
			{
				var dialog = new ContentDialog
				{
					Title = lang.GetString("ConfigureCategory/Title"),
					PrimaryButtonText = lang.GetString("ConfigureCategory/PrimaryButtonText"),
					SecondaryButtonText = lang.GetString("ConfigureCategory/SecondaryButtonText"),
					CloseButtonText = lang.GetString("ConfigureCategory/CloseButtonText"),
					Content = lang.GetString("ConfigureCategory/Content").AsFormat("\n")
				};
				var result = await dialog.ShowAsync();

				switch (result)
				{
					case ContentDialogResult.None:
						return;
					case ContentDialogResult.Primary:
						listMode = true;
						break;
					case ContentDialogResult.Secondary:
						listMode = false;
						break;
				}
			}

			if (listMode)
				Service.Tabs.OpenTab(Tab.CategoryEdit, ViewModel.Category);
			else
			{
				var dialog = new CreateCategory(true);
				dialog.CategoryName.Text = ViewModel.Category.name;
				dialog.SearchQuery.Text = ViewModel.Category.search;
				dialog.Pinned.IsOn = ViewModel.Category.pinned;
				var result = await dialog.ShowAsync();
				if (result == ContentDialogResult.Primary)
				{
					await ViewModel.UpdateCategory(dialog.CategoryName.Text, dialog.SearchQuery.Text, dialog.Pinned.IsOn);
				}
			}
		}

		private async void Remove_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new ContentDialog()
			{
				Title = lang.GetString("RemoveCategory/Title").AsFormat(ViewModel.Category.name),
				Content = lang.GetString("RemoveCategory/Content"),
				PrimaryButtonText = lang.GetString("RemoveCategory/PrimaryButtonText"),
				CloseButtonText = lang.GetString("RemoveCategory/CloseButtonText")
			};
			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				if (ViewModel.Category.DeleteCategory != null)
					await ViewModel.Category.DeleteCategory(ViewModel.Category);
			}
		}
	}
}
