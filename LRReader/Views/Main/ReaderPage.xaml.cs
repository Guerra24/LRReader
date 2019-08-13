using LRReader.Internal;
using LRReader.Models.Main;
using LRReader.ViewModels;
using LRReader.Views.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LRReader.Views.Main
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class ReaderPage : Page
	{

		private ReaderPageViewModel Data;

		public ReaderPage()
		{
			this.InitializeComponent();
			Data = DataContext as ReaderPageViewModel;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			ReaderPagePayload payload = e.Parameter as ReaderPagePayload;
			Data.Archive = payload.Archive;
			Data.LoadImages();
			FlipView.SelectedIndex = Data.ArchiveImages.IndexOf(payload.Image);
		}

		private void FlipView_Loaded(object sender, RoutedEventArgs e)
		{
			//var anim = ConnectedAnimationService.GetForCurrentView().GetAnimation("imageReaderForward");
			//var item = FlipView.ContainerFromItem(FlipView.SelectedItem) as FlipViewItem;
			//anim?.TryStart((item.ContentTemplateRoot as ReaderImage).Image);
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
	}
}
