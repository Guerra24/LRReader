using FluentAvalonia.UI.Controls;

namespace LRReader.Avalonia.Views.Dialogs
{

	public partial class GenericDialog : FAContentDialog
	{
		protected override Type StyleKeyOverride => typeof(FAContentDialog);

		public GenericDialog()
		{
			InitializeComponent();
		}

	}
}
