using LRReader.Shared.ViewModels.Tools;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Content.Tools
{
	public sealed partial class DeduplicatorHidden : Page
	{

		private DeduplicatorHiddenViewModel Data;

		public DeduplicatorHidden()
		{
			this.InitializeComponent();
			Data = DataContext as DeduplicatorHiddenViewModel;
		}

	}
}
