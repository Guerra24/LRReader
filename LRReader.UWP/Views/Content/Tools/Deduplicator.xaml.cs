using LRReader.Shared.ViewModels.Tools;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Content.Tools
{
	public sealed partial class Deduplicator : UserControl
	{
		private DeduplicatorToolViewModel Data;

		public Deduplicator()
		{
			this.InitializeComponent();
			Data = DataContext as DeduplicatorToolViewModel;
		}
	}
}
