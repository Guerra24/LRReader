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
		}
	}
}
