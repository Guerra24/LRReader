using LRReader.Shared.Models.Main;
using LRReader.UWP.Views.Controls;
using Windows.UI.Xaml;

namespace LRReader.UWP.Views.Tabs
{
	public sealed partial class TankoubonTab : ModernTab
	{
		public TankoubonTab(Tankoubon tankoubon)
		{
			this.InitializeComponent();
			this.CustomTabId = "Tankoubon_" + tankoubon.id;
			TabContent.Data.Tankoubon = tankoubon;
		}

	}
}
