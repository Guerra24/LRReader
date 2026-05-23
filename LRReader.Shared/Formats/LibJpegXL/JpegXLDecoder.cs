using JxlNet;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;

namespace LRReader.Shared.Formats.LibJpegXL;

public sealed class JpegXLDecoder : IImageDecoder
{
	public Image<TPixel> Decode<TPixel>(DecoderOptions options, Stream stream) where TPixel : unmanaged, IPixel<TPixel>
	{
		unsafe
		{
			var runner = JxlThreads.JxlResizableParallelRunnerCreate(null);
			var decoder = Jxl.JxlDecoderCreate(null);
			var input = NativeMemory.Alloc((nuint)stream.Length);
			Jxl.JxlDecoderSetParallelRunner(decoder, JxlThreads.JxlResizableParallelRunner, runner);
			try
			{
				var inputSpan = new Span<byte>(input, (int)stream.Length);
				stream.ReadExactly(inputSpan);

				Jxl.JxlDecoderSetInput(decoder, (byte*)input, (nuint)inputSpan.Length);
				Jxl.JxlDecoderCloseInput(decoder);
				Jxl.JxlDecoderSubscribeEvents(decoder, (int)(JxlDecoderStatus.JXL_DEC_BASIC_INFO | JxlDecoderStatus.JXL_DEC_FRAME | JxlDecoderStatus.JXL_DEC_FULL_IMAGE));

				var status = Jxl.JxlDecoderProcessInput(decoder);

				if (status != JxlDecoderStatus.JXL_DEC_BASIC_INFO)
					throw new Exception();

				var info = new JxlBasicInfo();
				Jxl.JxlDecoderGetBasicInfo(decoder, &info);

				JxlThreads.JxlResizableParallelRunnerSetThreads(runner, JxlThreads.JxlResizableParallelRunnerSuggestThreads(info.xsize, info.ysize));

				status = Jxl.JxlDecoderProcessInput(decoder);

				if (status != JxlDecoderStatus.JXL_DEC_FRAME)
					throw new Exception();

				byte[] buffer = new byte[info.xsize * info.ysize * info.num_color_channels];
				fixed (byte* output = buffer)
				{
					var pixelFormat = new JxlPixelFormat
					{
						data_type = JxlDataType.JXL_TYPE_UINT8,
						endianness = JxlEndianness.JXL_NATIVE_ENDIAN,
						num_channels = info.num_color_channels,
						align = 0
					};

					nuint size = new();
					Jxl.JxlDecoderImageOutBufferSize(decoder, &pixelFormat, &size);

					if (info.xsize * info.ysize * sizeof(byte) * info.num_color_channels != size.ToUInt64())
					{
						throw new Exception();
					}
					if ((ulong)(buffer.Length * sizeof(byte)) != size.ToUInt64())
					{
						throw new Exception();
					}

					Jxl.JxlDecoderSetImageOutBuffer(decoder, &pixelFormat, output, (nuint)(buffer.Length * sizeof(byte)));

					status = Jxl.JxlDecoderProcessInput(decoder);

					if (status != JxlDecoderStatus.JXL_DEC_FULL_IMAGE)
						throw new Exception();

					var image = new Image<TPixel>(options.Configuration, (int)info.xsize, (int)info.ysize);

					if (image.Frames.RootFrame.DangerousTryGetSinglePixelMemory(out var pixels))
					{
						switch (info.num_color_channels)
						{
							case 1:
								var r8 = MemoryMarshal.Cast<byte, L8>(buffer);
								PixelOperations<TPixel>.Instance.FromL8(options.Configuration, r8, pixels.Span);
								break;
							case 3:
								var rgb24 = MemoryMarshal.Cast<byte, Rgb24>(buffer);
								PixelOperations<TPixel>.Instance.FromRgb24(options.Configuration, rgb24, pixels.Span);
								break;
							case 4:
								var rgba32 = MemoryMarshal.Cast<byte, Rgba32>(buffer);
								PixelOperations<TPixel>.Instance.FromRgba32(options.Configuration, rgba32, pixels.Span);
								break;
							default:
								throw new Exception();
						}
					}
					else
					{
						switch (info.num_color_channels)
						{
							case 1:
								var r8 = MemoryMarshal.Cast<byte, L8>(buffer);
								for (int y = 0; y < image.Height; y++)
									PixelOperations<TPixel>.Instance.FromL8(options.Configuration, r8.Slice(image.Width * y, image.Width), image.Frames.RootFrame.PixelBuffer.DangerousGetRowSpan(y));
								break;
							case 3:
								var rgb24 = MemoryMarshal.Cast<byte, Rgb24>(buffer);
								for (int y = 0; y < image.Height; y++)
									PixelOperations<TPixel>.Instance.FromRgb24(options.Configuration, rgb24.Slice(image.Width * y, image.Width), image.Frames.RootFrame.PixelBuffer.DangerousGetRowSpan(y));
								break;
							case 4:
								var rgba32 = MemoryMarshal.Cast<byte, Rgba32>(buffer);
								for (int y = 0; y < image.Height; y++)
									PixelOperations<TPixel>.Instance.FromRgba32(options.Configuration, rgba32.Slice(image.Width * y, image.Width), image.Frames.RootFrame.PixelBuffer.DangerousGetRowSpan(y));
								break;
							default:
								throw new Exception();
						}
					}

					status = Jxl.JxlDecoderProcessInput(decoder);

					if (status != JxlDecoderStatus.JXL_DEC_SUCCESS)
						throw new Exception();

					return image;
				}
			}
			finally
			{
				Jxl.JxlDecoderDestroy(decoder);
				JxlThreads.JxlResizableParallelRunnerDestroy(runner);
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
			var decoder = Jxl.JxlDecoderCreate(null);
			var input = NativeMemory.Alloc((nuint)stream.Length);
			try
			{
				var span = new Span<byte>(input, (int)stream.Length);
				stream.ReadExactly(span);

				Jxl.JxlDecoderSetInput(decoder, (byte*)input, (nuint)span.Length);
				Jxl.JxlDecoderCloseInput(decoder);
				Jxl.JxlDecoderSubscribeEvents(decoder, (int)JxlDecoderStatus.JXL_DEC_BASIC_INFO);
				var status = Jxl.JxlDecoderProcessInput(decoder);
				if (status != JxlDecoderStatus.JXL_DEC_BASIC_INFO)
					throw new Exception();
				var info = new JxlBasicInfo();
				Jxl.JxlDecoderGetBasicInfo(decoder, &info);

				var metadata = new ImageMetadata();
				var jpegxlMetadata = metadata.GetFormatMetadata(JpegXLFormat.Instance);
				jpegxlMetadata.BitsPerSample = (int)info.bits_per_sample;

				return new ImageInfo(new Size((int)info.xsize, (int)info.ysize), metadata);
			}
			finally
			{
				Jxl.JxlDecoderDestroy(decoder);
				NativeMemory.Free(input);
			}
		}
	}

	public Task<ImageInfo> IdentifyAsync(DecoderOptions options, Stream stream, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(Identify(options, stream));
	}
}
