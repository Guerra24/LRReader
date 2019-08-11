using LRReader.Models.Main;
using LRReader.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LRReader.Views.Main
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class ArchivesPage : Page
	{

		private static string _selectedID = "";

		private ArchivesPageViewModel Data;

		public ArchivesPage()
		{
			this.InitializeComponent();
			Data = DataContext as ArchivesPageViewModel;
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			await Data.Refresh();
			if (!string.IsNullOrEmpty(_selectedID))
				ArchivesGrid.ScrollIntoView(Data.ArchiveList.FirstOrDefault(a => a.arcid.Equals(_selectedID)));
		}

		private void ArchivesGrid_ItemClick(object sender, ItemClickEventArgs e)
		{
			_selectedID = (e.ClickedItem as Archive).arcid;
			Frame.Navigate(typeof(ArchivePage), e.ClickedItem, new DrillInNavigationTransitionInfo());
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			await Data.Refresh();
		}
	}
}
