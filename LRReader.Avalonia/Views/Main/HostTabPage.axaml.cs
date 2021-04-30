using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LRReader.Avalonia.Views.Tabs;
using LRReader.Shared.Services;

namespace LRReader.Avalonia.Views.Main
{
	public class HostTabPage : UserControl
	{
		private TabsService Data;

		private ResourceLoader lang;

		public HostTabPage()
		{
			InitializeComponent();
			Data = DataContext as TabsService;
			lang = ResourceLoader.GetForCurrentView("Pages");
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		private async void HostTabPage_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
		{
			if (Design.IsDesignMode)
				return;
			Data.Hook();

			await Service.Dispatcher.RunAsync(() =>
			{
				Data.AddTab(new ArchivesTab());
				/*if (Settings.OpenBookmarksTab)
					Data.AddTab(new BookmarksTab(), false);
				if (Api.ControlFlags.CategoriesEnabled)
					if (Settings.OpenCategoriesTab)
						Data.AddTab(new CategoriesTab(), false);*/
			});
		}
		
		private void HostTabPage_DetachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
		{
			if (Design.IsDesignMode)
				return;
			Data.UnHook();
		}
	}
}
