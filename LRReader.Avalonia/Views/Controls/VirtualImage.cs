using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Skia;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
using System.Numerics;
using System.Runtime.InteropServices;

namespace LRReader.Avalonia.Views.Controls;

public partial class VirtualImage : Control
{
	private CompositionCustomVisual? Visual;
	private SKImage? Image;
	private double RenderScaling = 1;

	static VirtualImage()
	{
		AffectsRender<VirtualImage>(SourceProperty, ScaledSourceProperty, DecodePixelWidthProperty, DecodePixelHeightProperty);
		AffectsMeasure<VirtualImage>(SourceProperty, ScaledSourceProperty, DecodePixelWidthProperty, DecodePixelHeightProperty);
	}

	public Image<Rgba32>? Source
	{
		get => GetValue(SourceProperty);
		set => SetValue(SourceProperty, value);
	}

	public Image<Rgba32>? ScaledSource
	{
		get => GetValue(ScaledSourceProperty);
		set => SetValue(ScaledSourceProperty, value);
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
		ScaledSource = null;
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
		else if (prop == ScaledSourceProperty)
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
		var image = ScaledSource ?? Source;
		if (image == null)
			return new global::Avalonia.Size();
		return Stretch.Uniform.CalculateSize(availableSize, new global::Avalonia.Size(image.Size.Width, image.Size.Height));
	}

	protected override global::Avalonia.Size ArrangeOverride(global::Avalonia.Size finalSize)
	{
		var image = ScaledSource ?? Source;
		if (image == null)
			return new global::Avalonia.Size();
		return Stretch.Uniform.CalculateSize(finalSize, new global::Avalonia.Size(image.Size.Width, image.Size.Height));
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
			var scaledSource = source.Clone();

			if (decodePixelWidth != 0 || decodePixelHeight != 0)
				scaledSource.Mutate(p => p.Resize((int)Math.Round(decodePixelWidth * RenderScaling), (int)Math.Round(decodePixelHeight * RenderScaling), KnownResamplers.Lanczos2));

			var info = new SKImageInfo(scaledSource.Width, scaledSource.Height, SKColorType.Rgba8888, SKAlphaType.Opaque);
			SKImage img;
			unsafe
			{
				if (scaledSource.DangerousTryGetSinglePixelMemory(out var memory))
				{
					using var handle = memory.Pin();
					img = SKImage.FromPixelCopy(info, (nint)handle.Pointer);
				}
				else
				{
					var length = info.Width * info.Height * info.BytesPerPixel;
					var temp = NativeMemory.Alloc((nuint)length);
					var span = new Span<byte>(temp, length);
					scaledSource.CopyPixelDataTo(span);
					img = SKImage.FromPixelCopy(info, (nint)temp);
					NativeMemory.Free(temp);
				}
			}

			return (source, scaledSource, image: img);
		});

		Source = results.source;
		ScaledSource = results.scaledSource;
		Image = results.image;
		Visual?.SendHandlerMessage(results.image);
	}

	private async void ReloadDisplaySource()
	{
		if (Source == null || Visual == null)
			return;

		var decodePixelWidth = DecodePixelWidth;
		var decodePixelHeight = DecodePixelHeight;
		var scaledSource = await Task.Run(Source.Clone);

		if (decodePixelWidth != 0 || decodePixelHeight != 0)
			await Task.Run(() => scaledSource.Mutate(p => p.Resize((int)Math.Round(decodePixelWidth * RenderScaling), (int)Math.Round(decodePixelHeight * RenderScaling), KnownResamplers.Lanczos2)));

		var image = await Task.Run(() =>
		{
			var info = new SKImageInfo(scaledSource.Width, scaledSource.Height, SKColorType.Rgba8888, SKAlphaType.Opaque);
			unsafe
			{
				if (scaledSource.DangerousTryGetSinglePixelMemory(out var memory))
				{
					using var handle = memory.Pin();
					return SKImage.FromPixelCopy(info, (nint)handle.Pointer);
				}
				else
				{
					var length = info.Width * info.Height * info.BytesPerPixel;
					var temp = NativeMemory.Alloc((nuint)length);
					var span = new Span<byte>(temp, length);
					scaledSource.CopyPixelDataTo(span);
					var img = SKImage.FromPixelCopy(info, (nint)temp);
					NativeMemory.Free(temp);
					return img;
				}
			}

		});

		ScaledSource = scaledSource;
		Image = image;
		Visual.SendHandlerMessage(image);
	}

	public static readonly StyledProperty<Image<Rgba32>?> SourceProperty = AvaloniaProperty.Register<VirtualImage, Image<Rgba32>?>("Source");
	public static readonly StyledProperty<Image<Rgba32>?> ScaledSourceProperty = AvaloniaProperty.Register<VirtualImage, Image<Rgba32>?>("ScaledSource");
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
