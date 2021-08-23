using LRReader.UWP.Views.Controls;

namespace LRReader.UWP.Views.Content.Settings
{
	public sealed partial class Main : ModernBasePage
	{

		public Main()
		{
			this.InitializeComponent();
#if !SIDELOAD
			Updates.IsEnabled = false;
#endif
		}

	}
}
