using LRReader.Shared.ViewModels;

namespace LRReader.Avalonia.Views.Main
{
	public partial class FirstRunPage : UserControl
	{
		public SettingsPageViewModel Data { get; }

		public FirstRunPage()
		{
			InitializeComponent();
			Data = (SettingsPageViewModel)DataContext!;
		}

	}
}
