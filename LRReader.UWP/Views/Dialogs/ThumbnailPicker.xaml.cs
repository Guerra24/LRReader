using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using LRReader.UWP.Extensions;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Dialogs
{
	public sealed partial class ThumbnailPicker : ContentDialog, IThumbnailPickerDialog
	{
		private ObservableCollection<ImagePageSet> Thumbnails = new ObservableCollection<ImagePageSet>();
		private string id;

		public ThumbnailPicker(string id)
		{
			this.InitializeComponent();
			RequestedTheme = Service.Platform.Theme.ToXamlTheme();
			this.id = id;
		}

		public int Page { get => ImagesGrid.SelectedIndex + 1; set { } }

		public new async Task<IDialogResult> ShowAsync() => (IDialogResult)(int)await base.ShowAsync();

		public async Task LoadThumbnails()
		{
			Thumbnails.Clear();
			var archive = Service.Archives.GetArchive(id);
			if (archive != null)
			{
				if (archive.pagecount > 0)
				{
					for (int i = 1; i <= archive.pagecount; i++)
						Thumbnails.Add(new ImagePageSet(id, null, i));
				}
				else
				{
					var result = await ArchivesProvider.ExtractArchive(id);
					if (result != null)
						await result.WaitForMinionJob();
					if (result != null)
					{
						await Task.Run(async () =>
						{
							foreach (var (s, index) in result.pages.Select((item, index) => (item, index)))
								await Service.Dispatcher.RunAsync(() => Thumbnails.Add(new ImagePageSet(id, s, index + 1)), 10);
						});
					}
				}
			}
		}
	}
}
