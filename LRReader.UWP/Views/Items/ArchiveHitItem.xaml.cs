#nullable enable
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Items;
using LRReader.UWP.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace LRReader.UWP.Views.Items
{
	public sealed partial class ArchiveHitItem : UserControl
	{

		private ArchiveItemViewModel LeftViewModel, RightViewModel;

		private ArchiveHitViewModel Data;

		private ArchiveHit _old = new ArchiveHit { Left = "", Right = "" };

		public ArchiveHitItem()
		{
			this.InitializeComponent();
			// TODO: Proper fix
			LeftViewModel = Service.Services.GetRequiredService<ArchiveItemViewModel>();
			RightViewModel = Service.Services.GetRequiredService<ArchiveItemViewModel>();
			Data = Service.Services.GetRequiredService<ArchiveHitViewModel>();
		}

		public bool ShowRemove
		{
			get => (bool)GetValue(ShowRemoveProperty);
			set => SetValue(ShowRemoveProperty, value);
		}

		public ICommand RemoveCommand
		{
			get => (ICommand)GetValue(RemoveCommandProperty);
			set => SetValue(RemoveCommandProperty, value);
		}

		private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (args.NewValue == null)
				return;
			VisualStateManager.GoToState(this, "Normal", true);
			var hit = (ArchiveHit)args.NewValue;

			if (!hit.Equals(_old))
			{
				_old = hit;
				Data.ArchiveHit = hit;
				LeftViewModel.Archive = Service.Archives.GetArchive(hit.Left)!;
				RightViewModel.Archive = Service.Archives.GetArchive(hit.Right)!;

				LeftGrid.SetVisualOpacity(0);
				RightGrid.SetVisualOpacity(0);
				LeftThumbnail.Source = null;
				RightThumbnail.Source = null;
				LeftViewModel.MissingImage = RightViewModel.MissingImage = false;

				var leftImage = await Service.ImageProcessing.ByteToBitmap(await Service.Images.GetThumbnailCached(LeftViewModel.Archive?.arcid), decodeHeight: 275) as BitmapImage;

				var rightImage = await Service.ImageProcessing.ByteToBitmap(await Service.Images.GetThumbnailCached(RightViewModel.Archive?.arcid), decodeHeight: 275) as BitmapImage;
				if (leftImage != null && rightImage != null)
				{
					if (leftImage.PixelHeight != 0 && leftImage.PixelWidth != 0)
						if (Math.Abs(ActualHeight / ActualWidth - leftImage.PixelHeight / leftImage.PixelWidth) > .65)
							LeftThumbnail.Stretch = Stretch.Uniform;
					if (rightImage.PixelHeight != 0 && rightImage.PixelWidth != 0)
						if (Math.Abs(ActualHeight / ActualWidth - rightImage.PixelHeight / rightImage.PixelWidth) > .65)
							RightThumbnail.Stretch = Stretch.Uniform;
					LeftThumbnail.Source = leftImage;
					RightThumbnail.Source = rightImage;
				}
				else
					LeftViewModel.MissingImage = RightViewModel.MissingImage = true;

				if (Service.Platform.AnimationsEnabled)
				{
					LeftGrid.FadeIn();
					RightGrid.FadeIn();
				}
				else
				{
					LeftGrid.SetVisualOpacity(1);
					RightGrid.SetVisualOpacity(1);
				}
			}
		}

		private void Remove_Click(object sender, RoutedEventArgs e)
		{
			if (RemoveCommand.CanExecute(Data.ArchiveHit))
				RemoveCommand.Execute(Data.ArchiveHit);
		}

		public static readonly DependencyProperty ShowRemoveProperty = DependencyProperty.Register("ShowRemove", typeof(bool), typeof(ArchiveHitItem), new PropertyMetadata(false));
		public static readonly DependencyProperty RemoveCommandProperty = DependencyProperty.RegisterAttached("RemoveCommand", typeof(ICommand), typeof(ArchiveHitItem), new PropertyMetadata(null));

	}
}
