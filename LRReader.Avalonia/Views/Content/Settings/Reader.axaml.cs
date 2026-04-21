using Avalonia.Interactivity;
using Avalonia.Media;
using LRReader.Avalonia.Resources;
using LRReader.Avalonia.Views.Controls;
using LRReader.Shared.ViewModels;

namespace LRReader.Avalonia.Views.Content.Settings;

public partial class Reader : ModernBasePage
{
	public SettingsPageViewModel Data { get; }

	public Reader()
	{
		InitializeComponent();
		Data = (SettingsPageViewModel)DataContext!;

		var lang = ResourceLoader.GetForCurrentView("Settings");
		/*
			<x:String>All archives</x:String>
			<x:String>Only "New" Archives</x:String>
		 */
		ClearNew.Items.Add(lang.GetString("Reader/ClearNew/Original"));
		ClearNew.Items.Add(lang.GetString("Reader/ClearNew/Web"));
		ClearNew.Items.Add(lang.GetString("Reader/ClearNew/Custom"));
	}

	private void ColorPicker_ColorChanged(object? sender, ColorChangedEventArgs args)
	{
		((SolidColorBrush)Application.Current!.Resources["CustomReaderBackground"]!).Color = args.NewColor;
	}

	private async void Page_Loaded(object sender, RoutedEventArgs e)
	{
		await Data.RefreshCategories();
	}

	private void SyncBookmarks_Toggled(object? sender, RoutedEventArgs e)
	{
		var toggleSwitch = (ToggleSwitch)sender!;
		Data.SettingsManager.Profile.SynchronizeBookmarks = toggleSwitch!.IsChecked!.Value;
		Data.SettingsManager.SaveProfiles();
	}
}