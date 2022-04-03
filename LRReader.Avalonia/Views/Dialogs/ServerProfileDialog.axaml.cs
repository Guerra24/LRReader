using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using LRReader.Shared.Models;

namespace LRReader.Avalonia.Views.Dialogs
{
	public partial class ServerProfileDialog : GenericDialog, ICreateProfileDialog
	{

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

		private void ProfileName_TextChanging(object? sender, TextInputEventArgs e)
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

		private void ProfileServerAddress_TextChanging(object? sender, TextInputEventArgs e)
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
