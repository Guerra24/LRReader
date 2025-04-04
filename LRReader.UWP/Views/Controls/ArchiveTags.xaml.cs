﻿using CommunityToolkit.WinUI;
using LRReader.Shared.Models.Main;
using System.Collections.Generic;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Controls
{
	public sealed partial class ArchiveTags : UserControl
	{
		public ArchiveTags()
		{
			this.InitializeComponent();
		}

		private void Tags_CopyTag(object sender, RoutedEventArgs e)
		{
			var dataPackage = new DataPackage();
			dataPackage.RequestedOperation = DataPackageOperation.Copy;
			dataPackage.SetText(((MenuFlyoutItem)sender).Tag as string);
			Clipboard.SetContent(dataPackage);
		}

		private void Tags_ItemClick(object sender, ItemClickEventArgs e)
		{
			if (ItemClickCommand?.CanExecute(e.ClickedItem) ?? false)
				ItemClickCommand.Execute(e.ClickedItem);
		}

		[GeneratedDependencyProperty]
		public partial ICommand? ItemClickCommand { get; set; }

		[GeneratedDependencyProperty(DefaultValueCallback = "GetDefaultItemSource")]
		public partial ICollection<ArchiveTagsGroup> ItemsSource { get; set; }

		private static ICollection<ArchiveTagsGroup> GetDefaultItemSource() => new List<ArchiveTagsGroup>();

		[GeneratedDependencyProperty(DefaultValue = double.MaxValue)]
		public partial double MaxTagsWidth { get; set; }

		private void TagsList_Loaded(object sender, RoutedEventArgs e)
		{
			var grid = sender as GridView;
			if (grid?.Tag == null)
			{
				grid!.MaxWidth = MaxTagsWidth;
				grid.Tag = "The absolute state of UWP Bindings";
			}
		}
	}
}
