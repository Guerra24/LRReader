using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Controls
{
	public sealed partial class ModernPageTab : UserControl
	{

		private ObservableCollection<ModernPageTabItem> BreadcrumbItems = new ObservableCollection<ModernPageTabItem>();

		private bool _loaded;

		public ModernPageTab()
		{
			this.InitializeComponent();
		}

		public string Title
		{
			get => GetValue(TitleProperty) as string;
			set => SetValue(TitleProperty, value);
		}

		public Type Initial
		{
			get => GetValue(InitialProperty) as Type;
			set => SetValue(InitialProperty, value);
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (_loaded)
				return;
			_loaded = true;
			if (Initial != null)
				Navigate(new ModernPageTabItem { Title = Title, Page = Initial });
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

		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(ModernPageTab), new PropertyMetadata(null));
		public static readonly DependencyProperty InitialProperty = DependencyProperty.Register("Initial", typeof(Type), typeof(ModernPageTab), new PropertyMetadata(null));
	}

	public class ModernPageTabItem
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public Type Page { get; set; }
	}
}
