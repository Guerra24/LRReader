using LRReader.Models.Main;
using LRReader.ViewModels;
using LRReader.Views.Items;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LRReader.Views.Tabs.Content
{
	public sealed partial class ArchiveTabContent : UserControl
	{
		private ArchivePageViewModel Data;

		public ArchiveTabContent()
		{
			this.InitializeComponent();
			Data = new ArchivePageViewModel();
		}

		public async void LoadArchive(Archive archive)
		{
			Data.Archive = archive;
			Data.LoadTags();
			await Data.LoadImages();
		}

		private void ImagesGrid_ItemClick(object sender, ItemClickEventArgs e)
		{
			//var animation = ImagesGrid.PrepareConnectedAnimation("imageReaderForward" + e.ClickedItem as string, e.ClickedItem, "Image");
			//animation.Configuration = new DirectConnectedAnimationConfiguration();
			Data.ShowReader = true;
			FlipView.SelectedIndex = Data.ArchiveImages.IndexOf(e.ClickedItem as string);
		}

		private async void RefreshButton_Click(object sender, RoutedEventArgs e)
		{
			await Data.LoadImages();
		}

		private void FlipView_Loaded(object sender, RoutedEventArgs e)
		{
			FlipView.Focus(FocusState.Programmatic);
			// Let's remove the buttons
			var grid = (Grid)VisualTreeHelper.GetChild(FlipView, 0);
			for (int i = grid.Children.Count - 1; i >= 0; i--)
				if (grid.Children[i] is Button)
					grid.Children.RemoveAt(i);
		}

		private void FlipView_Tapped(object sender, TappedRoutedEventArgs e)
		{
			var point = e.GetPosition(FlipView);
			double distance = FlipView.ActualWidth / 6.0;
			if (point.X < distance)
			{
				if (FlipView.SelectedIndex > 0)
					--FlipView.SelectedIndex;
			}
			else if (point.X > FlipView.ActualWidth - distance)
			{
				if (FlipView.SelectedIndex < FlipView.Items.Count - 1)
					++FlipView.SelectedIndex;
			}
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			if (Data.ShowReader)
				Data.ShowReader = false;
		}
	}
}
