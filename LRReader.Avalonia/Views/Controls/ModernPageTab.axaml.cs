using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using System.Collections.ObjectModel;

namespace LRReader.Avalonia.Views.Controls;

public partial class ModernPageTab : UserControl, IDisposable
{
	public ObservableCollection<ModernPageTabItem> MainBreadcrumbItems { get; } = [];
	private ObservableCollection<ModernPageTabItem> ExtraBreadcrumbItems = [];
	//private ModernPageTabItem? CurrentExtraPage;
	private ModernPageTabItem? CurrentMainPage;

	private bool _loaded;

	public ModernPageTab()
	{
		InitializeComponent();
	}

	public string? Title
	{
		get => GetValue(TitleProperty);
		set => SetValue(TitleProperty, value);
	}

	public Type? Initial
	{
		get => GetValue(InitialProperty);
		set => SetValue(InitialProperty, value);
	}

	private void UserControl_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
	{
		if (Design.IsDesignMode)
			return;

		if (_loaded)
			return;
		_loaded = true;
		if (Initial != null)
			Navigate(new ModernPageTabItem { Title = Title!, Page = Initial }, 0);
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
			ContentMain.Navigate(MainBreadcrumbItems.Last().Page, new ModernPageTabWrapper(this, CurrentMainPage.Parameter), new FASlideNavigationTransitionInfo { Effect = FASlideNavigationTransitionEffect.FromLeft, FromHorizontalOffset = 150 });
			return true;
		}
		return false;
	}

	public bool GoBackExtra()
	{
		if (ExtraBreadcrumbItems.Count > 1)
		{
			ExtraBreadcrumbItems.Remove(ExtraBreadcrumbItems.Last());
			//CurrentExtraPage = ExtraBreadcrumbItems.Last();
			//ContentExtra.Navigate(ExtraBreadcrumbItems.Last().Page, new ModernPageTabWrapper(this, CurrentExtraPage.Parameter));
			return true;
		}
		return false;
	}

	public void Navigate(ModernPageTabItem item, int framesource)
	{
		// 0 main
		// 1 extra
		// check origin frame and prevent stacking when main opens an extra page, replace it
		//if (CurrentMainPage == null || TwoPane.Mode == TwoPaneViewMode.SinglePane)
		{
			CurrentMainPage = item;
			MainBreadcrumbItems.Add(item);
			ContentMain.Navigate(item.Page, new ModernPageTabWrapper(this, item.Parameter), new FASlideNavigationTransitionInfo { Effect = FASlideNavigationTransitionEffect.FromRight, FromHorizontalOffset = 150 });
		}
		/*else if (TwoPane.Mode == TwoPaneViewMode.Wide && !item.Equals(CurrentExtraPage))
		{
			CurrentExtraPage = item;
			if (framesource == 0)
				ExtraBreadcrumbItems.Clear();
			ExtraBreadcrumbItems.Add(item);
			ContentExtra.Navigate(item.Page, new ModernPageTabWrapper(this, item.Parameter));
		}*/
		//, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight }
	}

	/*private void TwoPane_ModeChanged(TwoPaneView sender, object args)
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
	}*/

	private void MainBreadcrumb_ItemClicked(FABreadcrumbBar sender, FABreadcrumbBarItemClickedEventArgs args)
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

	/*private void ExtraBreadcrumb_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
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
	}*/

	public void Dispose()
	{
		if (ContentMain.Content is IDisposable main) main.Dispose();
		//if (ContentExtra.Content is IDisposable extra) extra.Dispose();
	}

	// Compat
	private void GoBackMain(object? sender, RoutedEventArgs e)
	{
		GoBackMain();
	}

	public static readonly StyledProperty<string?> TitleProperty = AvaloniaProperty.Register<ModernPageTab, string?>("Title");
	public static readonly StyledProperty<Type?> InitialProperty = AvaloniaProperty.Register<ModernPageTab, Type?>("Initial");
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