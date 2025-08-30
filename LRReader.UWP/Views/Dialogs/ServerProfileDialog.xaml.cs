using LRReader.Shared.Extensions;
using LRReader.Shared.Models;
using LRReader.Shared.Services;
using LRReader.UWP.Extensions;
using LRReader.UWP.Services;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Dialogs
{
	public sealed partial class ServerProfileDialog : ContentDialog, ICreateProfileDialog
	{
		private ResourceLoader lang;

		public ServerProfileDialog(bool edit)
		{
			this.InitializeComponent();
			RequestedTheme = Service.Platform.Theme.ToXamlTheme();
			lang = ResourceLoader.GetForCurrentView("Dialogs");
			if (edit)
				PrimaryButtonText = ResourceLoader.GetForCurrentView("Generic").GetString("Save");
		}

		public new string Name { get => ProfileName.Text; set => ProfileName.Text = value; }
		public string Address { get => ProfileServerAddress.Text; set => ProfileServerAddress.Text = value; }
		public string ApiKey { get => ProfileServerApiKey.Password; set => ProfileServerApiKey.Password = value; }
		public bool Integration { get => KarenIntegration.IsOn; set => KarenIntegration.IsOn = value; }

		public new async Task<IDialogResult> ShowAsync() => (IDialogResult)(int)await base.ShowAsync();

		private void ProfileName_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
		{
			bool allow = true;
			Command.Visibility = Visibility.Collapsed;
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
			Command.Visibility = Visibility.Collapsed;
			KarenStack.Visibility = Visibility.Collapsed;
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
			else if (ProfileServerAddress.Text.Contains("127.0.0.") || ProfileServerAddress.Text.Contains("localhost"))
			{
				ProfileError.Text = lang.GetString("ServerProfile/ErrorLocalHost").AsFormat("\n");
				Command.Visibility = Visibility.Visible;
				CommandBox.Text = $"CheckNetIsolation loopbackexempt -a -n=\"{((UWPlatformService)Service.Platform).GetPackageFamilyName()}\"";
				//KarenStack.Visibility = Visibility.Visible;
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

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var dataPackage = new DataPackage();
			dataPackage.RequestedOperation = DataPackageOperation.Copy;
			dataPackage.SetText($"CheckNetIsolation loopbackexempt -a -n=\"{((UWPlatformService)Service.Platform).GetPackageFamilyName()}\"");
			Clipboard.SetContent(dataPackage);
		}

	}
}
