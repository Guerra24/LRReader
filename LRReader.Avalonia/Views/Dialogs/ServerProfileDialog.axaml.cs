using FluentAvalonia.UI.Controls;
using LRReader.Avalonia.Resources;
using LRReader.Shared.Models;

namespace LRReader.Avalonia.Views.Dialogs
{
	public partial class ServerProfileDialog : FAContentDialog, ICreateProfileDialog
	{

		protected override Type StyleKeyOverride => typeof(FAContentDialog);

		private ResourceLoader lang;

		public ServerProfileDialog() : base()
		{
			InitializeComponent();
			lang = ResourceLoader.GetForCurrentView("Dialogs");
			//ProfileName.AddHandler(TextInputEvent, ProfileName_TextChanging, RoutingStrategies.Tunnel);
			//ProfileServerAddress.AddHandler(TextInputEvent, ProfileServerAddress_TextChanging, RoutingStrategies.Tunnel);
		}

		public ServerProfileDialog(bool edit) : this()
		{
			if (edit)
				PrimaryButtonText = ResourceLoader.GetForCurrentView("Generic").GetString("Save");
		}

		public new string Name { get => ProfileName.Text!; set => ProfileName.Text = value; }
		public string Address { get => ProfileServerAddress.Text!; set => ProfileServerAddress.Text = value; }
		public string ApiKey { get => ProfileServerApiKey.Text!; set => ProfileServerApiKey.Text = value; }
		public bool Integration { get; set; } // TODO

		public async Task<IDialogResult> ShowAsync(object root) => (IDialogResult)(int)await base.ShowAsync((TopLevel)root);

		private void ProfileName_TextChanging(object? sender, TextChangingEventArgs e)
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

		private void ProfileServerAddress_TextChanging(object? sender, TextChangingEventArgs e)
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
