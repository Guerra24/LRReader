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
			dataPackage.SetText((sender as MenuFlyoutItem).Tag as string);
			Clipboard.SetContent(dataPackage);
		}

		private void Tags_ItemClick(object sender, ItemClickEventArgs e)
		{
			if (ItemClickCommand?.CanExecute(e.ClickedItem) ?? false)
				ItemClickCommand.Execute(e.ClickedItem);
		}

		public ICommand ItemClickCommand
		{
			get => GetValue(ItemClickCommandProperty) as ICommand;
			set => SetValue(ItemClickCommandProperty, value);
		}

		public ICollection<ArchiveTagsGroup> ItemsSource
		{
			get => GetValue(ItemsSourceProperty) as ICollection<ArchiveTagsGroup>;
			set => SetValue(ItemsSourceProperty, value);
		}

		public double MaxTagsWidth
		{
			get => (double)GetValue(MaxTagsWidthProperty);
			set => SetValue(MaxTagsWidthProperty, value);
		}

		public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(ICollection<ArchiveTagsGroup>), typeof(ArchiveTags), new PropertyMetadata(new List<ArchiveTagsGroup>()));
		public static readonly DependencyProperty ItemClickCommandProperty = DependencyProperty.Register("ItemClickCommand", typeof(ICommand), typeof(ArchiveTags), new PropertyMetadata(null));
		public static readonly DependencyProperty MaxTagsWidthProperty = DependencyProperty.Register("MaxTagsWidth", typeof(double), typeof(ArchiveTags), new PropertyMetadata(double.MaxValue));

		private void TagsList_Loaded(object sender, RoutedEventArgs e)
		{
			var grid = sender as GridView;
			if (grid.Tag == null)
			{
				grid.MaxWidth = MaxTagsWidth;
				grid.Tag = "The absolute state of UWP Bindings";
			}
		}
	}
}
