using LRReader.Internal;
using LRReader.Models.Main;
using LRReader.ViewModels;
using LRReader.Views.Items;
using System;
using System.Collections.Generic;
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
	public sealed partial class ArchivePage : Page
	{

		private ArchivePageViewModel Data;

		public ArchivePage()
		{
			this.InitializeComponent();
			Data = DataContext as ArchivePageViewModel;
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			Data.Archive = e.Parameter as Archive;
			Data.LoadTags();
			await Data.LoadImages();
		}

		private void ImagesGrid_ItemClick(object sender, ItemClickEventArgs e)
		{
			//var animation = ImagesGrid.PrepareConnectedAnimation("imageReaderForward", e.ClickedItem, "Image");
			//animation.Configuration = new DirectConnectedAnimationConfiguration();
			Frame.Navigate(typeof(ReaderPage), new ReaderPagePayload() { Archive = Data.Archive, Image = e.ClickedItem as string }, new DrillInNavigationTransitionInfo()); //new SuppressNavigationTransitionInfo());
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			await Data.LoadImages();
		}
	}
}
