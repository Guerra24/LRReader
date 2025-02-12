using LRReader.Shared.Services;
using LRReader.UWP.Extensions;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Dialogs
{
	public sealed partial class GenericDialog : ContentDialog
	{
		public GenericDialog()
		{
			this.InitializeComponent();
			RequestedTheme = Service.Platform.Theme.ToXamlTheme();
		}

	}
}
