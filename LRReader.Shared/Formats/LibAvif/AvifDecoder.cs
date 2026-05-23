using AvifNet;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;

namespace LRReader.Shared.Formats.LibAvif;

public sealed class AvifDecoder : IImageDecoder
{
	public Image<TPixel> Decode<TPixel>(DecoderOptions options, Stream stream) where TPixel : unmanaged, IPixel<TPixel>
	{
		unsafe
		{
			var decoder = Avif.avifDecoderCreate();
			var input = NativeMemory.Alloc((nuint)stream.Length);
			try
			{
				var inputSpan = new Span<byte>(input, (int)stream.Length);
				stream.ReadExactly(inputSpan);
				var res = Avif.avifDecoderSetIOMemory(decoder, (byte*)input, (nuint)inputSpan.Length);
				if (res != avifResult.AVIF_RESULT_OK)
					throw new Exception();

				res = Avif.avifDecoderParse(decoder);
				if (res != avifResult.AVIF_RESULT_OK)
					throw new Exception();

				res = Avif.avifDecoderNextImage(decoder);
				if (res != avifResult.AVIF_RESULT_OK)
					throw new Exception();

				avifRGBImage rgb = new();

				Avif.avifRGBImageSetDefaults(&rgb, decoder->image);

				res = Avif.avifRGBImageAllocatePixels(&rgb);
				if (res != avifResult.AVIF_RESULT_OK)
					throw new Exception();
				try
				{

					res = Avif.avifImageYUVToRGB(decoder->image, &rgb);
					if (res != avifResult.AVIF_RESULT_OK)
						throw new Exception();

					var image = new Image<TPixel>(options.Configuration, (int)decoder->image->width, (int)decoder->image->height);

					var span = new ReadOnlySpan<byte>(rgb.pixels, (int)(rgb.rowBytes * decoder->image->height));

					if (image.Frames.RootFrame.DangerousTryGetSinglePixelMemory(out var pixels))
					{
						switch (rgb.format)
						{
							case avifRGBFormat.AVIF_RGB_FORMAT_RGB:
								if (rgb.depth > 8)
								{
									var rgb48 = MemoryMarshal.Cast<byte, Rgb48>(span);
									PixelOperations<TPixel>.Instance.FromRgb48(options.Configuration, rgb48, pixels.Span);
								}
								else
								{
									var rgb24 = MemoryMarshal.Cast<byte, Rgb24>(span);
									PixelOperations<TPixel>.Instance.FromRgb24(options.Configuration, rgb24, pixels.Span);
								}
								break;
							case avifRGBFormat.AVIF_RGB_FORMAT_RGBA:
								if (rgb.depth > 8)
								{
									var rgba64 = MemoryMarshal.Cast<byte, Rgba64>(span);
									PixelOperations<TPixel>.Instance.FromRgba64(options.Configuration, rgba64, pixels.Span);
								}
								else
								{
									var rgba32 = MemoryMarshal.Cast<byte, Rgba32>(span);
									PixelOperations<TPixel>.Instance.FromRgba32(options.Configuration, rgba32, pixels.Span);
								}
								break;
							case avifRGBFormat.AVIF_RGB_FORMAT_ARGB:
								if (rgb.depth > 8)
								{
									throw new Exception();
								}
								else
								{
									var argb32 = MemoryMarshal.Cast<byte, Argb32>(span);
									PixelOperations<TPixel>.Instance.FromArgb32(options.Configuration, argb32, pixels.Span);
								}
								break;
							case avifRGBFormat.AVIF_RGB_FORMAT_BGR:
								if (rgb.depth > 8)
								{
									throw new Exception();
								}
								else
								{
									var bgr24 = MemoryMarshal.Cast<byte, Bgr24>(span);
									PixelOperations<TPixel>.Instance.FromBgr24(options.Configuration, bgr24, pixels.Span);
								}
								break;
							case avifRGBFormat.AVIF_RGB_FORMAT_BGRA:
								if (rgb.depth > 8)
								{
									throw new Exception();
								}
								else
								{
									var bgra32 = MemoryMarshal.Cast<byte, Bgra32>(span);
									PixelOperations<TPixel>.Instance.FromBgra32(options.Configuration, bgra32, pixels.Span);
								}
								break;
							case avifRGBFormat.AVIF_RGB_FORMAT_ABGR:
								if (rgb.depth > 8)
								{
									throw new Exception();
								}
								else
								{
									var abgr32 = MemoryMarshal.Cast<byte, Abgr32>(span);
									PixelOperations<TPixel>.Instance.FromAbgr32(options.Configuration, abgr32, pixels.Span);
								}
								break;
							case avifRGBFormat.AVIF_RGB_FORMAT_GRAY:
								if (rgb.depth > 8)
								{
									var l16 = MemoryMarshal.Cast<byte, L16>(span);
									PixelOperations<TPixel>.Instance.FromL16(options.Configuration, l16, pixels.Span);
								}
								else
								{
									var l8 = MemoryMarshal.Cast<byte, L8>(span);
									PixelOperations<TPixel>.Instance.FromL8(options.Configuration, l8, pixels.Span);
								}
								break;
							case avifRGBFormat.AVIF_RGB_FORMAT_GRAYA:
								if (rgb.depth > 8)
								{
									var la32 = MemoryMarshal.Cast<byte, La32>(span);
									PixelOperations<TPixel>.Instance.FromLa32(options.Configuration, la32, pixels.Span);
								}
								else
								{
									var la16 = MemoryMarshal.Cast<byte, La16>(span);
									PixelOperations<TPixel>.Instance.FromLa16(options.Configuration, la16, pixels.Span);
								}
								break;
							default:
								throw new Exception();
						}
					}
					else
					{
						switch (rgb.format)
						{
							case avifRGBFormat.AVIF_RGB_FORMAT_RGB:
								if (rgb.depth > 8)
								{
									var rgb48 = MemoryMarshal.Cast<byte, Rgb48>(span);
									for (int y = 0; y < image.Height; y++)
										PixelOperations<TPixel>.Instance.FromRgb48(options.Configuration, rgb48.Slice(image.Width * y, image.Width), image.Frames.RootFrame.PixelBuffer.DangerousGetRowSpan(y));
									PixelOperations<TPixel>.Instance.FromRgb48(options.Configuration, rgb48, pixels.Span);
								}
								else
								{
									var rgb24 = MemoryMarshal.Cast<byte, Rgb24>(span);
									for (int y = 0; y < image.Height; y++)
										PixelOperations<TPixel>.Instance.FromRgb24(options.Configuration, rgb24.Slice(image.Width * y, image.Width), image.Frames.RootFrame.PixelBuffer.DangerousGetRowSpan(y));
								}
								break;
							case avifRGBFormat.AVIF_RGB_FORMAT_RGBA:
								if (rgb.depth > 8)
								{
									var rgba64 = MemoryMarshal.Cast<byte, Rgba64>(span);
									for (int y = 0; y < image.Height; y++)
										PixelOperations<TPixel>.Instance.FromRgba64(options.Configuration, rgba64.Slice(image.Width * y, image.Width), image.Frames.RootFrame.PixelBuffer.DangerousGetRowSpan(y));
								}
								else
								{
									var rgba32 = MemoryMarshal.Cast<byte, Rgba32>(span);
									for (int y = 0; y < image.Height; y++)
										PixelOperations<TPixel>.Instance.FromRgba32(options.Configuration, rgba32.Slice(image.Width * y, image.Width), image.Frames.RootFrame.PixelBuffer.DangerousGetRowSpan(y));
								}
								break;
							case avifRGBFormat.AVIF_RGB_FORMAT_ARGB:
								if (rgb.depth > 8)
								{
									throw new Exception();
								}
								else
								{
									var argb32 = MemoryMarshal.Cast<byte, Argb32>(span);
									for (int y = 0; y < image.Height; y++)
										PixelOperations<TPixel>.Instance.FromArgb32(options.Configuration, argb32.Slice(image.Width * y, image.Width), image.Frames.RootFrame.PixelBuffer.DangerousGetRowSpan(y));
								}
								break;
							case avifRGBFormat.AVIF_RGB_FORMAT_BGR:
								if (rgb.depth > 8)
								{
									throw new Exception();
								}
								else
								{
									var bgr24 = MemoryMarshal.Cast<byte, Bgr24>(span);
									for (int y = 0; y < image.Height; y++)
										PixelOperations<TPixel>.Instance.FromBgr24(options.Configuration, bgr24.Slice(image.Width * y, image.Width), image.Frames.RootFrame.PixelBuffer.DangerousGetRowSpan(y));
								}
								break;
							case avifRGBFormat.AVIF_RGB_FORMAT_BGRA:
								if (rgb.depth > 8)
								{
									throw new Exception();
								}
								else
								{
									var bgra32 = MemoryMarshal.Cast<byte, Bgra32>(span);
									for (int y = 0; y < image.Height; y++)
										PixelOperations<TPixel>.Instance.FromBgra32(options.Configuration, bgra32.Slice(image.Width * y, image.Width), image.Frames.RootFrame.PixelBuffer.DangerousGetRowSpan(y));
								}
								break;
							case avifRGBFormat.AVIF_RGB_FORMAT_ABGR:
								if (rgb.depth > 8)
								{
									throw new Exception();
								}
								else
								{
									var abgr32 = MemoryMarshal.Cast<byte, Abgr32>(span);
									for (int y = 0; y < image.Height; y++)
										PixelOperations<TPixel>.Instance.FromAbgr32(options.Configuration, abgr32.Slice(image.Width * y, image.Width), image.Frames.RootFrame.PixelBuffer.DangerousGetRowSpan(y));
								}
								break;
							case avifRGBFormat.AVIF_RGB_FORMAT_GRAY:
								if (rgb.depth > 8)
								{
									var l16 = MemoryMarshal.Cast<byte, L16>(span);
									for (int y = 0; y < image.Height; y++)
										PixelOperations<TPixel>.Instance.FromL16(options.Configuration, l16.Slice(image.Width * y, image.Width), image.Frames.RootFrame.PixelBuffer.DangerousGetRowSpan(y));
								}
								else
								{
									var l8 = MemoryMarshal.Cast<byte, L8>(span);
									for (int y = 0; y < image.Height; y++)
										PixelOperations<TPixel>.Instance.FromL8(options.Configuration, l8.Slice(image.Width * y, image.Width), image.Frames.RootFrame.PixelBuffer.DangerousGetRowSpan(y));
								}
								break;
							case avifRGBFormat.AVIF_RGB_FORMAT_GRAYA:
								if (rgb.depth > 8)
								{
									var la32 = MemoryMarshal.Cast<byte, La32>(span);
									for (int y = 0; y < image.Height; y++)
										PixelOperations<TPixel>.Instance.FromLa32(options.Configuration, la32.Slice(image.Width * y, image.Width), image.Frames.RootFrame.PixelBuffer.DangerousGetRowSpan(y));
								}
								else
								{
									var la16 = MemoryMarshal.Cast<byte, La16>(span);
									for (int y = 0; y < image.Height; y++)
										PixelOperations<TPixel>.Instance.FromLa16(options.Configuration, la16.Slice(image.Width * y, image.Width), image.Frames.RootFrame.PixelBuffer.DangerousGetRowSpan(y));
								}
								break;
							default:
								throw new Exception();
						}
					}
					return image;
				}
				finally
				{
					Avif.avifRGBImageFreePixels(&rgb);
				}
			}
			finally
			{
				Avif.avifDecoderDestroy(decoder);
				NativeMemory.Free(input);
			}
		}
	}

	public Image Decode(DecoderOptions options, Stream stream) => Decode<Rgb24>(options, stream);

	public Task<Image<TPixel>> DecodeAsync<TPixel>(DecoderOptions options, Stream stream, CancellationToken cancellationToken = default) where TPixel : unmanaged, IPixel<TPixel>
	{
		return Task.FromResult(Decode<TPixel>(options, stream));
	}

	public Task<Image> DecodeAsync(DecoderOptions options, Stream stream, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(Decode(options, stream));
	}

	public ImageInfo Identify(DecoderOptions options, Stream stream)
	{
		unsafe
		{
			var decoder = Avif.avifDecoderCreate();
			var input = NativeMemory.Alloc((nuint)stream.Length);
			try
			{
				var span = new Span<byte>(input, (int)stream.Length);
				stream.ReadExactly(span);

				var res = Avif.avifDecoderSetIOMemory(decoder, (byte*)input, (nuint)span.Length);
				if (res != avifResult.AVIF_RESULT_OK)
					throw new Exception();

				res = Avif.avifDecoderParse(decoder);
				if (res != avifResult.AVIF_RESULT_OK)
					throw new Exception();

				var metadata = new ImageMetadata();
				var avifMetadata = metadata.GetFormatMetadata(AvifFormat.Instance);
				avifMetadata.Depth = (int)decoder->image->depth;

				return new ImageInfo(new Size((int)decoder->image->width, (int)decoder->image->height), metadata);
			}
			finally
			{
				Avif.avifDecoderDestroy(decoder);
				NativeMemory.Free(input);
			}
		}
	}

	public Task<ImageInfo> IdentifyAsync(DecoderOptions options, Stream stream, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(Identify(options, stream));
	}
}
