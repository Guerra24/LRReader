using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Dialogs
{
	public sealed partial class ServerProfileDialog : ContentDialog
	{
		private ResourceLoader lang;

		public ServerProfileDialog(bool edit)
		{
			this.InitializeComponent();
			lang = ResourceLoader.GetForCurrentView("Dialogs");
			if (edit)
				PrimaryButtonText = ResourceLoader.GetForCurrentView("Generic").GetString("Save");
		}

		private void ProfileName_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
		{
			bool allow = true;
			ProfileError.Text = "";
			if (string.IsNullOrEmpty(ProfileName.Text))
			{
				ProfileError.Text = lang.GetString("ServerProfile/ErrorNoName");
				allow = false;
			}
			IsPrimaryButtonEnabled = allow && ValidateServerAddress();
		}

		private void ProfileServerAddress_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
		{
			bool allow = true;
			ProfileError.Text = "";
			if (string.IsNullOrEmpty(ProfileServerAddress.Text))
			{
				ProfileError.Text = lang.GetString("ServerProfile/ErrorNoAddress");
				allow = false;
			}
			if (!Uri.IsWellFormedUriString(ProfileServerAddress.Text, UriKind.Absolute))
			{
				ProfileError.Text = lang.GetString("ServerProfile/ErrorInvalidAddress");
				allow = false;
			}
			IsPrimaryButtonEnabled = allow && ValidateProfileName();
		}

		private bool ValidateProfileName()
		{
			return !string.IsNullOrEmpty(ProfileName.Text);
		}

		private bool ValidateServerAddress()
		{
			return !string.IsNullOrEmpty(ProfileServerAddress.Text) && Uri.IsWellFormedUriString(ProfileServerAddress.Text, UriKind.Absolute);
		}
	}
}
