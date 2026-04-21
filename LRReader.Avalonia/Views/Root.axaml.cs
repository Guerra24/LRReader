using System.Diagnostics;

namespace LRReader.Avalonia.Views
{
	public partial class Root : UserControl
	{
		public Root()
		{
			InitializeComponent();
		}

		private void FrameContent_NavigationFailed(object sender, FluentAvalonia.UI.Navigation.NavigationFailedEventArgs e)
		{
			Debugger.Break();
		}
	}
}
