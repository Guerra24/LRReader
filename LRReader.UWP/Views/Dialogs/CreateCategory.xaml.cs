using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LRReader.UWP.Views.Dialogs
{
	public sealed partial class CreateCategory : ContentDialog
	{
		private ResourceLoader lang;

		public CreateCategory(bool edit)
		{
			this.InitializeComponent();
			lang = ResourceLoader.GetForCurrentView("Dialogs");
			if (edit)
				PrimaryButtonText = ResourceLoader.GetForCurrentView("Generic").GetString("Save");
		}

		private void CategoryName_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
		{
			bool allow = true;
			CategoryError.Text = "";
			if (string.IsNullOrEmpty(CategoryName.Text))
			{
				CategoryError.Text = lang.GetString("CreateCategory/ErrorName");
				allow = false;
			}
			IsPrimaryButtonEnabled = allow;
		}

	}
}
