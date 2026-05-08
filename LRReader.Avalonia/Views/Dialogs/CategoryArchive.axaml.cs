using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.ViewModels;

namespace LRReader.Avalonia.Views.Dialogs;

public partial class CategoryArchive : FAContentDialog, IDialog
{
	protected override Type StyleKeyOverride => typeof(FAContentDialog);

	public CategoryArchiveViewModel Data { get; } = null!;
	private bool _searching;

	public CategoryArchive()
	{
		InitializeComponent();
	}

	public CategoryArchive(string archiveID, string title)
	{
		Title = title;
		Data = new CategoryArchiveViewModel(archiveID);
		InitializeComponent();
		Data.SelectedCategories = CategoriesList.SelectedItems;
	}

	private async void ContentDialog_Loaded(object? sender, RoutedEventArgs e)
	{
		_searching = true;
		await Data.Load();
		_searching = false;
	}

	private async void CategoriesList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		if (_searching)
			return;
		foreach (var c in e.AddedItems)
			await Data.AddToCategory(((Category)c).id);
		foreach (var c in e.RemovedItems)
			await Data.RemoveFromCategory(((Category)c).id);
		await Data.Reload();
	}

	private void AutoSuggestBox_TextChanged(object? sender, TextChangedEventArgs args)
	{
		_searching = true;
		Data.Search((sender as TextBox)?.Text ?? "");
		_searching = false;
	}

	private void ContentDialog_PrimaryButtonClick(FAContentDialog sender, FAContentDialogButtonClickEventArgs args)
	{
		_searching = true;
	}

	public async Task<IDialogResult> ShowAsync(object root) => (IDialogResult)(int)await base.ShowAsync((TopLevel)root);

}