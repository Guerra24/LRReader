using LRReader.UWP.Views.Content;
using System.Diagnostics.CodeAnalysis;
using TwoPaneView = Microsoft.UI.Xaml.Controls.TwoPaneView;
using TwoPaneViewMode = Microsoft.UI.Xaml.Controls.TwoPaneViewMode;

namespace LRReader.UWP.Views.Controls;

public sealed partial class ModernPageTab : UserControl, IDisposable
{

	[GeneratedDependencyProperty]
	public partial string? Title { get; set; }

	[GeneratedDependencyProperty]
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	public partial Type? Initial { get; set; }

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		if (_loaded)
			return;
		_loaded = true;
		if (Initial != null)
			Navigate(new ModernPageTabItem { Title = Title!, Page = Initial }, 0);
	}

	private void TwoPane_ModeChanged(TwoPaneView sender, object args)
	{
		if (TwoPane.Mode == TwoPaneViewMode.SinglePane && CurrentExtraPage != null)
		{
			_ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
			{
				ExtraBreadcrumbItems.Remove(CurrentExtraPage);
				foreach (var i in ExtraBreadcrumbItems)
					MainBreadcrumbItems.Add(i);
				ExtraBreadcrumbItems.Clear();
				Navigate(CurrentExtraPage, 0);
				CurrentExtraPage = null;
				ContentExtra.Navigate(typeof(Empty));
			});
		}
		else
		{
			if (MainBreadcrumbItems.Count > 1)
			{
				_ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
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
				});
			}
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

}
