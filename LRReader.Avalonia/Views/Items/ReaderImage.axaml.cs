using Avalonia.Animation.Easings;
using FluentAvalonia.UI.Controls.Experimental;
using LRReader.Avalonia.Extensions;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;

namespace LRReader.Avalonia.Views.Items;

public partial class ReaderImage : UserControl
{

	//private static AnimationBuilder FadeIn = AnimationBuilder.Create().Opacity(to: 1, duration: TimeSpan.FromMilliseconds(80), easingMode: EasingMode.EaseIn);
	//private static AnimationBuilder FadeOut = AnimationBuilder.Create().Opacity(to: 0, duration: TimeSpan.FromMilliseconds(80), easingMode: EasingMode.EaseOut);

	public bool disableAnimation = true;
	private int _height, _width;
	private ImageProcessingService imageProcessing = Service.ImageProcessing;

	private SemaphoreSlim decodePixel = new SemaphoreSlim(1);

	public ReaderImage()
	{
		InitializeComponent();
	}

	public async Task ChangePage(ReaderImageSet set)
	{
		await decodePixel.WaitAsync();
		var images = await Task.WhenAll(Service.Images.GetImageCached(set.LeftImage), Service.Images.GetImageCached(set.RightImage));
		/*var imageBitmaps = */await Task.WhenAll(imageProcessing.ByteToBitmap(images[0], _width, _height, LeftImage), imageProcessing.ByteToBitmap(images[1], _width, _height, RightImage));
		//LeftImage.Source = imageBitmaps[0] as Bitmap;
		//RightImage.Source = imageBitmaps[1] as Bitmap;
		var sizes = await Task.WhenAll(Service.Images.GetImageSizeCached(set.LeftImage), Service.Images.GetImageSizeCached(set.RightImage));
		var size = new Size(Math.Max(sizes[0].Width, sizes[1].Width), Math.Max(sizes[0].Height, sizes[1].Height));
		LeftImage.Height = RightImage.Height = 0;
		LeftImage.Width = RightImage.Width = 0;

		if (LeftImage.Source != null)
		{
			var aspect0 = (float)sizes[0].Width / (float)sizes[0].Height;
			LeftImage.Width = size.Height * aspect0;
			LeftImage.Height = size.Height;
		}
		if (RightImage.Source != null)
		{
			var aspect1 = (float)sizes[1].Width / (float)sizes[1].Height;
			RightImage.Width = size.Height * aspect1;
			RightImage.Height = size.Height;
		}
		decodePixel.Release();
	}

	public async Task FadeOutPage()
	{
		if (!(Service.Platform.AnimationsEnabled && Service.Settings.ReaderAnimations && Service.Settings.PageChangeAnimation))
			return;
		if (disableAnimation)
		{
			ImagesRoot.SetVisualOpacity(0);
			disableAnimation = false;
		}
		else
		{
			await ImagesRoot.FadeOutAsync(TimeSpan.FromMilliseconds(80), new QuadraticEaseOut());
		}
	}

	public void FadeInPage()
	{
		if (!(Service.Platform.AnimationsEnabled && Service.Settings.ReaderAnimations))
			return;
		var openLeft = FAConnectedAnimationService.GetForView(TopLevel.GetTopLevel(this)).GetAnimation("openL");
		var openRight = FAConnectedAnimationService.GetForView(TopLevel.GetTopLevel(this)).GetAnimation("openR");
		if (openLeft != null || openRight != null || !Service.Settings.PageChangeAnimation)
			ImagesRoot.SetVisualOpacity(1);
		else
			ImagesRoot.FadeIn(TimeSpan.FromMilliseconds(80), new QuadraticEaseIn());
		openLeft?.TryStart(LeftImage);
		openRight?.TryStart(RightImage);
	}

	public async Task ResizeHeight(int height)
	{
		height = (int)Math.Round(height * 2.0);
		if (_height == height)
			return;
		await decodePixel.WaitAsync();
		_height = height;
		if (LeftImage.Source != null)
			LeftImage.DecodePixelHeight = height;
		if (RightImage.Source != null)
			RightImage.DecodePixelHeight = height;
		decodePixel.Release();
	}

	public async Task ResizeWidth(int width)
	{
		width = (int)Math.Round(width * 2.0);
		if (_width == width)
			return;
		await decodePixel.WaitAsync();
		_width = width;
		if (LeftImage.Source != null)
			LeftImage.DecodePixelWidth = width;
		if (RightImage.Source != null)
			RightImage.DecodePixelWidth = width;
		decodePixel.Release();
	}

	private async void UserControl_DataContextChanged(object? sender, EventArgs e)
	{
		if (DataContext is ReaderImageSet set)
			await ChangePage(set);
	}
}