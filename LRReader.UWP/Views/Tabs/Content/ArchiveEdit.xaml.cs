using LRReader.Shared.Models.Main;
using LRReader.UWP.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LRReader.UWP.Views.Tabs.Content
{
	public sealed partial class ArchiveEdit : UserControl
	{

		public ArchiveEditViewModel Data;

		public ArchiveEdit()
		{
			this.InitializeComponent();
			Data = new ArchiveEditViewModel();
		}

		public void LoadArchive(Archive archive)
		{
			Data.LoadArchive(archive);
		}

		public async void Refresh() => await Data.ReloadArchive();

		private async void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			await Data.SaveArchive();
		}
	}
}
