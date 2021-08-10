using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media.Animation;

namespace LRReader.UWP.Views.Controls
{
	[ContentProperty(Name = "Items")]
	public sealed partial class ModernPageTab : UserControl
	{

		private ObservableCollection<ModernPageTabItem> BreadcrumbItems = new ObservableCollection<ModernPageTabItem>();

		private bool _loaded;

		public ModernPageTab()
		{
			this.InitializeComponent();
			Items = new List<ModernPageTabItem>();
		}

		public IList<ModernPageTabItem> Items
		{
			get => GetValue(ItemsProperty) as IList<ModernPageTabItem>;
			set => SetValue(ItemsProperty, value);
		}

		public string Header
		{
			get => GetValue(HeaderProperty) as string;
			set => SetValue(HeaderProperty, value);
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (_loaded)
				return;
			_loaded = true;
			Navigate(new ModernPageTabItem { Header = Header, Page = typeof(ModernPageTabInitial) });
		}

		private void GoBack_Click(object sender, RoutedEventArgs e)
		{
			if (BreadcrumbItems.Count > 1)
			{
				BreadcrumbItems.Remove(BreadcrumbItems.Last());
				Content.Navigate(BreadcrumbItems.Last().Page, this);
			}
		}

		public void Navigate(ModernPageTabItem item)
		{
			BreadcrumbItems.Add(item);
			Content.Navigate(item.Page, this);
		}

		private void Breadcrumb_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
		{
			if (args.Index < BreadcrumbItems.Count - 1)
			{
				for (int i = BreadcrumbItems.Count - 1; i > args.Index; i--)
				{
					BreadcrumbItems.RemoveAt(i);
				}
				var item = (ModernPageTabItem)args.Item;
				Content.Navigate(item.Page, this);
			}
		}

		public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(IList<ModernPageTabItem>), typeof(ModernPageTab), new PropertyMetadata(null));
		public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(ModernPageTab), new PropertyMetadata(null));
	}

	public class ModernPageTabItem
	{
		public string Header { get; set; }
		public string Description { get; set; }
		public Type Page { get; set; }
		public string Icon { get; set; }
	}
}
