using System;
using System.Threading.Tasks;
using LRReader.Shared.Models;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Dialogs
{
	public sealed partial class CreateTankoubon : ContentDialog, ICreateTankoubonDialog
	{
		private ResourceLoader lang;

		public CreateTankoubon()
		{
			this.InitializeComponent();
			lang = ResourceLoader.GetForCurrentView("Dialogs");
		}

		public new string Name { get => TankoubonName.Text; set => TankoubonName.Text = value; }

		public new async Task<IDialogResult> ShowAsync() => (IDialogResult)(int)await base.ShowAsync();

		private void Name_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
		{
			bool allow = true;
			Error.Text = "";
			if (string.IsNullOrEmpty(TankoubonName.Text))
			{
				Error.Text = lang.GetString("CreateTankoubon/ErrorName");
				allow = false;
			}
			IsPrimaryButtonEnabled = allow;
		}

	}
}
