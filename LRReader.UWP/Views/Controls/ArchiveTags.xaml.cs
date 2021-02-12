using LRReader.Shared.Models.Main;
using System.Collections.Generic;
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
			dataPackage.SetText((sender as MenuFlyoutItem).Tag as string);
			Clipboard.SetContent(dataPackage);
		}

		private void Tags_ItemClick(object sender, ItemClickEventArgs e) => ItemClick?.Invoke(sender, e);

		public event ItemClickEventHandler ItemClick;

		public ICollection<ArchiveTagsGroup> ItemsSource
		{
			get => GetValue(ItemsSourceProperty) as ICollection<ArchiveTagsGroup>;
			set => SetValue(ItemsSourceProperty, value);
		}

		public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(ICollection<ArchiveTagsGroup>), typeof(ArchiveTags), new PropertyMetadata(new List<ArchiveTagsGroup>()));

	}
}
