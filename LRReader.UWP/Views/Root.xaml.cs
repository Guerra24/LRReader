using Microsoft.UI.Xaml.Media;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views
{
	public sealed partial class Root : Page
	{
		public Root()
		{
			this.InitializeComponent();
			if (!ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 13))
			{
				Background = (AcrylicBrush)Resources["MicaFallbackBrush"];
			}
		}
	}
}
