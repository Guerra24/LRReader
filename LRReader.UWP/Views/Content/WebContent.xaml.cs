#nullable enable
using LRReader.Shared.Services;
using LRReader.UWP.Views.Controls;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace LRReader.UWP.Views.Content
{

	public sealed partial class WebContent : ModernBasePage
	{
		public WebContent()
		{
			this.InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			// Maybe make this more generic instead
			WebView.Navigate(Service.Settings.Profile.ServerAddressBrowser + (string)Wrapper.Parameter!);
		}

		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
			WebView.Close();
			base.OnNavigatingFrom(e);
		}

		private void WebView_OnCloseRequested()
		{
			Wrapper.ModernPageTab.GoBack((int)((Frame)Parent).Tag);
		}
	}
}
