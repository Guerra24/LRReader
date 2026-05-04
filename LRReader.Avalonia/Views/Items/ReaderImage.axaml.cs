using Avalonia.Media.Imaging;
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

	private ReaderImageSet? Set;

	public ReaderImage()
	{
		InitializeComponent();
	}

	public async Task ChangePage(ReaderImageSet set)
	{
		await decodePixel.WaitAsync();
		Set = set;
		var images = await Task.WhenAll(Service.Images.GetImageCached(set.LeftImage), Service.Images.GetImageCached(set.RightImage));
		var imageBitmaps = await Task.WhenAll(imageProcessing.ByteToBitmap(images[0], _width, _height), imageProcessing.ByteToBitmap(images[1], _width, _height));
		LeftImage.Source = imageBitmaps[0] as Bitmap;
		RightImage.Source = imageBitmaps[1] as Bitmap;
		var sizes = await Task.WhenAll(Service.Images.GetImageSizeCached(set.LeftImage), Service.Images.GetImageSizeCached(set.RightImage));
		var size = new Size(Math.Max(sizes[0].Width, sizes[1].Width), Math.Max(sizes[0].Height, sizes[1].Height));
		LeftImage.Height = RightImage.Height = 0;
		if (LeftImage.Source != null)
		{
			//LeftImage.Width = size.Width;
			LeftImage.Height = set.Height == 0 ? size.Height : set.Height;
		}
		if (RightImage.Source != null)
		{
			//RightImage.Width = size.Width;
			RightImage.Height = size.Height;
		}
		decodePixel.Release();
	}

	public async Task FadeOutPage()
	{
		if (!(Service.Platform.AnimationsEnabled && Service.Settings.ReaderAnimations && Service.Settings.PageChangeAnimation))
			return;
		//if (disableAnimation)
		{
			ImagesRoot.SetVisualOpacity(0);
			disableAnimation = false;
		}
		/*else
		{
			await FadeOut.StartAsync(ImagesRoot);
		}*/
	}

	public void FadeInPage()
	{
		if (!(Service.Platform.AnimationsEnabled && Service.Settings.ReaderAnimations))
			return;
		/*var openLeft = ConnectedAnimationService.GetForCurrentView().GetAnimation("openL");
		var openRight = ConnectedAnimationService.GetForCurrentView().GetAnimation("openR");
		if (openLeft != null || openRight != null || !Service.Settings.PageChangeAnimation)*/
		ImagesRoot.SetVisualOpacity(1);
		/*else
			FadeIn.Start(ImagesRoot);*/
		//openLeft?.TryStart(LeftImage);
		//openRight?.TryStart(RightImage);
	}

	public async Task ResizeHeight(int height)
	{
		if (_height == height)
			return;
		await decodePixel.WaitAsync();
		_height = height;
		/*if (LeftImage.Source != null)
			((Bitmap)LeftImage.Source).DecodePixelHeight = height;
		if (RightImage.Source != null)
			((Bitmap)RightImage.Source).DecodePixelHeight = height;*/
		// Manually decode again in the target size
		if (Set != null)
		{
			var images = await Task.WhenAll(Service.Images.GetImageCached(Set.LeftImage), Service.Images.GetImageCached(Set.RightImage));
			var imageBitmaps = await Task.WhenAll(imageProcessing.ByteToBitmap(images[0], _width, _height), imageProcessing.ByteToBitmap(images[1], _width, _height));
			LeftImage.Source = imageBitmaps[0] as Bitmap;
			RightImage.Source = imageBitmaps[1] as Bitmap;
		}
		decodePixel.Release();
	}

	public async Task ResizeWidth(int width)
	{
		if (_width == width)
			return;
		await decodePixel.WaitAsync();
		_width = width;
		/*if (LeftImage.Source != null)
			((Bitmap)LeftImage.Source).DecodePixelWidth = width;
		if (RightImage.Source != null)
			((Bitmap)RightImage.Source).DecodePixelWidth = width;*/
		if (Set != null)
		{
			var images = await Task.WhenAll(Service.Images.GetImageCached(Set.LeftImage), Service.Images.GetImageCached(Set.RightImage));
			var imageBitmaps = await Task.WhenAll(imageProcessing.ByteToBitmap(images[0], _width, _height), imageProcessing.ByteToBitmap(images[1], _width, _height));
			LeftImage.Source = imageBitmaps[0] as Bitmap;
			RightImage.Source = imageBitmaps[1] as Bitmap;
		}
		decodePixel.Release();
	}

	private async void UserControl_DataContextChanged(object? sender, EventArgs e)
	{
		if (DataContext is ReaderImageSet set)
			await ChangePage(set);
	}
}