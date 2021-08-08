using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace LRReader.UWP.Views.Controls
{
	public sealed partial class ModernPageTabInitial : Page
	{

		private ModernPageTab Page;

		public ModernPageTabInitial()
		{
			this.InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			if (e.Parameter is ModernPageTab page && Page == null)
			{
				Page = page;
				foreach (var item in page.Items)
				{
					var tmp = new ModernInput { Title = item.Title, Tag = item, IsButton = true, Glyph = item.Icon, RightGlyph = "\uE76C" };
					tmp.Click += PageButton_Click;
					Pages.Items.Add(tmp);
				}
			}
		}

		private void PageButton_Click(object sender, RoutedEventArgs e)
		{
			Page.Navigate((sender as ModernInput).Tag as ModernPageTabItem, new SuppressNavigationTransitionInfo() /*new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight }*/);
		}
	}
}
