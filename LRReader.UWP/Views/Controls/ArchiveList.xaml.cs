using CommunityToolkit.WinUI;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using LRReader.UWP.Views.Items;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using WinRT;
using static LRReader.Shared.Services.Service;
using RefreshContainer = Microsoft.UI.Xaml.Controls.RefreshContainer;
using RefreshRequestedEventArgs = Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs;

namespace LRReader.UWP.Views.Controls
{
	[ContentProperty(Name = "ItemDataTemplate")]
	public sealed partial class ArchiveList : UserControl
	{

		public SearchResultsViewModel Data;

		private bool loaded, ready;

		private string query = "";

		private string random = $"ArchiveList_{new Random().Next().ToString()}";

		public ArchiveList()
		{
			this.InitializeComponent();
			Data = Service.Services.GetRequiredService<SearchResultsViewModel>();
		}

		private async void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (loaded)
				return;
			loaded = true;
			AscFlyoutItem.IsChecked = Settings.OrderByDefault == Order.Ascending;
			DesFlyoutItem.IsChecked = Settings.OrderByDefault == Order.Descending;

			if (OnLoad != null)
				await OnLoad();
			await HandleSearch();
			ready = true;
		}

		private async void Button_Click(object sender, RoutedEventArgs e) => await Refresh();

		public async void SearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
			{
				Data.Suggestions.Clear();
				if (!string.IsNullOrEmpty(sender.Text))
				{
					var text = sender.Text;
					//var queryText = text.Split(",").Last().Trim();
					var comma = text.LastIndexOf(',');
					var queryText = text;

					if (comma != -1)
					{
						text = sender.Text.Substring(0, comma);
						queryText = sender.Text.Substring(comma + 1).Trim();
					}

					foreach (var t in Archives.TagStats.Where(t =>
					{
						var names = t.@namespace.ToLower();
						return t.GetNamespacedTag().Contains(queryText, StringComparison.OrdinalIgnoreCase) && !names.Equals("date_added") && !names.Equals("source");
					}))
					{
						Data.Suggestions.Add((comma == -1 ? "" : text + (string.IsNullOrEmpty(text) ? "" : ", ")) + t.GetNamespacedTag());
						//Data.Suggestions.Add(t.GetNamespacedTag());
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
			if (ready)
				await Data.LoadPage(args.NewPageIndex);
		}

		private async Task HandleSearch(bool reload = false)
		{
			if (Settings.ShowSuggestedTags)
				Data.Query = string.Join(',', query.Trim(','), string.Join(',', SuggestedTags.SelectedItems)).Trim(',');
			else
				Data.Query = query;
			await Data.ReloadSearch(reload);
		}

		public async Task Refresh()
		{
			await HandleSearch(true);
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

		[DynamicWindowsRuntimeCast(typeof(RadioMenuFlyoutItem))]
		private async void OrderBy_Click(object sender, RoutedEventArgs e)
		{
			Data.OrderBy = Enum.Parse<Order>((string)((RadioMenuFlyoutItem)sender).Tag);
			await HandleSearch();
		}

		private async void SuggestedTags_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			await HandleSearch();
		}

		private void ArchivesGrid_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			if (!args.InRecycleQueue && args.ItemContainer.ContentTemplateRoot is GenericArchiveItem item)
			{
				if (item.Group == null)
					item.Group = Data.ArchiveList.ToList();
				item.Phase0();
				args.RegisterUpdateCallback(Phase1);
			}
			args.Handled = true;
		}

		private void Phase1(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			if (!args.InRecycleQueue && args.ItemContainer.ContentTemplateRoot is GenericArchiveItem item)
			{
				item.Phase1((Archive)args.Item);
				args.RegisterUpdateCallback(Phase2);
			}
		}

		private void Phase2(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			if (!args.InRecycleQueue && args.ItemContainer.ContentTemplateRoot is GenericArchiveItem item)
				item.Phase2();
		}

		// Dependency
		[GeneratedDependencyProperty(DefaultValue = true)]
		public partial bool RandomVisible { get; set; }

		public new bool AllowDrop
		{
			get => ArchivesGrid.AllowDrop;
			set => ArchivesGrid.AllowDrop = value;
		}

		public bool CanDragItems
		{
			get => ArchivesGrid.CanDragItems;
			set => ArchivesGrid.CanDragItems = value;
		}

		public bool ItemClickEnabled
		{
			get => ArchivesGrid.IsItemClickEnabled;
			set => ArchivesGrid.IsItemClickEnabled = value;
		}

		public ListViewSelectionMode SelectionMode
		{
			get => ArchivesGrid.SelectionMode;
			set
			{
				ArchivesGrid.SelectionMode = value;
				if (value == ListViewSelectionMode.Multiple || value == ListViewSelectionMode.Extended)
					VisualStateManager.GoToState(this, "Selected", false);
			}
		}

		public IList<object> SelectedItems
		{
			get => ArchivesGrid.SelectedItems;
		}

		public event DragItemsStartingEventHandler DragItemsStarting = new DragItemsStartingEventHandler((a, b) => { });
		public new event DragEventHandler DragOver = new DragEventHandler((a, b) => { });
		public new event DragEventHandler Drop = new DragEventHandler((a, b) => { });

		public DragItemsStartingEventHandler DragItemsStartingI => DragItemsStarting;
		public DragEventHandler DragOverI => DragOver;
		public DragEventHandler DropI => Drop;

		public DataTemplate ItemDataTemplate
		{
			get => ArchivesGrid.ItemTemplate;
			set => ArchivesGrid.ItemTemplate = value;
		}

		public DataTemplateSelector ItemTemplateSelector
		{
			get => ArchivesGrid.ItemTemplateSelector;
			set => ArchivesGrid.ItemTemplateSelector = value;
		}

		[GeneratedDependencyProperty(DefaultValue = true)]
		public partial bool HandleF5 { get; set; }

		public event Func<Task>? OnRefresh
		{
			add
			{
				Data.OnRefresh += value;
			}
			remove
			{
				Data.OnRefresh -= value;
			}
		}

		public event Func<Task>? OnLoad;

		[GeneratedDependencyProperty(DefaultValue = Visibility.Visible)]
		public partial Visibility ArchiveStyleButtonVisibility { get; set; }
	}
}
