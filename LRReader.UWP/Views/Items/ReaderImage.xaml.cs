using CommunityToolkit.WinUI.Animations;
using LRReader.Shared.Models.Main;
using LRReader.UWP.Extensions;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using static LRReader.Shared.Services.Service;

namespace LRReader.UWP.Views.Items;

public sealed partial class ReaderImage : UserControl
{

	private static readonly AnimationBuilder FadeIn = AnimationBuilder.Create().Opacity(to: 1, duration: TimeSpan.FromMilliseconds(80), easingMode: EasingMode.EaseIn);
	private static readonly AnimationBuilder FadeOut = AnimationBuilder.Create().Opacity(to: 0, duration: TimeSpan.FromMilliseconds(80), easingMode: EasingMode.EaseOut);

	public bool disableAnimation = true;
	private int _height, _width;

	private readonly SemaphoreSlim decodePixel = new(1);
	private CancellationTokenSource Cts = new();

	public ReaderImage()
	{
		this.InitializeComponent();
	}

	[DynamicWindowsRuntimeCast(typeof(BitmapImage))]
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
			var imageBitmaps = await Task.WhenAll(ImageProcessing.ByteToBitmap(images[0], _width, _height, LeftImage.Source, cancellationToken), ImageProcessing.ByteToBitmap(images[1], _width, _height, RightImage.Source, cancellationToken));
			if (cancellationToken.IsCancellationRequested)
				return;
			LeftImage.Source = imageBitmaps[0] as BitmapImage;
			RightImage.Source = imageBitmaps[1] as BitmapImage;
			var sizes = await Task.WhenAll(Images.GetImageSizeCached(set.LeftImage, cancellationToken: cancellationToken), Images.GetImageSizeCached(set.RightImage, cancellationToken: cancellationToken));
			if (cancellationToken.IsCancellationRequested)
				return;
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
			await FadeOut.StartAsync(ImagesRoot);
		}
	}

	public void FadeInPage()
	{
		if (!(Platform.AnimationsEnabled && Settings.ReaderAnimations))
			return;
		var openLeft = ConnectedAnimationService.GetForCurrentView().GetAnimation("openL");
		var openRight = ConnectedAnimationService.GetForCurrentView().GetAnimation("openR");
		if (openLeft != null || openRight != null || !Settings.PageChangeAnimation)
			ImagesRoot.SetVisualOpacity(1);
		else
			FadeIn.Start(ImagesRoot);
		openLeft?.TryStart(LeftImage);
		openRight?.TryStart(RightImage);
	}

	[DynamicWindowsRuntimeCast(typeof(BitmapImage))]
	public async Task ResizeHeight(int height)
	{
		if (_height == height)
			return;
		await decodePixel.WaitAsync();
		_height = height;
		if (LeftImage.Source != null)
			((BitmapImage)LeftImage.Source).DecodePixelHeight = height;
		if (RightImage.Source != null)
			((BitmapImage)RightImage.Source).DecodePixelHeight = height;
		decodePixel.Release();
	}

	[DynamicWindowsRuntimeCast(typeof(BitmapImage))]
	public async Task ResizeWidth(int width)
	{
		if (_width == width)
			return;
		await decodePixel.WaitAsync();
		_width = width;
		if (LeftImage.Source != null)
			((BitmapImage)LeftImage.Source).DecodePixelWidth = width;
		if (RightImage.Source != null)
			((BitmapImage)RightImage.Source).DecodePixelWidth = width;
		decodePixel.Release();
	}

	private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
	{
		if (args.NewValue is ReaderImageSet set)
		{
			Cts.Cancel();
			Cts.Dispose();
			Cts = new();
			await ChangePage(set, Cts.Token);
		}
	}
}
