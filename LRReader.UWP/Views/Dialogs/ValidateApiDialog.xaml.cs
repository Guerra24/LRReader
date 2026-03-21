using LRReader.Shared.Models;

namespace LRReader.UWP.Views.Dialogs
{
	public sealed partial class ValidateApiDialog : ContentDialog, IDialog
	{

		private bool ArchivesTest;
		private bool CategoriesTest;
		private bool DatabaseTest;

		public ValidateApiDialog(bool archives, bool categories, bool database)
		{
			ArchivesTest = archives;
			CategoriesTest = categories;
			DatabaseTest = database;
			this.InitializeComponent();
		}

		public new async Task<IDialogResult> ShowAsync() => (IDialogResult)(int)await base.ShowAsync();
	}
}
