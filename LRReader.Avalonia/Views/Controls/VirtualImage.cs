using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Skia;
using LRReader.Avalonia.Extensions;
using LRReader.Avalonia.Services;
using LRReader.Shared.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
using System.Numerics;

namespace LRReader.Avalonia.Views.Controls;

public partial class VirtualImage : Control
{
	private CompositionCustomVisual? Visual;
	private SKImage? Image;
	private double RenderScaling = ((AvaloniaPlatformService)Service.Platform).RenderScaling;

	static VirtualImage()
	{
		AffectsRender<VirtualImage>(SourceProperty, DecodePixelWidthProperty, DecodePixelHeightProperty);
		AffectsMeasure<VirtualImage>(SourceProperty, DecodePixelWidthProperty, DecodePixelHeightProperty);
	}

	public Image<Rgba32>? Source
	{
		get => GetValue(SourceProperty);
		set => SetValue(SourceProperty, value);
	}

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

		var elementVisual = ElementComposition.GetElementVisual(this);
		var compositor = elementVisual?.Compositor;

		if (compositor == null)
			return;

		Visual = compositor.CreateCustomVisual(new VirtualImageCustomVisualHandler());

		ElementComposition.SetElementChildVisual(this, Visual);

		LayoutUpdated += OnLayoutUpdated;

		Visual.Size = new Vector2((float)Bounds.Size.Width, (float)Bounds.Size.Height);

		if (Image != null)
			Visual.SendHandlerMessage(Image);

		InvalidateVisual();
	}

	protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnDetachedFromVisualTree(e);

		LayoutUpdated -= OnLayoutUpdated;

		Visual?.SendHandlerMessage(null!);
		Visual = null;
	}

	protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
	{
		base.OnDetachedFromLogicalTree(e);
		Source = null;
		Image?.Dispose();
		Image = null;
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);

		var prop = change.Property;

		if (prop == SourceProperty)
		{
			if (change.OldValue is Image<Rgba32> old)
				old.Dispose();
		}
		else if (prop == DecodePixelWidthProperty)
		{
			ReloadDisplaySource();
		}
		else if (prop == DecodePixelHeightProperty)
		{
			ReloadDisplaySource();
		}
	}

	protected override global::Avalonia.Size MeasureOverride(global::Avalonia.Size availableSize)
	{
		if (Image == null)
			return new global::Avalonia.Size();
		return Stretch.Uniform.CalculateSize(availableSize, new global::Avalonia.Size(Image.Width, Image.Height));
	}

	protected override global::Avalonia.Size ArrangeOverride(global::Avalonia.Size finalSize)
	{
		if (Image == null)
			return new global::Avalonia.Size();
		return Stretch.Uniform.CalculateSize(finalSize, new global::Avalonia.Size(Image.Width, Image.Height));
	}

	private void OnLayoutUpdated(object? sender, EventArgs e)
	{
		if (Visual == null)
			return;
		Visual.Size = new Vector2((float)Bounds.Size.Width, (float)Bounds.Size.Height);
	}

	public async Task SetSourceAsync(byte[] bytes)
	{
		var decodePixelWidth = DecodePixelWidth;
		var decodePixelHeight = DecodePixelHeight;

		var results = await Task.Run(() =>
		{
			var source = SixLabors.ImageSharp.Image.Load<Rgba32>(bytes);
			var scaledSource = source;

			if (decodePixelWidth != 0 || decodePixelHeight != 0)
				scaledSource = source.Clone(p => p.Resize((int)Math.Round(decodePixelWidth * RenderScaling), (int)Math.Round(decodePixelHeight * RenderScaling), KnownResamplers.Lanczos2));

			var img = scaledSource.ToSKImage();

			if (scaledSource != source)
				scaledSource.Dispose();

			return (source, image: img);
		});

		Source = results.source;
		Image = results.image;
		Visual?.SendHandlerMessage(results.image);
	}

	private async void ReloadDisplaySource()
	{
		if (Source == null || Visual == null)
			return;

		var decodePixelWidth = DecodePixelWidth;
		var decodePixelHeight = DecodePixelHeight;
		var scaledSource = Source;

		var image = await Task.Run(() =>
		{
			if (decodePixelWidth != 0 || decodePixelHeight != 0)
				scaledSource = scaledSource.Clone(p => p.Resize((int)Math.Round(decodePixelWidth * RenderScaling), (int)Math.Round(decodePixelHeight * RenderScaling), KnownResamplers.Lanczos2));

			return scaledSource.ToSKImage();
		});

		if (scaledSource != Source)
			scaledSource.Dispose();

		Image = image;
		Visual.SendHandlerMessage(image);
	}

	public static readonly StyledProperty<Image<Rgba32>?> SourceProperty = AvaloniaProperty.Register<VirtualImage, Image<Rgba32>?>("Source");
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

		var size = new global::Avalonia.Size(image.Width, image.Height);
		var scale = Stretch.Uniform.CalculateScaling(bounds, size);
		var scaledSize = size * scale;
		var destRect = viewPort.CenterRect(new Rect(scaledSize)).Intersect(viewPort);

		using var lease = skia.Lease();
		using var paint = new SKPaint();
		paint.ColorF = new SKColorF(0, 0, 0, (float)lease.CurrentOpacity);
		lease.SkCanvas.DrawImage(image, destRect.ToSKRect(), SamplingOptions, paint);
	}

	public override void OnMessage(object message)
	{
		if (message is SKImage img)
		{
			image?.Dispose();
			image = img;
			Invalidate();
		}
		else
		{
			image = null;
		}

	}
}
