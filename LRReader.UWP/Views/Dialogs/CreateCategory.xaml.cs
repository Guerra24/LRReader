using System;
using System.Threading.Tasks;
using LRReader.Shared.Models;
using LRReader.Shared.Services;
using LRReader.UWP.Extensions;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Dialogs
{
	public sealed partial class CreateCategory : ContentDialog, ICreateCategoryDialog
	{
		private ResourceLoader lang;

		public CreateCategory(bool edit)
		{
			this.InitializeComponent();
			RequestedTheme = Service.Platform.Theme.ToXamlTheme();
			lang = ResourceLoader.GetForCurrentView("Dialogs");
			if (edit)
				PrimaryButtonText = ResourceLoader.GetForCurrentView("Generic").GetString("Save");
		}

		public new string Name { get => CategoryName.Text; set => CategoryName.Text = value; }
		public string Query { get => SearchQuery.Text; set => SearchQuery.Text = value; }
		public bool Pin { get => Pinned.IsOn; set => Pinned.IsOn = value; }

		public new async Task<IDialogResult> ShowAsync() => (IDialogResult)(int)await base.ShowAsync();

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
