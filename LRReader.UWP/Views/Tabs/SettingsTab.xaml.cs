using LRReader.UWP.Views.Controls;

namespace LRReader.UWP.Views.Tabs
{
	public sealed partial class SettingsTab : ModernTab
	{
		public SettingsTab()
		{
			this.InitializeComponent();
		}

		public override void Unload()
		{
			base.Unload();
			TabContent.RemoveTimer();
		}
	}
}
