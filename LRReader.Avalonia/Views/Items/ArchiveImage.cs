using Avalonia.Animation.Easings;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Rendering.Composition;
using LRReader.Avalonia.Extensions;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;

namespace LRReader.Avalonia.Views.Items
{
	[PseudoClasses(":hidden")]
	public sealed partial class ArchiveImage : TemplatedControl
	{
		//private static AnimationBuilder FadeIn = AnimationBuilder.Create().Opacity(to: 1, duration: TimeSpan.FromMilliseconds(150), easingMode: EasingMode.EaseIn);
		//private static AnimationBuilder FadeOut = AnimationBuilder.Create().Opacity(to: 0, duration: TimeSpan.FromMilliseconds(150), easingMode: EasingMode.EaseOut);

		public ArchiveImageViewModel Data { get; }

		private Grid Root = null!;

		private bool _loaded;
		private TaskCompletionSource tcs = new();
		public ArchiveImage()
		{
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
				Root?.FadeInAsync(TimeSpan.FromMilliseconds(150), new QuadraticEaseIn()).ContinueWith(t => Dispatcher.Post(() => Root?.SetOpacity(1)));
			else
				Root?.SetOpacity(1);
			return Task.CompletedTask;
		}

		private async Task Hide(bool animate)
		{
			if (animate)
			{
				await Root.FadeOutAsync(TimeSpan.FromMilliseconds(150), new QuadraticEaseOut());
				Root.SetOpacity(0);
			}
			else
				Root.SetOpacity(0);
		}

		protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
		{
			// This is running AFTER context changed
			base.OnApplyTemplate(e);

			Root = e.NameScope.Get<Grid>("Root");

			PseudoClasses.Set(":hidden", !KeepOverlayOpen);

			if (!_loaded)
				tcs.SetResult();
			_loaded = true;
		}

		private async void UserControl_DataContextChanged(object? sender, EventArgs args)
		{
			if (!_loaded)
				await tcs.Task;
			if (DataContext is ImagePageSet set)
				await Data.LoadImage(set);
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

		private void UserControl_PointerEntered(object? sender, PointerEventArgs e)
		{
			if (KeepOverlayOpen || Data.HideOverlay)
				return;
			//VisualStateManager.GoToState(this, "Visible", true);
		}

		private void UserControl_PointerExited(object? sender, PointerEventArgs e)
		{
			if (KeepOverlayOpen || Data.HideOverlay)
				return;
			//VisualStateManager.GoToState(this, "Hidden", true);
		}

		private void UserControl_PointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
		{
			if (KeepOverlayOpen || Data.HideOverlay)
				return;
			//VisualStateManager.GoToState(this, "Hidden", true);
		}

		public bool KeepOverlayOpen
		{
			get => GetValue(KeepOverlayOpenProperty);
			set
			{
				SetValue(KeepOverlayOpenProperty, value);
				PseudoClasses.Set(":hidden", !value);
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

		public static readonly StyledProperty<bool> KeepOverlayOpenProperty = AvaloniaProperty.Register<ArchiveImage, bool>("KeepOverlayOpen");
	}

}
