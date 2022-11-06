#nullable enable
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
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
			get => (string)GetValue(TitleProperty);
			set => SetValue(TitleProperty, value);
		}

		public Type Initial
		{
			get => (Type)GetValue(InitialProperty);
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

		public bool GoBack()
		{
			if (BreadcrumbItems.Count > 1)
			{
				BreadcrumbItems.Remove(BreadcrumbItems.Last());
				ContentFrame.Navigate(BreadcrumbItems.Last().Page, this);
				return true;
			}
			return false;
		}

		public void Navigate(ModernPageTabItem item)
		{
			BreadcrumbItems.Add(item);
			ContentFrame.Navigate(item.Page, this);
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
				ContentFrame.Navigate(item.Page, this);
			}
		}

		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(ModernPageTab), new PropertyMetadata(null));
		public static readonly DependencyProperty InitialProperty = DependencyProperty.Register("Initial", typeof(Type), typeof(ModernPageTab), new PropertyMetadata(null));
	}

	public class ModernPageTabItem
	{
		public string Title { get; set; } = null!;
		public string Description { get; set; } = null!;
		public Type Page { get; set; } = null!;
	}
}
