using LRReader.Internal;
using LRReader.Views.Main;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LRReader.Views
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class HostPage : Page
	{
		public HostPage()
		{
			this.InitializeComponent();
			CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
			coreTitleBar.ExtendViewIntoTitleBar = true;

			ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
			titleBar.ButtonBackgroundColor = Colors.Transparent;
			titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
			titleBar.ButtonForegroundColor = (Color)this.Resources["SystemBaseHighColor"];

			Global.Init(); // Init global static data
			Global.EventManager.ShowErrorEvent += ShowError;

			ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;

			object firstRun = roamingSettings.Values["FirstRun"];
			if (firstRun != null)
			{
				if (!(bool)firstRun)
				{
					Global.LRRApi.RefreshSettings();
					NavView.SelectedItem = NavView.MenuItems[0];
					NavView_Navigate("archives", new EntranceNavigationTransitionInfo());
				}
			}
			else
			{
				ContentFrame.Navigate(typeof(FirstRun), new DrillInNavigationTransitionInfo());
			}
		}

		private readonly List<(string Tag, Type Page)> pages = new List<(string Tag, Type Page)>
		{
			("archives", typeof(ArchivesPage)),
			("archives", typeof(ArchivePage)),
			("archives", typeof(ReaderPage)),
			("stats", typeof(StatisticsPage))
		};

		private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
		{
			ContentFrame.GoBack();
		}

		private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
		{
			if (args.IsSettingsInvoked)
			{
				NavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
			}
			else if (args.InvokedItemContainer != null)
			{
				var tag = args.InvokedItemContainer.Tag.ToString();
				NavView_Navigate(tag, args.RecommendedNavigationTransitionInfo);
			}
		}

		private void NavView_Navigate(string tag, NavigationTransitionInfo transInfo)
		{
			Type page = null;
			if (tag == "settings")
			{
				page = typeof(SettingsPage);
			}
			else
			{
				var item = pages.FirstOrDefault(p => p.Tag.Equals(tag));
				page = item.Page;
			}
			Type prevPage = ContentFrame.CurrentSourcePageType;
			if (!(page is null) && !Type.Equals(prevPage, page))
			{
				ContentFrame.Navigate(page, null, transInfo);
			}
		}

		private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
		{
			if (ContentFrame.SourcePageType == typeof(SettingsPage))
			{
				NavView.SelectedItem = NavView.SettingsItem;
			}
			else if (ContentFrame.SourcePageType != null)
			{
				var item = pages.FirstOrDefault(p => p.Page == e.SourcePageType);

				NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(n => n.Tag.Equals(item.Tag));
			}
		}

		private async void ShowError(string title, string content)
		{
			ContentDialog noServer = new ContentDialog()
			{
				Title = title,
				Content = content,
				CloseButtonText = "Ok"
			};
			await noServer.ShowAsync();
		}
	}
}
