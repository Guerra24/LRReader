using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using LRReader.UWP.Views.Items;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using RefreshContainer = Microsoft.UI.Xaml.Controls.RefreshContainer;
using RefreshRequestedEventArgs = Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs;

namespace LRReader.UWP.Views.Tabs.Content
{
	public sealed partial class ArchivesTabContent : UserControl
	{

		private ArchivesPageViewModel Data;

		private bool loaded, reloading;

		private string query = "";

		public ArchivesTabContent()
		{
			this.InitializeComponent();
			if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
				Shadow.Receivers.Add(Root);
			Data = DataContext as ArchivesPageViewModel;
		}

		private async void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (loaded)
				return;
			loaded = true;
			reloading = true;
			AscFlyoutItem.IsChecked = Service.Settings.OrderByDefault == Order.Ascending;
			DesFlyoutItem.IsChecked = Service.Settings.OrderByDefault == Order.Descending;
			Data.ControlsEnabled = false; // THIS ---------------
			Data.LoadBookmarks();
			await HandleSearch();
			Data.ControlsEnabled = true;
			reloading = false;
		}

		private void ArchivesGrid_ItemClick(object sender, ItemClickEventArgs e) => Service.Tabs.OpenTab(Tab.Archive, e.ClickedItem as Archive);

		private async void Button_Click(object sender, RoutedEventArgs e) => await Refresh();

		public async void SearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
			{
				Data.Suggestions.Clear();
				if (!string.IsNullOrEmpty(sender.Text))
				{
					string text;
					var sQuery = sender.Text.ToUpper();
					if (sender.Text.Length > query.Length)
					{
						text = sQuery.Substring(query.Length).TrimStart();
					}
					else
					{
						text = sQuery.Split(" ").Last();
						query = sender.Text.Substring(0, Math.Max(0, sQuery.LastIndexOf(" ")));
					}
					foreach (var t in Service.Archives.TagStats.Where(t => t.GetNamespacedTag().ToUpper().Contains(text)))
					{
						Data.Suggestions.Add(query.TrimEnd() + (string.IsNullOrEmpty(query) ? "" : " ") + t.GetNamespacedTag());
					}
				}
				else if (string.IsNullOrEmpty(sender.Text) && !string.IsNullOrEmpty(query))
				{
					query = "";
					await HandleSearch();
				}
			}
		}

		public async void SearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{
			if (args.ChosenSuggestion != null)
			{
				query = args.ChosenSuggestion as string;
				await HandleSearch();
			}
			else
			{
				query = args.QueryText;
				await HandleSearch();
			}
		}

		private void RandomButton_Click(object sender, RoutedEventArgs e)
		{
			var list = Service.Archives.Archives;
			if (list.Count <= 1)
				return;
			var random = new Random();
			var item = list.ElementAt(random.Next(list.Count - 1));
			Service.Tabs.OpenTab(Tab.Archive, item.Value);
		}

		private async void FilterToggle_Click(object sender, RoutedEventArgs e) => await Data.ReloadSearch();

		private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
		{
			using (var deferral = args.GetDeferral())
			{
				await Refresh();
			}
		}

		private async void Refresh_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			args.Handled = true;
			await Refresh();
		}

		private async void PrevButton_Click(object sender, RoutedEventArgs e) => await Data.PrevPage();

		private async void NextButton_Click(object sender, RoutedEventArgs e) => await Data.NextPage();

		private async void PagerControl_SelectedIndexChanged(PagerControl sender, PagerControlSelectedIndexChangedEventArgs args)
		{
			if (loaded)
				await Data.LoadPage(args.NewPageIndex);
		}

		private async Task HandleSearch()
		{
			Data.Query = query;
			await Data.ReloadSearch();
		}

		public async Task Refresh()
		{
			reloading = true;
			Data.ControlsEnabled = false;
			await Data.Refresh();// THIS ---------------
			AscFlyoutItem.IsChecked = Service.Settings.OrderByDefault == Order.Ascending;
			DesFlyoutItem.IsChecked = Service.Settings.OrderByDefault == Order.Descending;
			await HandleSearch();
			Data.ControlsEnabled = true;
			reloading = false;
		}

		private async void ArchivesGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
		{
			var pointerPoint = e.GetCurrentPoint(ArchivesGrid);
			if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
			{
				if (pointerPoint.Properties.IsXButton1Pressed)
				{
					e.Handled = true;
					await Data.PrevPage();
				}
				else if (pointerPoint.Properties.IsXButton2Pressed)
				{
					e.Handled = true;
					await Data.NextPage();
				}
			}
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			Data.SortByIndex = -1;
		}

		private async void SortBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (loaded && !reloading)
				await HandleSearch();
		}

		private void ArchivesGrid_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
			{
				var itemShadow = (args.ItemContainer as GridViewItem).Shadow;
				if (itemShadow == null)
				{
					(args.ItemContainer as GridViewItem).Shadow = Shadow;
					(args.ItemContainer as GridViewItem).Translation = new Vector3(0, 0, 14);
					(args.ItemContainer as GridViewItem).TranslationTransition = new Vector3Transition();
					(args.ItemContainer as GridViewItem).TranslationTransition.Duration = TimeSpan.FromMilliseconds(200);
					(args.ItemContainer as GridViewItem).PointerEntered += ArchiveItem_PointerEntered;
					(args.ItemContainer as GridViewItem).PointerExited += ArchiveItem_PointerExited;
				}
			}
		}

		private void ArchiveItem_PointerEntered(object sender, PointerRoutedEventArgs e)
		{
			(sender as GridViewItem).Translation = new Vector3(0, 0, 32);
		}

		private void ArchiveItem_PointerExited(object sender, PointerRoutedEventArgs e)
		{
			(sender as GridViewItem).Translation = new Vector3(0, 0, 14);
		}

		private async void OrderBy_Click(object sender, RoutedEventArgs e)
		{
			Data.OrderBy = (Order)Enum.Parse(typeof(Order), (sender as RadioMenuFlyoutItem).Tag as string);
			await HandleSearch();
		}

	}
}
