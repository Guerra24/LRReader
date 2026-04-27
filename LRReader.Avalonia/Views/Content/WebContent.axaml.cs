using FluentAvalonia.UI.Navigation;
using LRReader.Avalonia.Views.Controls;
using LRReader.Shared.Services;

namespace LRReader.Avalonia.Views.Content;

public partial class WebContent : ModernBasePage
{

	public WebContent()
	{
		InitializeComponent();
	}

	protected override void OnNavigatedTo(object? sender, FANavigationEventArgs e)
	{
		base.OnNavigatedTo(sender, e);
		WebView.Navigate(Service.Settings.Profile.ServerAddressBrowser + (string)Wrapper.Parameter!);
	}

	protected override void OnNavigatingFrom(object? sender, FANavigatingCancelEventArgs e)
	{
		base.OnNavigatingFrom(sender, e);
	}

	private void WebView_OnCloseRequested()
	{
		Wrapper.ModernPageTab.GoBack();
	}

}