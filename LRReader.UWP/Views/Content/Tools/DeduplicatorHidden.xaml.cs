using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Tools;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Content.Tools
{
	public sealed partial class DeduplicatorHidden : Page
	{

		private DeduplicatorHiddenViewModel Data;

		public DeduplicatorHidden()
		{
			this.InitializeComponent();
			Data = Service.Services.GetRequiredService<DeduplicatorHiddenViewModel>();
			Data.Refresh();
		}

	}
}
