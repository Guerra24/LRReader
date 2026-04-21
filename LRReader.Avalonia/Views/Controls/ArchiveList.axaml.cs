using Avalonia.Interactivity;
using LRReader.Shared.ViewModels;

namespace LRReader.Avalonia.Views.Controls;

public partial class ArchiveList : UserControl
{
	public SearchResultsViewModel Data { get; }

	private bool loaded;

	public ArchiveList()
	{
		InitializeComponent();
		Data = (SearchResultsViewModel)DataContext!;
	}

	private async void UserControl_Loaded(object? sender, RoutedEventArgs e)
	{
		if (loaded)
			return;
		loaded = true;
		if (OnLoad != null)
			await OnLoad();

		await Data.LoadPage(0);
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
}