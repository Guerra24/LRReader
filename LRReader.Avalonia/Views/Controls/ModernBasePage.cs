using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;

namespace LRReader.Avalonia.Views.Controls;

public class ModernBasePage : UserControl
{
	protected ModernPageTabWrapper Wrapper = null!;

	public ModernBasePage()
	{
		AddHandler(Frame.NavigatedToEvent, OnNavigatedTo);
		AddHandler(Frame.NavigatingFromEvent, OnNavigatingFrom);
	}

	protected virtual void OnNavigatedTo(object? sender, NavigationEventArgs e)
	{
		if (e.Parameter is ModernPageTabWrapper wrapper)
			Wrapper = wrapper;
	}

	protected virtual void OnNavigatingFrom(object? sender, NavigatingCancelEventArgs e)
	{
	}

	protected void PageButton_Click(object sender, RoutedEventArgs e)
	{
		/*if (_navigating)
			return;
		_navigating = true;*/
		if (Parent == null)
			return;
		Wrapper.ModernPageTab.Navigate((ModernPageTabItem)((ModernInput)sender).Tag!, (int)((Frame)Parent).Tag!);
	}
}
