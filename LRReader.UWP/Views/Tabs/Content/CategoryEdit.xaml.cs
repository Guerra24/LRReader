using LRReader.Shared.Models.Main;
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

namespace LRReader.UWP.Views.Tabs.Content
{
	public sealed partial class CategoryEdit : UserControl
	{

		private CategoryEditViewModel ViewModel;

		public CategoryEdit()
		{
			this.InitializeComponent();
			ViewModel = new CategoryEditViewModel();
		}

		public void LoadCategory(Category category)
		{
			ViewModel.LoadCategory(category);
		}

		private void CategoryName_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
		{
			bool allow = true;
			CategoryError.Text = "";
			if (string.IsNullOrEmpty(sender.Text))
			{
				CategoryError.Text = "Empty Category Name";
				allow = false;
			}
			ViewModel.CanSave = allow;
		}

		private async void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			await ViewModel.SaveCategory();
		}
	}
}
