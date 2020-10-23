using LRReader.Internal;

namespace LRReader.UWP.Views.Tabs
{
	public sealed partial class SettingsTab : CustomTab
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
