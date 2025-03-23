using CommunityToolkit.WinUI.Animations;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Tools;
using LRReader.UWP.Extensions;
using LRReader.UWP.Views.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace LRReader.UWP.Views.Content.Tools
{
	public sealed partial class Deduplicator : ModernBasePage
	{
		private static AnimationBuilder FadeIn = AnimationBuilder.Create().Opacity(to: 1, duration: TimeSpan.FromMilliseconds(200), easingMode: EasingMode.EaseIn);
		private static AnimationBuilder FadeOut = AnimationBuilder.Create().Opacity(to: 0, duration: TimeSpan.FromMilliseconds(200), easingMode: EasingMode.EaseOut);

		private DeduplicatorToolViewModel Data;

		private ScrollViewer? LeftScroller, RightScroller;

		private int _state = 0;

		public Deduplicator()
		{
			this.InitializeComponent();
			Data = Service.Services.GetRequiredService<DeduplicatorToolViewModel>();
			for (int i = Environment.ProcessorCount; i > 0; i--)
				WorkerThreads.Items.Add(i);
			Details.SetVisualOpacity(0);
		}

		private void RightScroller_ViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
		{
			if (!e.IsIntermediate)
			{
				_state = 0;
				return;
			}
			if (_state != 2)
			{
				LeftScroller?.ChangeView(null, RightScroller?.VerticalOffset, null, true);
				_state = 1;
			}
		}

		private void LeftScroller_ViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
		{
			if (!e.IsIntermediate)
			{
				_state = 0;
				return;
			}
			if (_state != 1)
			{
				RightScroller?.ChangeView(null, LeftScroller?.VerticalOffset, null, true);
				_state = 2;
			}
		}

		private async void GridView_ItemClick(object sender, ItemClickEventArgs e)
		{
			await FadeOut.StartAsync(Results);
			var item = (ArchiveHit)e.ClickedItem;
			await Data.LoadArchives(item);
			LeftPages.SetVisualOpacity(0);
			RightPages.SetVisualOpacity(0);
			Details.Visibility = Visibility.Visible;
			await FadeIn.StartAsync(Details);
			if (RightScroller == null && LeftScroller == null)
			{
				var border = VisualTreeHelper.GetChild(RightPages, 0);
				if (border != null)
					RightScroller = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
				if (RightScroller != null)
					RightScroller.ViewChanged += RightScroller_ViewChanged;

				border = VisualTreeHelper.GetChild(LeftPages, 0);
				if (border != null)
					LeftScroller = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
				if (LeftScroller != null)
					LeftScroller.ViewChanged += LeftScroller_ViewChanged;
			}
			FadeIn.Start(LeftPages);
			FadeIn.Start(RightPages);
		}

		private void Flyout_Closing(FlyoutBase sender, FlyoutBaseClosingEventArgs e) => e.Cancel = !Service.Platform.Active;

		private async void CloseButton_Click(object? sender, RoutedEventArgs e)
		{
			LeftFlyout.Hide();
			RightFlyout.Hide();
			LeftFlyoutN.Hide();
			RightFlyoutN.Hide();
			await FadeOut.StartAsync(Details);
			Details.Visibility = Visibility.Collapsed;
			FadeIn.Start(Results);
		}
	}
}
