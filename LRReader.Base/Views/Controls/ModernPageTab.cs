using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

#if WINDOWS_UWP
using FABreadcrumbBar = Microsoft.UI.Xaml.Controls.BreadcrumbBar;
using FABreadcrumbBarItemClickedEventArgs = Microsoft.UI.Xaml.Controls.BreadcrumbBarItemClickedEventArgs;
using TwoPaneViewMode = Microsoft.UI.Xaml.Controls.TwoPaneViewMode;

namespace LRReader.UWP.Views.Controls;
#else
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;

namespace LRReader.Avalonia.Views.Controls;
#endif

public partial class ModernPageTab : UserControl, IDisposable
{
	public ObservableCollection<ModernPageTabItem> MainBreadcrumbItems { get; } = [];
	private ObservableCollection<ModernPageTabItem> ExtraBreadcrumbItems = [];
	private ModernPageTabItem? CurrentMainPage;
#if WINDOWS_UWP
	private ModernPageTabItem? CurrentExtraPage;
#endif

	private bool _loaded;

	public ModernPageTab()
	{
		InitializeComponent();
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
#if WINDOWS_UWP
			ContentMain.Navigate(MainBreadcrumbItems.Last().Page, new ModernPageTabWrapper(this, CurrentMainPage.Parameter));
#else
			ContentMain.Navigate(MainBreadcrumbItems.Last().Page, new ModernPageTabWrapper(this, CurrentMainPage.Parameter), new FASlideNavigationTransitionInfo { Effect = FASlideNavigationTransitionEffect.FromLeft, FromHorizontalOffset = 150 });
#endif
			return true;
		}
		return false;
	}

	public bool GoBackExtra()
	{
		if (ExtraBreadcrumbItems.Count > 1)
		{
			ExtraBreadcrumbItems.Remove(ExtraBreadcrumbItems.Last());
#if WINDOWS_UWP
			CurrentExtraPage = ExtraBreadcrumbItems.Last();
			ContentExtra.Navigate(ExtraBreadcrumbItems.Last().Page, new ModernPageTabWrapper(this, CurrentExtraPage.Parameter));
#endif
			return true;
		}
		return false;
	}

	public void Navigate(ModernPageTabItem item, int framesource)
	{
		// 0 main
		// 1 extra
		// check origin frame and prevent stacking when main opens an extra page, replace it
#if WINDOWS_UWP
		if (CurrentMainPage == null || TwoPane.Mode == TwoPaneViewMode.SinglePane)
#endif
		{
			CurrentMainPage = item;
			MainBreadcrumbItems.Add(item);
#if WINDOWS_UWP
			ContentMain.Navigate(item.Page, new ModernPageTabWrapper(this, item.Parameter));
#else
			ContentMain.Navigate(item.Page, new ModernPageTabWrapper(this, item.Parameter), new FASlideNavigationTransitionInfo { Effect = FASlideNavigationTransitionEffect.FromRight, FromHorizontalOffset = 150 });
#endif
		}
#if WINDOWS_UWP
		else if (TwoPane.Mode == TwoPaneViewMode.Wide && !item.Equals(CurrentExtraPage))
		{
			CurrentExtraPage = item;
			if (framesource == 0)
				ExtraBreadcrumbItems.Clear();
			ExtraBreadcrumbItems.Add(item);
			ContentExtra.Navigate(item.Page, new ModernPageTabWrapper(this, item.Parameter));
		}
#endif
		//, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight }
	}

	private void MainBreadcrumb_ItemClicked(FABreadcrumbBar sender, FABreadcrumbBarItemClickedEventArgs args)
	{
		if (args.Index < MainBreadcrumbItems.Count - 1)
		{
			for (int i = MainBreadcrumbItems.Count - 1; i > args.Index; i--)
			{
				MainBreadcrumbItems.RemoveAt(i);
			}
			var item = (ModernPageTabItem)args.Item;
#if WINDOWS_UWP
			ContentMain.Navigate(item.Page, new ModernPageTabWrapper(this, item.Parameter));
#else
			ContentMain.Navigate(item.Page, new ModernPageTabWrapper(this, item.Parameter), new FASlideNavigationTransitionInfo { Effect = FASlideNavigationTransitionEffect.FromLeft, FromHorizontalOffset = 150 });
#endif
		}
	}

	public void Dispose()
	{
		if (ContentMain.Content is IDisposable main) main.Dispose();
#if WINDOWS_UWP
		if (ContentExtra.Content is IDisposable extra) extra.Dispose();
#endif
	}

#if !WINDOWS_UWP
	// Compat
	private void GoBackMain(object? sender, RoutedEventArgs e)
	{
		GoBackMain();
	}
#endif

}

public class ModernPageTabItem
{
	public string Title { get; set; } = null!;
	public string Description { get; set; } = null!;

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
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