using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LRReader.Avalonia.Views.Tabs;
using LRReader.Shared.Messages;
using LRReader.Shared.Services;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace LRReader.Avalonia.Views.Main
{
	public class HostTabPage : UserControl
	{
		private TabsService Data;

		public HostTabPage()
		{
			InitializeComponent();
			Data = DataContext as TabsService;
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		private async void HostTabPage_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
		{
			if (Design.IsDesignMode)
				return;
			//WeakReferenceMessenger.Default.Register(this);

			await Service.Dispatcher.RunAsync(() =>
			{
				Data.OpenTab(Tab.Archives);
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
			//WeakReferenceMessenger.Default.UnregisterAll(this);
		}
	}
}
