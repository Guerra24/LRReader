using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Skia;
using LRReader.Avalonia.Extensions;
using LRReader.Avalonia.Services;
using LRReader.Shared.Services;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
using System.Numerics;

namespace LRReader.Avalonia.Views.Controls;

public partial class VirtualImage : Control
{
	private CompositionCustomVisual? Visual;
	private SKImage? Image;
	private byte[]? Original;
	private int ImageWidth, ImageHeight;
	private double RenderScaling = ((AvaloniaPlatformService)Service.Platform).RenderScaling;

	static VirtualImage()
	{
		AffectsRender<VirtualImage>(DecodePixelWidthProperty, DecodePixelHeightProperty);
		AffectsMeasure<VirtualImage>(DecodePixelWidthProperty, DecodePixelHeightProperty);
	}

	public bool IsValid => Original != null;

	public int DecodePixelWidth
	{
		get => GetValue(DecodePixelWidthProperty);
		set => SetValue(DecodePixelWidthProperty, value);
	}

	public int DecodePixelHeight
	{
		get => GetValue(DecodePixelHeightProperty);
		set => SetValue(DecodePixelHeightProperty, value);
	}

	protected override bool BypassFlowDirectionPolicies => true;

	protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnAttachedToVisualTree(e);

		RenderScaling = e.PresentationSource.RenderScaling;
		LayoutUpdated += OnLayoutUpdated;

		if (Visual == null)
		{
			var elementVisual = ElementComposition.GetElementVisual(this);
			var compositor = elementVisual?.Compositor;

			if (compositor == null)
				return;

			Visual = compositor.CreateCustomVisual(new VirtualImageCustomVisualHandler());
			Visual.Size = new Vector2((float)Bounds.Size.Width, (float)Bounds.Size.Height);

			ElementComposition.SetElementChildVisual(this, Visual);

			if (Image != null)
			{
				Visual.SendHandlerMessage(Image);
				Image = null;
			}
		}

		InvalidateVisual();
	}

	protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnDetachedFromVisualTree(e);

		LayoutUpdated -= OnLayoutUpdated;
	}

	protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
	{
		base.OnDetachedFromLogicalTree(e);
		Visual?.SendHandlerMessage(null!);
		Visual = null;

		Original = null;
		Image?.Dispose();
		Image = null;
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);

		var prop = change.Property;

		if (prop == DecodePixelWidthProperty || prop == DecodePixelHeightProperty)
		{
			ReloadDisplaySource();
		}
	}

	protected override Size MeasureOverride(Size availableSize)
	{
		return Stretch.Uniform.CalculateSize(availableSize, new Size(ImageWidth, ImageHeight));
	}

	protected override Size ArrangeOverride(Size finalSize)
	{
		return Stretch.Uniform.CalculateSize(finalSize, new Size(ImageWidth, ImageHeight));
	}

	private void OnLayoutUpdated(object? sender, EventArgs e)
	{
		if (Visual == null)
			return;
		Visual.Size = new Vector2((float)Bounds.Size.Width, (float)Bounds.Size.Height);
	}

	public async Task SetSourceAsync(byte[] bytes, CancellationToken cancellationToken = default)
	{
		var decodePixelWidth = DecodePixelWidth;
		var decodePixelHeight = DecodePixelHeight;

		var image = await Task.Run(() =>
		{
			if (cancellationToken.IsCancellationRequested)
				return null;

			var options = new DecoderOptions
			{
				Sampler = KnownResamplers.Lanczos2,
				TargetSize = (decodePixelWidth != 0 || decodePixelHeight != 0) ? new((int)Math.Round(decodePixelWidth * RenderScaling), (int)Math.Round(decodePixelHeight * RenderScaling)) : null
			};

			using var source = SixLabors.ImageSharp.Image.Load<Rgba32>(options, bytes);

			if (cancellationToken.IsCancellationRequested)
				return null;

			ImageWidth = source.Width;
			ImageHeight = source.Height;

			return source.ToSKImage();
		});

		Original = image != null ? bytes : null;
		if (Visual == null)
			Image = image;
		else
			Visual?.SendHandlerMessage(image!);
		InvalidateMeasure();
	}

	public void ClearSource()
	{
		Visual?.SendHandlerMessage(null!);
		Original = null;
	}

	private async void ReloadDisplaySource()
	{
		if (Original == null || Visual == null)
			return;

		var decodePixelWidth = DecodePixelWidth;
		var decodePixelHeight = DecodePixelHeight;

		var image = await Task.Run(() =>
		{
			var options = new DecoderOptions
			{
				Sampler = KnownResamplers.Lanczos2,
				TargetSize = (decodePixelWidth != 0 || decodePixelHeight != 0) ? new((int)Math.Round(decodePixelWidth * RenderScaling), (int)Math.Round(decodePixelHeight * RenderScaling)) : null
			};

			using var source = SixLabors.ImageSharp.Image.Load<Rgba32>(options, Original);

			ImageWidth = source.Width;
			ImageHeight = source.Height;

			return source.ToSKImage();
		});
		Visual.SendHandlerMessage(image);
	}

	public static readonly StyledProperty<int> DecodePixelWidthProperty = AvaloniaProperty.Register<VirtualImage, int>("DecodePixelWidth");
	public static readonly StyledProperty<int> DecodePixelHeightProperty = AvaloniaProperty.Register<VirtualImage, int>("DecodePixelHeight");
}

public class VirtualImageCustomVisualHandler : CompositionCustomVisualHandler
{
	private static readonly SKSamplingOptions SamplingOptions = new(SKFilterMode.Nearest);

	private SKImage? image;

	public override void OnRender(ImmediateDrawingContext drawingContext)
	{
		if (image == null)
			return;
		var skia = drawingContext.TryGetFeature<ISkiaSharpApiLeaseFeature>();
		if (skia == null)
			return;

		var bounds = GetRenderBounds().Size;
		var viewPort = new Rect(bounds);

		var size = new Size(image.Width, image.Height);
		var scale = Stretch.Uniform.CalculateScaling(bounds, size);
		var scaledSize = size * scale;
		var destRect = viewPort.CenterRect(new Rect(scaledSize)).Intersect(viewPort);

		using var lease = skia.Lease();

		if (lease.GrContext != null && !image.IsTextureBacked)
		{
			var gpuImage = image.ToTextureImage(lease.GrContext);
			image.Dispose();
			image = gpuImage;
		}

		using var paint = new SKPaint();
		paint.ColorF = new SKColorF(0, 0, 0, (float)lease.CurrentOpacity);
		lease.SkCanvas.DrawImage(image, destRect.ToSKRect(), SamplingOptions, paint);
	}

	public override void OnMessage(object message)
	{
		image?.Dispose();
		if (message is SKImage img)
		{
			image = img;
			Invalidate();
		}
		else
		{
			image = null;
		}

	}
}
