using LRReader.UWP.ViewModels;
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
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LRReader.UWP.Views.Tabs.Content
{
	public sealed partial class Categories : UserControl
	{
		private CategoriesViewModel Data;

		private bool loaded;

		public Categories()
		{
			this.InitializeComponent();
			Data = DataContext as CategoriesViewModel;
		}

		private async void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (loaded)
				return;
			loaded = true;
			await Data.Refresh();
		}
		private void CategoriesGrid_ItemClick(object sender, ItemClickEventArgs e)
		{

		}

		private async void Button_Click(object sender, RoutedEventArgs e) => await Data.Refresh();

		private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
		{
			using (var deferral = args.GetDeferral())
			{
				await Data.Refresh();
			}
		}

		private async void Refresh_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) => await Data.Refresh();

		public async void Refresh() => await Data.Refresh();
	}
}
