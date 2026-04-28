using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.ViewModels;
using static LRReader.Shared.Services.Service;

namespace LRReader.Avalonia.Views.Controls;

public partial class ArchiveList : UserControl
{
	public SearchResultsViewModel Data { get; }

	private bool loaded, ready, selectMode;

	private string query = "";

	private string random = $"ArchiveList_{new Random().Next().ToString()}";

	private SearchState? searchState;

	public ArchiveList()
	{
		InitializeComponent();
		Data = (SearchResultsViewModel)DataContext!;
		SearchBox.AddHandler(KeyDownEvent, async (sender, e) =>
		{
			switch (e.Key)
			{
				case Key.Enter:
					query = SearchBox.Text ?? "";
					await HandleSearch();
					break;
			}
		});
		SearchBox.DropDownClosed += async (sender, e) =>
		{
			if (SearchBox.SelectedItem != null)
			{
				query = SearchBox.Text ?? "";
				await HandleSearch();
			}
		};
	}

	private async void UserControl_Loaded(object? sender, RoutedEventArgs e)
	{
		if (loaded)
			return;
		loaded = true;
		AscFlyoutItem.IsChecked = Settings.OrderByDefault == Order.Ascending;
		DesFlyoutItem.IsChecked = Settings.OrderByDefault == Order.Descending;

		if (OnLoad != null)
			await OnLoad();

		if (!string.IsNullOrEmpty(searchState?.Category))
		{
			var category = await CategoriesProvider.GetCategory(searchState.Category);
			if (category != null)
				Data.Category = category;
		}

		await Data.LoadPage(searchState?.Page ?? 0);
		searchState = null;
		ready = true;
	}

	private async void Button_Click(object sender, RoutedEventArgs e) => await Refresh();

	private async Task<IEnumerable<object>> SearchBox_AsyncPopulator(string? arg1, CancellationToken arg2)
	{
		var suggestions = new List<object>();
		if (!string.IsNullOrEmpty(arg1))
		{
			var text = arg1;
			//var queryText = text.Split(",").Last().Trim();
			var comma = text.LastIndexOf(',');
			var queryText = text;

			if (comma != -1)
			{
				text = arg1.Substring(0, comma);
				queryText = arg1.Substring(comma + 1).Trim();
			}

			foreach (var t in Archives.TagStats.Where(t =>
			{
				var names = t.@namespace.ToLower();
				return t.GetNamespacedTag().Contains(queryText, StringComparison.OrdinalIgnoreCase) && !Api.ServerInfo.excluded_namespaces.Contains(names);
			}))
			{
				suggestions.Add((comma == -1 ? "" : text + (string.IsNullOrEmpty(text) ? "" : ", ")) + t.GetNamespacedTag());
				await Task.Yield();
				//Data.Suggestions.Add(t.GetNamespacedTag());
			}
		}
		return suggestions;
	}

	public async void SearchTextChanged(object sender, TextChangedEventArgs args)
	{
		/*if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
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
					return t.GetNamespacedTag().Contains(queryText, StringComparison.OrdinalIgnoreCase) && !Api.ServerInfo.excluded_namespaces.Contains(names);
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
		}*/
		if (string.IsNullOrEmpty(SearchBox.Text) && !string.IsNullOrEmpty(query))
		{
			query = "";
			await HandleSearch();
		}
	}

	/*public async void SearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
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
	}*/
	private async void SearchChanged_Click(object? sender, RoutedEventArgs e)
	{
		query = SearchBox.Text ?? "";
		await HandleSearch();
	}

	private void ClearSearch_Click(object? sender, RoutedEventArgs e)
	{
		query = SearchBox.Text = "";
	}

	private async void FilterToggle_Click(object sender, RoutedEventArgs e) => await Data.ReloadSearch();

	private async void RefreshContainer_RefreshRequested(object? sender, RefreshRequestedEventArgs e)
	{
		var deferral = e.GetDeferral();
		await Refresh();
		deferral.Complete();
	}
	/*
	private async void Refresh_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
	{
		args.Handled = true;
		await Refresh();
	}*/

	private async void PagerControl_SelectedIndexChanged(PagerControl sender, PagerControlSelectedIndexChangedEventArgs args)
	{
		if (ready)
			await Data.LoadPage(args.NewPageIndex);
	}

	private async Task HandleSearch(bool reload = false)
	{
		/*if (Settings.ShowSuggestedTags)
			Data.Query = string.Join(',', query.Trim(','), string.Join(',', SuggestedTags.SelectedItems)).Trim(',');
		else*/
		Data.Query = query;
		await Data.ReloadSearch(reload);
	}

	[RelayCommand]
	public async Task Refresh()
	{
		await HandleSearch(true);
	}

	/*private async void ArchivesGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
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
	}*/

	public void Search(string query)
	{
		SearchBox.Text = Data.Query = this.query = query;
	}

	public void Search(Category category)
	{
		Data.Category = category;
	}

	public void Search(SearchState state)
	{
		searchState = state;
		SearchBox.Text = Data.Query = this.query = state.Query;
		Data.NewOnly = state.New;
		Data.UntaggedOnly = state.Untagged;
		Data.SortByIndex = Data.SortBy.IndexOf(state.SortBy);
		Data.OrderBy = state.OrderBy;
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
		Data.OrderBy = Enum.Parse<Order>((string)((MenuItem)sender).Tag!);
		await HandleSearch();
	}

	private async void SuggestedTags_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		await HandleSearch();
	}

	public SearchTabState GetTabState()
	{
		var sortby = "";
		if (Data.SortByIndex != -1)
			sortby = Data.SortBy.ElementAt(Data.SortByIndex);
		return new(searchState ?? new(Data.Query, Data.Page, Data.NewOnly, Data.UntaggedOnly, sortby, Data.OrderBy, Data.Category.id));
	}

	public bool RandomVisible
	{
		get => GetValue(RandomVisibleProperty);
		set => SetValue(RandomVisibleProperty, value);
	}

	public IDataTemplate? ItemDataTemplate
	{
		get => ArchivesGrid.ItemTemplate;
		set => ArchivesGrid.ItemTemplate = value;
	}

	public bool HandleF5
	{
		get => GetValue(ArchiveStyleButtonVisibilityProperty);
		set => SetValue(ArchiveStyleButtonVisibilityProperty, value);
	}

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

	public bool ArchiveStyleButtonVisibility
	{
		get => GetValue(ArchiveStyleButtonVisibilityProperty);
		set => SetValue(ArchiveStyleButtonVisibilityProperty, value);
	}

	public static readonly StyledProperty<bool> RandomVisibleProperty = AvaloniaProperty.Register<ArchiveList, bool>("RandomVisible", true);
	public static readonly StyledProperty<bool> HandleF5Property = AvaloniaProperty.Register<ArchiveList, bool>("HandleF5", true);
	public static readonly StyledProperty<bool> ArchiveStyleButtonVisibilityProperty = AvaloniaProperty.Register<ArchiveList, bool>("ArchiveStyleButtonVisibility", true);

}