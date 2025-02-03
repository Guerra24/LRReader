using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using JxlNet;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace LRReader.Shared.Formats.JpegXL
{
	public sealed class JpegXLDecoder : IImageDecoder, IImageInfoDetector
	{
		public Image<TPixel> Decode<TPixel>(Configuration configuration, Stream stream, CancellationToken cancellationToken) where TPixel : unmanaged, IPixel<TPixel>
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
						Jxl.JxlDecoderSetInput(decoder, input, (UIntPtr)stream.Length);
						Jxl.JxlDecoderCloseInput(decoder);
						Jxl.JxlDecoderSubscribeEvents(decoder, (int)(JxlDecoderStatus.JXL_DEC_BASIC_INFO | JxlDecoderStatus.JXL_DEC_FRAME | JxlDecoderStatus.JXL_DEC_FULL_IMAGE));

						var status = Jxl.JxlDecoderProcessInput(decoder);

						if (status != JxlDecoderStatus.JXL_DEC_BASIC_INFO)
							throw new Exception();

						var info = new JxlBasicInfo();
						Jxl.JxlDecoderGetBasicInfo(decoder, &info);

						JxlThreads.JxlResizableParallelRunnerSetThreads(runner, (UIntPtr)JxlThreads.JxlResizableParallelRunnerSuggestThreads(info.xsize, info.ysize));

						status = Jxl.JxlDecoderProcessInput(decoder);

						if (status != JxlDecoderStatus.JXL_DEC_FRAME)
							throw new Exception();

						byte[] buffer = new byte[info.xsize * info.ysize * 3];
						fixed (byte* output = buffer)
						{
							var pixelFormat = new JxlPixelFormat();
							pixelFormat.data_type = JxlDataType.JXL_TYPE_UINT8;
							pixelFormat.endianness = JxlEndianness.JXL_NATIVE_ENDIAN;
							pixelFormat.num_channels = 3;
							pixelFormat.align = (UIntPtr)0;

							UIntPtr size = new();
							Jxl.JxlDecoderImageOutBufferSize(decoder, &pixelFormat, &size);

							if (info.xsize * info.ysize * sizeof(byte) * 3 != size.ToUInt64())
							{
								throw new Exception();
							}
							if ((ulong)(buffer.Length * sizeof(byte)) != size.ToUInt64())
							{
								throw new Exception();
							}

							Jxl.JxlDecoderSetImageOutBuffer(decoder, &pixelFormat, output, (UIntPtr)(buffer.Length * sizeof(byte)));

							status = Jxl.JxlDecoderProcessInput(decoder);

							if (status != JxlDecoderStatus.JXL_DEC_FULL_IMAGE)
								throw new Exception();

							var image = new Image<TPixel>(configuration, (int)info.xsize, (int)info.ysize);

							var buf = MemoryMarshal.Cast<byte, Rgb24>(buffer);
							for (int y = 0; y < image.Height; y++)
								PixelOperations<TPixel>.Instance.FromRgb24(configuration, buf.Slice(image.Width * y, image.Width), image.Frames.RootFrame.PixelBuffer.DangerousGetRowSpan(y));

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

		public Image Decode(Configuration configuration, Stream stream, CancellationToken cancellationToken) => Decode<Rgb24>(configuration, stream, cancellationToken);

		public IImageInfo Identify(Configuration configuration, Stream stream, CancellationToken cancellationToken)
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
						Jxl.JxlDecoderSetInput(decoder, p, (UIntPtr)stream.Length);
						Jxl.JxlDecoderCloseInput(decoder);
						Jxl.JxlDecoderSubscribeEvents(decoder, (int)JxlDecoderStatus.JXL_DEC_BASIC_INFO);
						var status = Jxl.JxlDecoderProcessInput(decoder);
						if (status != JxlDecoderStatus.JXL_DEC_BASIC_INFO)
							throw new Exception();
						var info = new JxlBasicInfo();
						Jxl.JxlDecoderGetBasicInfo(decoder, &info);
						return new JpegXLImageInfo() { Width = (int)info.xsize, Height = (int)info.ysize, PixelType = new PixelTypeInfo((int)info.bits_per_sample) };
					}
				}
				finally
				{
					Jxl.JxlDecoderDestroy(decoder);
				}
			}
		}
	}
}
