using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace LRReader.UWP.Views.Content.Tools
{

	public sealed partial class BulkEditor : Page
	{
		private BulkEditorViewModel Data;

		public BulkEditor()
		{
			this.InitializeComponent();
			Data = Service.Services.GetRequiredService<BulkEditorViewModel>();
		}

	}
}
