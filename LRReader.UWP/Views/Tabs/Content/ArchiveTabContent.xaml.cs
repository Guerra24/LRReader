using CommunityToolkit.WinUI.Animations;
using LRReader.Shared.Extensions;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using LRReader.UWP.Extensions;
using LRReader.UWP.Views.Items;
using Microsoft.Extensions.DependencyInjection;
using Windows.Devices.Input;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Animation;
using RefreshContainer = Microsoft.UI.Xaml.Controls.RefreshContainer;

namespace LRReader.UWP.Views.Tabs.Content;

public sealed partial class ArchiveTabContent : UserControl
{
	private static AnimationBuilder FadeIn = AnimationBuilder.Create().Opacity(to: 1, duration: TimeSpan.FromMilliseconds(200), easingMode: EasingMode.EaseIn);
	private static AnimationBuilder FadeOut = AnimationBuilder.Create().Opacity(to: 0, duration: TimeSpan.FromMilliseconds(200), easingMode: EasingMode.EaseIn);

	public ArchiveTabContent()
	{
		this.InitializeComponent();
		ReaderBackground.SetVisualOpacity(0);
		ScrollViewer.SetVisualOpacity(0);
		/*
		ElementCompositionPreview.SetIsTranslationEnabled(ReaderThumbnailOverlay, true);
		ElementCompositionPreview.GetElementVisual(ReaderThumbnailOverlay).Properties.InsertVector3("Translation", new Vector3(0, 317, 0));
		*/
		ScrollViewer.AddHandler(PointerReleasedEvent, new PointerEventHandler(ScrollViewer_PointerRelease), true);

		Data = Service.Services.GetRequiredService<ArchivePageViewModel>();
		Data.ZoomChangedEvent += FitImages;
		Data.RebuildReader += RebuildReader;

		_loadSemaphore.Wait();

		Service.Events.RebuildReaderImagesSetEvent += RebuildReader;
	}

	public async void CloseReader()
	{
		if (_transition)
			return;
		_transition = true;
		if (RefreshContainer.Visibility == Visibility.Collapsed)
		{
			RefreshContainer.Visibility = Visibility.Visible;
			RefreshContainer.UpdateLayout();
			await Task.Delay(100); // Otherwise scrollings into view breaks
		}

		await PlayStop(false);
		ConnectedAnimation? animLeft = null, animRight = null;

		if (!Data.UseVerticalReader)
		{
			ReaderImage.disableAnimation = true;

			if (Animate)
			{
				var left = ReaderImage.LeftImage;
				var right = ReaderImage.RightImage;
				if (Data.ReaderContent.LeftImage != null && !(left.ActualWidth == 0 || left.ActualHeight == 0))
				{
					animLeft = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("closeL", left);
					animLeft.Configuration = new BasicConnectedAnimationConfiguration();
				}
				if (Data.ReaderContent.RightImage != null && !(right.ActualWidth == 0 || right.ActualHeight == 0))
				{
					animRight = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("closeR", right);
					animRight.Configuration = new BasicConnectedAnimationConfiguration();
				}
			}
		}

		_focus = false;
		int currentPage = Data.ReaderContent.Page;
		int count = Data.Pages;

		int leftTarget = currentPage;
		int rightTarget = currentPage;

		if (Data.ReaderContent.TwoPages)
		{
			leftTarget--;
			if (Data.ReadRTL)
			{
				int tmp = leftTarget;
				leftTarget = rightTarget;
				rightTarget = tmp;
			}
		}
		leftTarget = leftTarget.Clamp(0, count - 1);
		rightTarget = rightTarget.Clamp(0, count - 1);
		var delay = ImagesGrid.ContainerFromIndex(leftTarget) == null ? 200 : 50; // Man
		await ImagesGrid.SmoothScrollIntoViewWithIndexAsync(leftTarget, disableAnimation: true);
		await Task.Delay(delay);
		if (Animate)
		{
			var leftThumb = ImagesGrid.ContainerFromIndex(leftTarget)?.FindDescendant("Thumbnail");
			var rightThumb = ImagesGrid.ContainerFromIndex(rightTarget)?.FindDescendant("Thumbnail");
			if (Data.ReaderContent.LeftImage != null && leftThumb != null && Data.ArchiveImages.Count > leftTarget)
				animLeft?.TryStart(leftThumb);
			if (Data.ReaderContent.RightImage != null && rightThumb != null && Data.ArchiveImages.Count > rightTarget)
				animRight?.TryStart(rightThumb);
			await Task.WhenAll(FadeOut.StartAsync(ReaderBackground), FadeOut.StartAsync(ScrollViewer));
			await Task.Delay(200); // Give it a sec
		}
		else
		{
			ReaderBackground.SetVisualOpacity(0);
			ScrollViewer.SetVisualOpacity(0);
		}
		Data.ShowReader = false;

		_wasNew = await Data.SaveReaderData(_wasNew);

		_transition = false;
		_open = false;
		gcCounter = 0;
		Data.PageCounter = 0;
	}

	private async void Random_Clicked() => await Random((CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Shift) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down);

	private async void Next_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
	{
		args.Handled = true;
		await NextArchiveAsync();
	}
	private async void Prev_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
	{
		args.Handled = true;
		await PrevArchiveAsync();
	}

	private void Close_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
	{
		if (!Data.ShowReader)
			return;
		args.Handled = true;
		CloseReader();
	}

	private void ReaderControl_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
	{
		if (_changingPage)
			return;
		var pointerPoint = e.GetCurrentPoint(ScrollViewer);
		if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
		{
			var delta = pointerPoint.Properties.MouseWheelDelta;
			if (e.KeyModifiers == VirtualKeyModifiers.Control || pointerPoint.Properties.IsRightButtonPressed)
			{
				e.Handled = true;
				Data.ZoomValue = Math.Clamp(Data.ZoomValue + (int)(delta * 0.1), Data.UseVerticalReader ? 50 : 100, 400);
			}
			else if (e.KeyModifiers == VirtualKeyModifiers.None)
			{
				if (Math.Ceiling(ScrollViewer.VerticalOffset) >= ScrollViewer.ScrollableHeight && delta < 0 && Service.Settings.ScrollToChangePage)
				{
					e.Handled = true;
					NextPage(true);
				}
				else if (Math.Floor(ScrollViewer.VerticalOffset) <= 0 && delta > 0 && Service.Settings.ScrollToChangePage)
				{
					e.Handled = true;
					PrevPage(true);
				}
			}
		}
	}

	private void ReaderControl_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
	{
		e.Handled = true;
		double vertical = ScrollViewer.VerticalOffset;
		double horizontal = ScrollViewer.HorizontalOffset;
		ScrollViewer.ChangeView(horizontal - e.Delta.Translation.X, vertical - e.Delta.Translation.Y, null, true);
	}

	private void FitImages(bool disableAnim = false, bool force = false)
	{
		if (ReaderControl.ActualWidth == 0 || ReaderControl.ActualHeight == 0)
			return;
		float zoomFactor;
		if (Data.UseVerticalReader)
		{
			if (_fitAgainstFixedWidth == 0)
				_fitAgainstFixedWidth = ReaderControl.ActualWidth;
			zoomFactor = (float)(ScrollViewer.ViewportWidth / _fitAgainstFixedWidth);
		}
		else if (Data.FitToWidth)
		{
			zoomFactor = (float)Math.Min(ScrollViewer.ViewportWidth / ReaderControl.ActualWidth, Data.FitScaleLimit * 0.01);
		}
		else
		{
			zoomFactor = (float)Math.Min(ScrollViewer.ViewportWidth / ReaderControl.ActualWidth, ScrollViewer.ViewportHeight / ReaderControl.ActualHeight);
		}
		var zoom = zoomFactor * (Data.ZoomValue * 0.01f);
		if (zoom != _lastZoom || force)
		{
			_lastZoom = zoom;
			var yOffset = ScrollViewer.VerticalOffset / ScrollViewer.ZoomFactor * zoom;
			ScrollViewer.ChangeView(null, yOffset, zoom, disableAnim || !Animate);
		}
	}

	private async void Refresh_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
	{
		await Data.Reload();
		args.Handled = true;
	}

	private void ImagesGrid_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
	{
		if (!args.InRecycleQueue && args.ItemContainer.ContentTemplateRoot is ArchiveImage item)
		{
			item.Phase0();
			args.RegisterUpdateCallback(Phase1);
		}
		args.Handled = true;
	}

	private void Phase1(ListViewBase sender, ContainerContentChangingEventArgs args)
	{
		if (!args.InRecycleQueue && args.ItemContainer.ContentTemplateRoot is ArchiveImage item)
		{
			item.Phase1((ImagePageSet)args.Item);
			args.RegisterUpdateCallback(Phase2);
		}
	}

	private void Phase2(ListViewBase sender, ContainerContentChangingEventArgs args)
	{
		if (!args.InRecycleQueue && args.ItemContainer.ContentTemplateRoot is ArchiveImage item)
		{
			item.Phase2();
			args.RegisterUpdateCallback(Phase3);
		}
	}

	private void Phase3(ListViewBase sender, ContainerContentChangingEventArgs args)
	{
		if (!args.InRecycleQueue && args.ItemContainer.ContentTemplateRoot is ArchiveImage item)
			item.Phase3();
	}

	private async void OverlayThumbnails_ItemClick(object sender, ItemClickEventArgs e)
	{
		var readerSet = Data.ArchiveImagesReader.FirstOrDefault(s => s.Page >= Data.ArchiveImages.IndexOf((ImagePageSet)e.ClickedItem));
		if (readerSet == null)
			return;

		int index = Data.ArchiveImagesReader.IndexOf(readerSet);

		if (Data.UseVerticalReader)
		{
			await Task.Delay(100);
			var element = ReaderVertical.GetOrCreateElement(index);
			element.UpdateLayout();
			element.StartBringIntoView(new BringIntoViewOptions { AnimationDesired = true, VerticalAlignmentRatio = 0f });
		}
		else
		{
			_changingPage = true;

			Data.ReaderIndex = index;
			await ReaderImage.FadeOutPage();
			ScrollViewer.ChangeView(null, 0, null, true);
			await ChangePage();

			_changingPage = false;
		}
	}

}
