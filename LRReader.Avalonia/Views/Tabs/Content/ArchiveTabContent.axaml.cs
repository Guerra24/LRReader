using Avalonia.Animation.Easings;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls.Experimental;
using LRReader.Avalonia.Extensions;
using LRReader.Avalonia.Views.Controls;
using LRReader.Avalonia.Views.Items;
using LRReader.Shared.Extensions;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LRReader.Avalonia.Views.Tabs.Content;

public partial class ArchiveTabContent : UserControl
{

	private Action<int> _resizer;
	private Action<int, ReaderImage, int> _resizerVertical;

	public ArchiveTabContent()
	{
		InitializeComponent();
		ReaderBackground.SetOpacity(0);
		ScrollViewer.SetOpacity(0);
		/*
		ElementCompositionPreview.SetIsTranslationEnabled(ReaderThumbnailOverlay, true);
		ElementCompositionPreview.GetElementVisual(ReaderThumbnailOverlay).Properties.InsertVector3("Translation", new Vector3(0, 317, 0));
		*/
		ScrollViewer.AddHandler(PointerReleasedEvent, ScrollViewer_PointerRelease);

		DataContext = Data = Service.Services.GetRequiredService<ArchivePageViewModel>();
		Data.ZoomChangedEvent += FitImages;
		Data.RebuildReader += RebuildReader;

		_loadSemaphore.Wait();

		Service.Events.RebuildReaderImagesSetEvent += RebuildReader;

		Action<int> resizer = (height) =>
		{
			Service.Dispatcher.Run(async () =>
			{
				await ReaderImage.ResizeHeight(height);
			});
		};

		Action<int, ReaderImage, int> resizerVertical = (width, image, index) =>
		{
			Service.Dispatcher.Run(async () =>
			{
				await image.ResizeWidth(width);
				if (ReaderVertical.TryGetElement(index - 1) is ReaderImage imgminus1)
					await imgminus1.ResizeWidth(width);
				if (ReaderVertical.TryGetElement(index + 1) is ReaderImage img1)
					await img1.ResizeWidth(width);
				if (ReaderVertical.TryGetElement(index + 2) is ReaderImage img2)
					await img2.ResizeWidth(width);
			});
		};

		_resizer = resizer.Debounce(500);
		_resizerVertical = resizerVertical.Debounce(500);
	}

	public async void CloseReader()
	{
		if (_transition)
			return;
		_transition = true;
		if (!StackRoot.IsVisible)
		{
			StackRoot.IsVisible = true;
			StackRoot.UpdateLayout();
			await Task.Delay(100); // Otherwise scrollings into view breaks
		}

		await PlayStop(false);
		FAConnectedAnimation? animLeft = null, animRight = null;

		if (!Data.UseVerticalReader)
		{
			ReaderImage.disableAnimation = true;

			if (Animate)
			{
				/*var left = ReaderImage.LeftImage;
				var right = ReaderImage.RightImage;
				if (Data.ReaderContent.LeftImage != null && !(left.Bounds.Width == 0 || left.Bounds.Height == 0))
				{
					animLeft = FAConnectedAnimationService.GetForView(TopLevel.GetTopLevel(this)).PrepareToAnimate("closeL", left);
					animLeft.Configuration = new FADirectConnectedAnimationConfiguration();
				}
				if (Data.ReaderContent.RightImage != null && !(right.Bounds.Width == 0 || right.Bounds.Height == 0))
				{
					animRight = FAConnectedAnimationService.GetForView(TopLevel.GetTopLevel(this)).PrepareToAnimate("closeR", right);
					animRight.Configuration = new FADirectConnectedAnimationConfiguration();
				}*/
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
		//ImagesGrid.GetOrCreateElement(leftTarget).BringIntoView();
		//await ImagesGrid.SmoothScrollIntoViewWithIndexAsync(leftTarget, disableAnimation: true);
		await Task.Delay(50);
		if (Animate)
		{
			var leftThumb = ImagesGrid.TryGetElement(leftTarget).FindDescendantOfType<VirtualImage>();
			var rightThumb = ImagesGrid.TryGetElement(rightTarget).FindDescendantOfType<VirtualImage>();
			if (Data.ReaderContent.LeftImage != null && leftThumb != null && Data.ArchiveImages.Count > leftTarget)
				animLeft?.TryStart(leftThumb);
			if (Data.ReaderContent.RightImage != null && rightThumb != null && Data.ArchiveImages.Count > rightTarget)
				animRight?.TryStart(rightThumb);
			await Task.WhenAll(ReaderBackground.FadeOutAsync(TimeSpan.FromMilliseconds(200), new QuadraticEaseIn()), ScrollViewer.FadeOutAsync(TimeSpan.FromMilliseconds(200), new QuadraticEaseIn()));
			ReaderBackground.SetOpacity(0);
			ScrollViewer.SetOpacity(0);
			await Task.Delay(200); // Give it a sec
		}
		else
		{
			ReaderBackground.SetOpacity(0);
			ScrollViewer.SetOpacity(0);
		}
		Data.ShowReader = false;

		_wasNew = await Data.SaveReaderData(_wasNew);

		_transition = false;
		_open = false;
		gcCounter = 0;
		Data.PageCounter = 0;
	}

	private async void Random_Clicked(object? sender, RoutedEventArgs e) => await Random(false/*(CoreWindow.GetForCurrentThread().GetKeyState(Key.Shift) & CoreKeyStates.Down) == CoreKeyStates.Down*/);

	private void ReaderControl_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
	{
		if (_changingPage)
			return;
		var pointerPoint = e.GetCurrentPoint(ScrollViewer);
		if (e.Pointer.Type == PointerType.Mouse)
		{
			var delta = e.Delta.Y * 120; // UWP's delta is 120, Avalonia is 1
			if (e.KeyModifiers == KeyModifiers.Control || pointerPoint.Properties.IsRightButtonPressed)
			{
				e.Handled = true;
				Data.ZoomValue = Math.Clamp(Data.ZoomValue + (int)(delta * 0.1), Data.UseVerticalReader ? 50 : 100, 400);
			}
			else if (e.KeyModifiers == KeyModifiers.None)
			{
				if (Math.Ceiling(ScrollViewer.Offset.Y) >= ScrollViewer.Extent.Height - ScrollViewer.Viewport.Height && delta < 0 && Service.Settings.ScrollToChangePage)
				{
					e.Handled = true;
					NextPage(true);
				}
				else if (Math.Floor(ScrollViewer.Offset.Y) <= 0 && delta > 0 && Service.Settings.ScrollToChangePage)
				{
					e.Handled = true;
					PrevPage(true);
				}
			}
		}
	}

	private void FitImages(bool disableAnim = false, bool force = false)
	{
		if (ReaderControl.Bounds.Width == 0 || ReaderControl.Bounds.Height == 0)
			return;
		float zoomFactor;
		if (Data.UseVerticalReader)
		{
			if (_fitAgainstFixedWidth == 0)
				_fitAgainstFixedWidth = ReaderControl.Bounds.Width;
			zoomFactor = (float)(ScrollViewer.Viewport.Width / _fitAgainstFixedWidth);
		}
		else if (Data.FitToWidth)
		{
			zoomFactor = (float)Math.Min(ScrollViewer.Viewport.Width / ReaderControl.Bounds.Width, Data.FitScaleLimit * 0.01);
		}
		else
		{
			zoomFactor = (float)Math.Min(ScrollViewer.Viewport.Width / ReaderControl.Bounds.Width, ScrollViewer.Viewport.Height / ReaderControl.Bounds.Height);
		}
		var zoom = zoomFactor * (Data.ZoomValue * 0.01f);
		if (zoom != _lastZoom || force)
		{
			_lastZoom = zoom;
			var yOffset = ScrollViewer.Offset.Y / Data.ZoomFactor * zoom;
			Data.ZoomFactor = zoom;
			if (!Data.UseVerticalReader)
				ScrollViewer.Offset = new Point(ScrollViewer.Offset.X, yOffset);
		}
	}

	public void RedrawReader()
	{
		Dispatcher.Post(async () =>
		{
			await Task.Yield();
			ReaderBackground.InvalidateVisual();
			ScrollViewer.InvalidateVisual();
		});
	}

}