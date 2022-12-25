#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Items;
using LRReader.UWP.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace LRReader.UWP.Views.Items
{
	public sealed class ArchiveImage : Control
	{
		private static AnimationBuilder FadeIn = AnimationBuilder.Create().Opacity(to: 1, duration: TimeSpan.FromMilliseconds(150), easingMode: EasingMode.EaseIn);
		private static AnimationBuilder FadeOut = AnimationBuilder.Create().Opacity(to: 0, duration: TimeSpan.FromMilliseconds(150), easingMode: EasingMode.EaseOut);

		public ArchiveImageViewModel Data { get; }

		private Grid Root = null!;

		public ArchiveImage()
		{
			this.DefaultStyleKey = typeof(ArchiveImage);
			// TODO: Proper fix
			Data = Service.Services.GetRequiredService<ArchiveImageViewModel>();
			Data.Show += Show;
			Data.Hide += Hide;
			DataContextChanged += UserControl_DataContextChanged;
			PointerEntered += UserControl_PointerEntered;
			PointerExited += UserControl_PointerExited;
			PointerCaptureLost += UserControl_PointerCaptureLost;
		}

		private Task Show(bool animate)
		{
			if (animate)
				Root.Start(FadeIn);
			else
				Root.SetVisualOpacity(1);
			return Task.CompletedTask;
		}

		private async Task Hide(bool animate)
		{
			if (animate)
				await Root.StartAsync(FadeOut);
			else
				Root.SetVisualOpacity(0);
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			Root = (Grid)GetTemplateChild("Root");
			if (!KeepOverlayOpen)
				VisualStateManager.GoToState(this, "Hidden", false);
		}

		private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (args.NewValue == null)
				return;
			await Data.LoadImage((ImagePageSet)args.NewValue);
		}

		public async void Phase0()
		{
			await Data.Phase0();
		}

		public void Phase1(ImagePageSet set)
		{
			Data.Phase1(set);
		}

		public async void Phase2()
		{
			await Data.Phase2();
		}
		public async void Phase3()
		{
			await Data.Phase3();
		}

		private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e)
		{
			if (KeepOverlayOpen || Data.HideOverlay)
				return;
			VisualStateManager.GoToState(this, "Visible", true);
		}

		private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
		{
			if (KeepOverlayOpen || Data.HideOverlay)
				return;
			VisualStateManager.GoToState(this, "Hidden", true);
		}

		private void UserControl_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
		{
			if (KeepOverlayOpen || Data.HideOverlay)
				return;
			VisualStateManager.GoToState(this, "Hidden", true);
		}

		public bool KeepOverlayOpen
		{
			get => (bool)GetValue(KeepOverlayOpenProperty);
			set
			{
				if (value)
					VisualStateManager.GoToState(this, "Visible", false);
				else
					VisualStateManager.GoToState(this, "Hidden", false);
				SetValue(KeepOverlayOpenProperty, value);
			}
		}

		public bool HideOverlay
		{
			set => Data.HideOverlay = value;
		}

		public bool ShowExtraDetails
		{
			set => Data.ShowExtraDetails = value;
		}

		public static readonly DependencyProperty KeepOverlayOpenProperty = DependencyProperty.Register("KeepOverlayOpen", typeof(bool), typeof(ArchiveImage), new PropertyMetadata(false));

	}

}
