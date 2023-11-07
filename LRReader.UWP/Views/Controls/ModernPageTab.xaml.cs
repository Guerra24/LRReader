#nullable enable
using System;
using System.Collections.ObjectModel;
using System.Linq;
using LRReader.UWP.Views.Content;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using TwoPaneView = Microsoft.UI.Xaml.Controls.TwoPaneView;
using TwoPaneViewMode = Microsoft.UI.Xaml.Controls.TwoPaneViewMode;

namespace LRReader.UWP.Views.Controls
{
	public sealed partial class ModernPageTab : UserControl
	{

		private ObservableCollection<ModernPageTabItem> MainBreadcrumbItems = new ObservableCollection<ModernPageTabItem>();
		private ObservableCollection<ModernPageTabItem> ExtraBreadcrumbItems = new ObservableCollection<ModernPageTabItem>();
		private ModernPageTabItem? CurrentExtraPage;
		private ModernPageTabItem? CurrentMainPage;

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
				Navigate(new ModernPageTabItem { Title = Title, Page = Initial }, 0);
		}

		public bool GoBack()
		{
			return GoBackExtra() || GoBackMain();
		}

		public void GoBack(int framesource)
		{
			if (framesource == 0)
				GoBackMain();
			else
				GoBackExtra();
		}

		public bool GoBackMain()
		{
			if (MainBreadcrumbItems.Count > 1)
			{
				MainBreadcrumbItems.Remove(MainBreadcrumbItems.Last());
				CurrentMainPage = MainBreadcrumbItems.Last();
				ContentMain.Navigate(MainBreadcrumbItems.Last().Page, new ModernPageTabWrapper(this, CurrentMainPage.Parameter));
				return true;
			}
			return false;
		}

		public bool GoBackExtra()
		{
			if (ExtraBreadcrumbItems.Count > 1)
			{
				ExtraBreadcrumbItems.Remove(ExtraBreadcrumbItems.Last());
				CurrentExtraPage = ExtraBreadcrumbItems.Last();
				ContentExtra.Navigate(ExtraBreadcrumbItems.Last().Page, new ModernPageTabWrapper(this, CurrentExtraPage.Parameter));
				return true;
			}
			return false;
		}

		public void Navigate(ModernPageTabItem item, int framesource)
		{
			// 0 main
			// 1 extra
			// check origin frame and prevent stacking when main opens an extra page, replace it
			if (CurrentMainPage == null || TwoPane.Mode == TwoPaneViewMode.SinglePane)
			{
				CurrentMainPage = item;
				MainBreadcrumbItems.Add(item);
				ContentMain.Navigate(item.Page, new ModernPageTabWrapper(this, item.Parameter));
			}
			else if (TwoPane.Mode == TwoPaneViewMode.Wide && !item.Equals(CurrentExtraPage))
			{
				CurrentExtraPage = item;
				if (framesource == 0)
					ExtraBreadcrumbItems.Clear();
				ExtraBreadcrumbItems.Add(item);
				ContentExtra.Navigate(item.Page, new ModernPageTabWrapper(this, item.Parameter));
			}
			//, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight }
		}

		private void TwoPane_ModeChanged(TwoPaneView sender, object args)
		{
			if (TwoPane.Mode == TwoPaneViewMode.SinglePane && CurrentExtraPage != null)
			{
				ExtraBreadcrumbItems.Remove(CurrentExtraPage);
				foreach (var i in ExtraBreadcrumbItems)
					MainBreadcrumbItems.Add(i);
				ExtraBreadcrumbItems.Clear();

				Navigate(CurrentExtraPage, 0);
				CurrentExtraPage = null;
				ContentExtra.Navigate(typeof(Empty));
			}
			else
			{
				if (MainBreadcrumbItems.Count > 1)
				{
					var main = MainBreadcrumbItems.First();
					var extra = MainBreadcrumbItems.Skip(1).ToList();

					MainBreadcrumbItems.Clear();

					CurrentMainPage = main;
					MainBreadcrumbItems.Add(main);
					ContentMain.Navigate(CurrentMainPage.Page, new ModernPageTabWrapper(this, main.Parameter));

					foreach (var i in extra)
						ExtraBreadcrumbItems.Add(i);
					CurrentExtraPage = ExtraBreadcrumbItems.Last();
					ContentExtra.Navigate(CurrentExtraPage.Page, new ModernPageTabWrapper(this, CurrentExtraPage.Parameter));
				}
			}
		}

		private void MainBreadcrumb_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
		{
			if (args.Index < MainBreadcrumbItems.Count - 1)
			{
				for (int i = MainBreadcrumbItems.Count - 1; i > args.Index; i--)
				{
					MainBreadcrumbItems.RemoveAt(i);
				}
				var item = (ModernPageTabItem)args.Item;
				ContentMain.Navigate(item.Page, new ModernPageTabWrapper(this, item.Parameter));
			}
		}

		private void ExtraBreadcrumb_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
		{
			if (args.Index < ExtraBreadcrumbItems.Count - 1)
			{
				for (int i = ExtraBreadcrumbItems.Count - 1; i > args.Index; i--)
				{
					ExtraBreadcrumbItems.RemoveAt(i);
				}
				var item = (ModernPageTabItem)args.Item;
				ContentExtra.Navigate(item.Page, new ModernPageTabWrapper(this, item.Parameter));
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

		public object? Parameter { get; set; }

		public override bool Equals(object? obj)
		{
			return obj is ModernPageTabItem item && Page.Equals(item.Page);
		}

		public override int GetHashCode()
		{
			return Page.GetHashCode();
		}
	}

	public class ModernPageTabWrapper
	{
		public ModernPageTab ModernPageTab { get; } = null!;
		public object? Parameter { get; }

		public ModernPageTabWrapper(ModernPageTab modernPageTab, object? parameter)
		{
			ModernPageTab = modernPageTab;
			Parameter = parameter;
		}
	}
}
