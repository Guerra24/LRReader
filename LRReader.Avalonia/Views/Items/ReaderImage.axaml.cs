using Avalonia.Animation.Easings;
using FluentAvalonia.UI.Controls.Experimental;
using LRReader.Avalonia.Extensions;
using LRReader.Shared.Models.Main;
using static LRReader.Shared.Services.Service;

namespace LRReader.Avalonia.Views.Items;

public partial class ReaderImage : UserControl
{

	//private static readonly AnimationBuilder FadeIn = AnimationBuilder.Create().Opacity(to: 1, duration: TimeSpan.FromMilliseconds(80), easingMode: EasingMode.EaseIn);
	//private static readonly AnimationBuilder FadeOut = AnimationBuilder.Create().Opacity(to: 0, duration: TimeSpan.FromMilliseconds(80), easingMode: EasingMode.EaseOut);

	public bool disableAnimation = true;
	private int _height, _width;

	private readonly SemaphoreSlim decodePixel = new(1);
	private CancellationTokenSource Cts = new();

	public ReaderImage()
	{
		InitializeComponent();
	}

	public async Task ChangePage(ReaderImageSet set, CancellationToken cancellationToken = default)
	{
		await decodePixel.WaitAsync();
		try
		{
			if (cancellationToken.IsCancellationRequested)
				return;
			var images = await Task.WhenAll(Images.GetImageCached(set.LeftImage, cancellationToken: cancellationToken), Images.GetImageCached(set.RightImage, cancellationToken: cancellationToken));
			if (cancellationToken.IsCancellationRequested)
				return;
			/*var imageBitmaps = */
			await Task.WhenAll(ImageProcessing.ByteToBitmap(images[0], _width, _height, LeftImage, cancellationToken), ImageProcessing.ByteToBitmap(images[1], _width, _height, RightImage, cancellationToken));
			if (cancellationToken.IsCancellationRequested)
				return;
			//LeftImage.Source = imageBitmaps[0] as Bitmap;
			//RightImage.Source = imageBitmaps[1] as Bitmap;
			var sizes = await Task.WhenAll(Images.GetImageSizeCached(set.LeftImage, cancellationToken: cancellationToken), Images.GetImageSizeCached(set.RightImage, cancellationToken: cancellationToken));
			if (cancellationToken.IsCancellationRequested)
				return;
			var size = new Size(Math.Max(sizes[0].Width, sizes[1].Width), Math.Max(sizes[0].Height, sizes[1].Height));
			LeftImage.Height = RightImage.Height = 0;
			LeftImage.Width = RightImage.Width = 0;

			if (LeftImage.IsValid)
			{
				var aspect0 = (float)sizes[0].Width / (float)sizes[0].Height;
				var height = set.Height == 0 ? size.Height : set.Height;
				LeftImage.Width = Math.Round(height * aspect0);
				LeftImage.Height = height;
			}
			if (RightImage.IsValid)
			{
				var aspect1 = (float)sizes[1].Width / (float)sizes[1].Height;
				RightImage.Width = Math.Round(size.Height * aspect1);
				RightImage.Height = size.Height;
			}
		}
		finally
		{
			decodePixel.Release();
		}
	}

	public async Task FadeOutPage()
	{
		if (!(Platform.AnimationsEnabled && Settings.ReaderAnimations && Settings.PageChangeAnimation))
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
		if (!(Platform.AnimationsEnabled && Settings.ReaderAnimations))
			return;
		var openLeft = FAConnectedAnimationService.GetForView(TopLevel.GetTopLevel(this)).GetAnimation("openL");
		var openRight = FAConnectedAnimationService.GetForView(TopLevel.GetTopLevel(this)).GetAnimation("openR");
		if (openLeft != null || openRight != null || !Settings.PageChangeAnimation)
			ImagesRoot.SetVisualOpacity(1);
		else
			ImagesRoot.FadeIn(TimeSpan.FromMilliseconds(80), new QuadraticEaseIn());
		openLeft?.TryStart(LeftImage);
		openRight?.TryStart(RightImage);
	}

	public async Task ResizeHeight(int height)
	{
		if (_height == height)
			return;
		await decodePixel.WaitAsync();
		_height = height;
		if (LeftImage.IsValid)
			LeftImage.DecodePixelHeight = height;
		if (RightImage.IsValid)
			RightImage.DecodePixelHeight = height;
		decodePixel.Release();
	}

	public async Task ResizeWidth(int width)
	{
		if (_width == width)
			return;
		await decodePixel.WaitAsync();
		_width = width;
		if (LeftImage.IsValid)
			LeftImage.DecodePixelWidth = width;
		if (RightImage.IsValid)
			RightImage.DecodePixelWidth = width;
		decodePixel.Release();
	}

	private async void UserControl_DataContextChanged(object? sender, EventArgs e)
	{
		if (DataContext is ReaderImageSet set)
		{
			Cts.Cancel();
			Cts.Dispose();
			Cts = new();
			await ChangePage(set, Cts.Token);
		}
	}
}