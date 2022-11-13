#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using LRReader.Shared;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Items;
using LRReader.UWP.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Windows.Devices.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using ParallaxView = Microsoft.UI.Xaml.Controls.ParallaxView;

namespace LRReader.UWP.Views.Items
{
	public sealed class GenericArchiveItem : Control
	{
		private static AnimationBuilder FadeIn = AnimationBuilder.Create().Opacity(to: 1, duration: TimeSpan.FromMilliseconds(150), easingMode: EasingMode.EaseIn);

		public ArchiveItemViewModel ViewModel { get; }

		private bool _open;

		// Default
		private Grid Root = null!;
		private Image Thumbnail = null!;
		private Grid TagsGrid = null!;
		private Flyout TagsFlyout = null!;

		// Bookmark
		public ParallaxView? Parallax;

		// Internal
		public IList<Archive> Group = null!;

		public GenericArchiveItem()
		{
			this.DefaultStyleKey = typeof(GenericArchiveItem);
			// TODO: Proper fix
			ViewModel = Service.Services.GetRequiredService<ArchiveItemViewModel>();
			ViewModel.Show += Show;
			ViewModel.Hide += Hide;
			DataContextChanged += Control_DataContextChanged;
			PointerPressed += Control_PointerPressed;
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			Root = (Grid)GetTemplateChild("Root");
			Thumbnail = (Image)GetTemplateChild("Thumbnail");
			TagsGrid = (Grid)GetTemplateChild("TagsGrid");
			TagsFlyout = (Flyout)GetTemplateChild("TagsFlyout");

			Parallax = GetTemplateChild("Parallax") as ParallaxView;
		}

		public int DecodePixelWidth
		{
			get => (int)GetValue(DecodePixelWidthProperty);
			set => SetValue(DecodePixelWidthProperty, value);
		}

		public int DecodePixelHeight
		{
			get => (int)GetValue(DecodePixelHeightProperty);
			set => SetValue(DecodePixelHeightProperty, value);
		}

		private Task Show(bool animate)
		{
			if (animate)
				Root.Start(FadeIn);
			else
				Root.SetVisualOpacity(1);
			return Task.CompletedTask;
		}

		private Task Hide(bool animate)
		{
			Root.SetVisualOpacity(0);
			return Task.CompletedTask;
		}

		private async void Control_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (args.NewValue == null)
				return;
			await ViewModel.Load((Archive)args.NewValue, DecodePixelWidth, DecodePixelHeight);
		}

		public void MenuFlyoutItem_Click(object sender, RoutedEventArgs e) => Service.Archives.OpenTab(ViewModel.Archive, false, Group);

		public async void DownloadMenuItem_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.Downloading = true;
			var download = await ViewModel.DownloadArchive();
			if (download == null)
			{
				ViewModel.Downloading = false;
				return;
			}

			var savePicker = new FileSavePicker();
			savePicker.SuggestedStartLocation = PickerLocationId.Downloads;
			savePicker.FileTypeChoices.Add(download.Type + " File", new List<string>() { download.Type });
			savePicker.SuggestedFileName = download.Name;

			StorageFile file = await savePicker.PickSaveFileAsync();
			ViewModel.Downloading = false;
			if (file != null)
			{
				CachedFileManager.DeferUpdates(file);
				await FileIO.WriteBytesAsync(file, download.Data);
				FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
				if (status == FileUpdateStatus.Complete)
				{
					//save
				}
				else
				{
					// not saved
				}
			}
			else
			{
				//cancel
			}
		}

		public void Add_Click(object sender, RoutedEventArgs e) => ViewModel.Bookmarked = true;

		public void Remove_Click(object sender, RoutedEventArgs e) => ViewModel.Bookmarked = false;

		public async void TagsGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
		{
			_open = true;
			await Task.Delay(TimeSpan.FromMilliseconds(Service.Platform.HoverTime));
			if (_open && !TagsFlyout.IsOpen)
			{
				_open = false;
				var placement = (FlyoutPlacementMode)typeof(TagsPopupLocation).GetMember(Service.Settings.TagsPopup.ToString())[0].GetCustomAttribute<IntAttribute>(false).Value;
				TagsFlyout.ShowAt(TagsGrid, new FlyoutShowOptions
				{
					Position = e.GetCurrentPoint(TagsGrid).Position,
					Placement = placement,
					ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway
				});
			}
		}

		public void TagsGrid_PointerExited(object sender, PointerRoutedEventArgs e)
		{
			if (_open)
			{
				_open = false;
			}
		}

		private void Control_PointerPressed(object sender, PointerRoutedEventArgs e)
		{
			var pointerPoint = e.GetCurrentPoint(this);
			if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
			{
				if (pointerPoint.Properties.IsMiddleButtonPressed)
				{
					Service.Archives.OpenTab(ViewModel.Archive, false, Group);
					e.Handled = true;
				}
			}
		}

		public static readonly DependencyProperty DecodePixelWidthProperty = DependencyProperty.Register("DecodePixelWidth", typeof(int), typeof(GenericArchiveItem), new PropertyMetadata(0));
		public static readonly DependencyProperty DecodePixelHeightProperty = DependencyProperty.Register("DecodePixelHeight", typeof(int), typeof(GenericArchiveItem), new PropertyMetadata(0));
	}
}
