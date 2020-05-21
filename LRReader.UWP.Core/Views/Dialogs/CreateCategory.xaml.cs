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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LRReader.UWP.Views.Dialogs
{
	public sealed partial class CreateCategory : ContentDialog
	{
		public CreateCategory(bool edit)
		{
			this.InitializeComponent();
			if (edit)
				PrimaryButtonText = "Save";
		}

		private void CategoryName_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
		{
			bool allow = true;
			CategoryError.Text = "";
			if (string.IsNullOrEmpty(CategoryName.Text))
			{
				CategoryError.Text = "Empty Category Name";
				allow = false;
			}
			IsPrimaryButtonEnabled = allow;
		}

	}
}
