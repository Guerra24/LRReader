#nullable enable
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using LRReader.UWP.Views.Items;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.System;
using Windows.UI.Core;
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
			Data = (ArchivesPageViewModel)DataContext;
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

		private void ArchivesGrid_ItemClick(object sender, ItemClickEventArgs e) => Service.Archives.OpenTab((Archive)e.ClickedItem, (CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control) & CoreVirtualKeyStates.Down) != CoreVirtualKeyStates.Down, Data.ArchiveList.ToList());

		private async void Button_Click(object sender, RoutedEventArgs e) => await Refresh();

		public async void SearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
			{
				Data.Suggestions.Clear();
				if (!string.IsNullOrEmpty(sender.Text))
				{
					string text;
					var sQuery = sender.Text.ToLower();
					if (sender.Text.Length > query.Length)
					{
						text = sQuery.Substring(query.Length).TrimStart();
					}
					else
					{
						text = sQuery.Split(" ").Last();
						query = sender.Text.Substring(0, Math.Max(0, sQuery.LastIndexOf(" ")));
					}
					foreach (var t in Service.Archives.TagStats.Where(t =>
					{
						var names = t.@namespace.ToLower();
						return t.GetNamespacedTag().ToLower().Contains(text) && !names.Equals("date_added") && !names.Equals("source");
					}))
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
				query = (string)args.ChosenSuggestion;
				await HandleSearch();
			}
			else
			{
				query = args.QueryText;
				await HandleSearch();
			}
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

		private async void OrderBy_Click(object sender, RoutedEventArgs e)
		{
			Data.OrderBy = (Order)Enum.Parse(typeof(Order), ((RadioMenuFlyoutItem)sender).Tag as string);
			await HandleSearch();
		}

		private void ArchivesGrid_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			if (args.ItemContainer.ContentTemplateRoot is GenericArchiveItem item && item.Group == null)
				item.Group = Data.ArchiveList.ToList();
		}

	}

	public class ArchiveTemplateSelector : DataTemplateSelector
	{
		public DataTemplate CompactTemplate { get; set; } = null!;
		public DataTemplate FullTemplate { get; set; } = null!;

		protected override DataTemplate SelectTemplateCore(object item)
		{
			//if (false)
				//return CompactTemplate;
			//else
				return FullTemplate;
		}
		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			return SelectTemplateCore(item);
		}
	}
}
