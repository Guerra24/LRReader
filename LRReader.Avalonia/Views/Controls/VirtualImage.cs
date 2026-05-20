using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Numerics;

namespace LRReader.Avalonia.Views.Controls;

public partial class VirtualImage : Control
{
	private CompositionCustomVisual? Visual;
	private WriteableBitmap? Bitmap;
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

		RenderScaling = TopLevel.GetTopLevel(this)!.RenderScaling;

		var elementVisual = ElementComposition.GetElementVisual(this);
		var compositor = elementVisual?.Compositor;

		if (compositor == null)
			return;

		Visual = compositor.CreateCustomVisual(new VirtualImageCustomVisualHandler());

		ElementComposition.SetElementChildVisual(this, Visual);

		LayoutUpdated += OnLayoutUpdated;

		Visual.Size = new Vector2((float)Bounds.Size.Width, (float)Bounds.Size.Height);

		if (Bitmap != null)
			Visual.SendHandlerMessage(Bitmap);

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
		Bitmap?.Dispose();
		Bitmap = null;
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
				scaledSource.Mutate(p => p.Resize((int)Math.Round(decodePixelWidth * RenderScaling), (int)Math.Round(decodePixelHeight * RenderScaling)));

			var bitmap = new WriteableBitmap(new PixelSize(scaledSource.Width, scaledSource.Height), new global::Avalonia.Vector(96, 96), PixelFormat.Rgba8888, AlphaFormat.Opaque);
			using (var lockedBitmap = bitmap.Lock())
			{
				unsafe
				{
					var span = new Span<byte>((void*)lockedBitmap.Address, lockedBitmap.RowBytes * lockedBitmap.Size.Height);
					scaledSource.CopyPixelDataTo(span);
				}
			}
			return (source, scaledSource, bitmap);
		});

		Source = results.source;
		ScaledSource = results.scaledSource;
		Bitmap = results.bitmap;
		Visual?.SendHandlerMessage(results.bitmap);
	}

	private void ReloadDisplaySource()
	{
		if (Source == null || Visual == null)
			return;

		ScaledSource = Source.Clone();

		if (DecodePixelWidth != 0 || DecodePixelHeight != 0)
			ScaledSource.Mutate(p => p.Resize((int)Math.Round(DecodePixelWidth * RenderScaling), (int)Math.Round(DecodePixelHeight * RenderScaling)));

		var bitmap = new WriteableBitmap(new PixelSize(ScaledSource.Width, ScaledSource.Height), new global::Avalonia.Vector(96, 96), PixelFormat.Rgba8888, AlphaFormat.Opaque);
		using (var lockedBitmap = bitmap.Lock())
		{
			unsafe
			{
				var span = new Span<byte>((void*)lockedBitmap.Address, lockedBitmap.RowBytes * lockedBitmap.Size.Height);
				ScaledSource.CopyPixelDataTo(span);
			}
		}
		Bitmap = bitmap;
		Visual.SendHandlerMessage(bitmap);
	}

	public static readonly StyledProperty<Image<Rgba32>?> SourceProperty = AvaloniaProperty.Register<VirtualImage, Image<Rgba32>?>("Source");
	public static readonly StyledProperty<Image<Rgba32>?> ScaledSourceProperty = AvaloniaProperty.Register<VirtualImage, Image<Rgba32>?>("ScaledSource");
	public static readonly StyledProperty<int> DecodePixelWidthProperty = AvaloniaProperty.Register<VirtualImage, int>("DecodePixelWidth");
	public static readonly StyledProperty<int> DecodePixelHeightProperty = AvaloniaProperty.Register<VirtualImage, int>("DecodePixelHeight");
}

public class VirtualImageCustomVisualHandler : CompositionCustomVisualHandler
{
	private Bitmap? bitmap;

	public override void OnRender(ImmediateDrawingContext drawingContext)
	{
		if (bitmap == null)
			return;
		var bounds = GetRenderBounds().Size;
		var viewPort = new Rect(bounds);

		var scale = Stretch.Uniform.CalculateScaling(bounds, bitmap.Size);
		var scaledSize = bitmap.Size * scale;
		var destRect = viewPort
			.CenterRect(new Rect(scaledSize))
			.Intersect(viewPort);

		drawingContext.DrawBitmap(bitmap, new Rect(bitmap.Size), destRect);
	}

	public override void OnMessage(object message)
	{
		if (message is Bitmap img)
		{
			bitmap?.Dispose();
			bitmap = img;
			Invalidate();
		}
		else
		{
			bitmap = null;
		}

	}
}
