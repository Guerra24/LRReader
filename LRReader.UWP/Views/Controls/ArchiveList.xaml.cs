using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using static LRReader.Shared.Services.Service;
using RefreshContainer = Microsoft.UI.Xaml.Controls.RefreshContainer;
using RefreshRequestedEventArgs = Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs;

namespace LRReader.UWP.Views.Controls
{
	public sealed partial class ArchiveList : UserControl
	{

		public SearchResultsViewModel Data;

		private bool loaded;

		private string query = "";

		public ArchiveList()
		{
			this.InitializeComponent();
			Data = DataContext as SearchResultsViewModel;
		}

		private async void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (loaded)
				return;
			loaded = true;
			AscFlyoutItem.IsChecked = Settings.OrderByDefault == Order.Ascending;
			DesFlyoutItem.IsChecked = Settings.OrderByDefault == Order.Descending;

			await Refresh();
		}

		private void ArchivesGrid_ItemClick(object sender, ItemClickEventArgs e) => Service.Tabs.OpenTab(Tab.Archive, true, e.ClickedItem as Archive);

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
					foreach (var t in Archives.TagStats.Where(t => t.GetNamespacedTag().ToUpper().Contains(text)))
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
			var list = Archives.Archives;
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
			Data.ControlsEnabled = false;
			await HandleSearch();
			Data.ControlsEnabled = true;
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

		public void Search(string query)
		{
			SearchBox.Text = this.query = query;
		}

		public void Search(Category category)
		{
			SearchBox.Text = this.query = category.search;
			Data.Category = category;
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			Data.SortByIndex = -1;
		}

		private async void SortBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (loaded)
				await HandleSearch();
		}

		private async void OrderBy_Click(object sender, RoutedEventArgs e)
		{
			Data.OrderBy = (Order)Enum.Parse(typeof(Order), (sender as RadioMenuFlyoutItem).Tag as string);
			await HandleSearch();
		}

		// Dependency
		public bool RandomVisible
		{
			get => (bool)GetValue(RandomVisibleProperty);
			set => SetValue(RandomVisibleProperty, value);
		}

		public new bool AllowDrop
		{
			get => (bool)GetValue(AllowDropProperty);
			set => SetValue(AllowDropProperty, value);
		}

		public bool CanDragItems
		{
			get => (bool)GetValue(CanDragItemsProperty);
			set => SetValue(CanDragItemsProperty, value);
		}

		public bool ItemClickEnabled
		{
			get => (bool)GetValue(ItemClickEnabledProperty);
			set => SetValue(ItemClickEnabledProperty, value);
		}

		public ListViewSelectionMode SelectionMode
		{
			get => (ListViewSelectionMode)GetValue(SelectionModeProperty);
			set => SetValue(SelectionModeProperty, value);
		}

		public event DragItemsStartingEventHandler DragItemsStarting = new DragItemsStartingEventHandler((a, b) => { });
		public new event DragEventHandler DragOver = new DragEventHandler((a, b) => { });
		public new event DragEventHandler Drop = new DragEventHandler((a, b) => { });

		public DragItemsStartingEventHandler DragItemsStartingI => DragItemsStarting;
		public DragEventHandler DragOverI => DragOver;
		public DragEventHandler DropI => Drop;

		public DataTemplate ItemDataTemplate
		{
			get => GetValue(ItemDataTemplateProperty) as DataTemplate;
			set => SetValue(ItemDataTemplateProperty, value);
		}

		public bool HandleF5
		{
			get => (bool)GetValue(HandleF5Property);
			set => SetValue(HandleF5Property, value);
		}

		public static readonly DependencyProperty RandomVisibleProperty = DependencyProperty.Register("RandomVisible", typeof(bool), typeof(ArchiveList), new PropertyMetadata(true));
		public static readonly DependencyProperty CanDragItemsProperty = DependencyProperty.Register("CanDragItems", typeof(bool), typeof(ArchiveList), new PropertyMetadata(false));
		public static readonly DependencyProperty ItemClickEnabledProperty = DependencyProperty.Register("ItemClickEnabled", typeof(bool), typeof(ArchiveList), new PropertyMetadata(true));
		public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.Register("SelectionMode", typeof(ListViewSelectionMode), typeof(ArchiveList), new PropertyMetadata(ListViewSelectionMode.None));
		public static readonly DependencyProperty ItemDataTemplateProperty = DependencyProperty.Register("ItemDataTemplate", typeof(DataTemplate), typeof(ArchiveList), new PropertyMetadata(null));
		public static readonly DependencyProperty HandleF5Property = DependencyProperty.Register("HandleF5", typeof(bool), typeof(ArchiveList), new PropertyMetadata(true));
	}
}
