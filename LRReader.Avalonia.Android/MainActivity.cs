using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace LRReader.Avalonia.Android
{
	[Activity(
		Label = "LRReader",
		Theme = "@style/MyTheme.NoActionBar",
		Icon = "@drawable/icon",
		MainLauncher = true,
		ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density | ConfigChanges.FontScale)]
	public class MainActivity : AvaloniaMainActivity
	{
	}
}
