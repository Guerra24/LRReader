using LRReader.Shared.ViewModels.Tools;
using LRReader.UWP.Extensions;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Numerics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace LRReader.UWP.Views.Content.Tools
{
	public sealed partial class Deduplicator : Page
	{
		private static AnimationBuilder FadeIn = AnimationBuilder.Create().Opacity(to: 1, duration: TimeSpan.FromMilliseconds(200), easingMode: EasingMode.EaseIn);
		private static AnimationBuilder FadeOut = AnimationBuilder.Create().Opacity(to: 0, duration: TimeSpan.FromMilliseconds(200), easingMode: EasingMode.EaseOut);

		private DeduplicatorToolViewModel Data;

		public Deduplicator()
		{
			this.InitializeComponent();
			Data = DataContext as DeduplicatorToolViewModel;
			for (int i = Environment.ProcessorCount; i > 0; i--)
				WorkerThreads.Items.Add(i);
			Details.SetVisualOpacity(0);
		}

		private void Help_Click(object sender, RoutedEventArgs e)
		{
			HowItWorks.IsOpen = true;
		}

		private void GridView_ItemClick(object sender, ItemClickEventArgs e)
		{
			Details.Visibility = Visibility.Visible;
			FadeIn.Start(Details);
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			await FadeOut.StartAsync(Details);
			Details.Visibility = Visibility.Collapsed;
		}
	}
}
