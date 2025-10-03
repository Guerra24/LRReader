using JxlNet;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace LRReader.Shared.Formats.JpegXL
{
	public sealed class JpegXLDecoder : IImageDecoder
	{
		public Image<TPixel> Decode<TPixel>(DecoderOptions options, Stream stream) where TPixel : unmanaged, IPixel<TPixel>
		{
			unsafe
			{
				var runner = JxlThreads.JxlResizableParallelRunnerCreate(null);
				var decoder = Jxl.JxlDecoderCreate(null);
				Jxl.JxlDecoderSetParallelRunner(decoder, JxlThreads.JxlResizableParallelRunner, runner);
				try
				{
					using var ms = new MemoryStream((int)stream.Length);
					stream.CopyTo(ms);
					fixed (byte* input = ms.ToArray())
					{
						Jxl.JxlDecoderSetInput(decoder, input, (nuint)stream.Length);
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
							var pixelFormat = new JxlPixelFormat();
							pixelFormat.data_type = JxlDataType.JXL_TYPE_UINT8;
							pixelFormat.endianness = JxlEndianness.JXL_NATIVE_ENDIAN;
							pixelFormat.num_channels = info.num_color_channels;
							pixelFormat.align = 0;

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
							image.Frames.RootFrame.DangerousTryGetSinglePixelMemory(out var pixels);

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

							status = Jxl.JxlDecoderProcessInput(decoder);

							if (status != JxlDecoderStatus.JXL_DEC_SUCCESS)
								throw new Exception();

							return image;
						}
					}
				}
				finally
				{
					JxlThreads.JxlResizableParallelRunnerDestroy(runner);
					Jxl.JxlDecoderDestroy(decoder);
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
				try
				{
					using var ms = new MemoryStream((int)stream.Length);
					stream.CopyTo(ms);
					fixed (byte* p = ms.ToArray())
					{
						Jxl.JxlDecoderSetInput(decoder, p, (nuint)stream.Length);
						Jxl.JxlDecoderCloseInput(decoder);
						Jxl.JxlDecoderSubscribeEvents(decoder, (int)JxlDecoderStatus.JXL_DEC_BASIC_INFO);
						var status = Jxl.JxlDecoderProcessInput(decoder);
						if (status != JxlDecoderStatus.JXL_DEC_BASIC_INFO)
							throw new Exception();
						var info = new JxlBasicInfo();
						Jxl.JxlDecoderGetBasicInfo(decoder, &info);
						return new ImageInfo(new PixelTypeInfo((int)info.bits_per_sample), new Size((int)info.xsize, (int)info.ysize), null);
					}
				}
				finally
				{
					Jxl.JxlDecoderDestroy(decoder);
				}
			}
		}

		public Task<ImageInfo> IdentifyAsync(DecoderOptions options, Stream stream, CancellationToken cancellationToken = default)
		{
			return Task.FromResult(Identify(options, stream));
		}
	}
}
