using JxlNet;
using LRReader.Shared.Internal;
using LRReader.Shared.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using WinRT;
using Size = System.Drawing.Size;

namespace LRReader.UWP.Services
{
	public class UWPImageProcessingService : ImageProcessingService
	{
		private readonly PlatformService Platform;

		//private readonly bool UseOsJpegXL;
		private TaskFactory TaskFactory;

		public UWPImageProcessingService(PlatformService platform)
		{
			Platform = platform;
			TaskFactory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(Math.Clamp(Environment.ProcessorCount / 4, 1, 4)));
		}

		public override async Task Init()
		{
			//UseOsJpegXL = Platform.WinRT_IsApiContractPresent("Windows.Foundation.UniversalApiContract", 19) && await Platform.CheckAppInstalled("Microsoft.JPEG-XLImageExtension_8wekyb3d8bbwe");
			await Task.CompletedTask;
		}

		[DynamicWindowsRuntimeCast(typeof(BitmapImage))]
		public override async Task<object?> ByteToBitmap(byte[]? bytes, int decodeWidth = 0, int decodeHeight = 0, object? img = default, CancellationToken cancellationToken = default)
		{
			if (bytes == null)
				return null;
			if (bytes.Length == 0)
				return null;

			var image = img as BitmapImage ?? new BitmapImage();

			image.DecodePixelType = DecodePixelType.Logical;
			if (decodeWidth > 0)
				image.DecodePixelWidth = decodeWidth;
			if (decodeHeight > 0)
				image.DecodePixelHeight = decodeHeight;
			try
			{
				if (/*!UseOsJpegXL && */IsJxl(bytes))
				{
					using (var converted = new InMemoryRandomAccessStream())
					{
						var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, converted);
						if (cancellationToken.IsCancellationRequested)
							return null;
						var ok = await TaskFactory.StartNew(() =>
						{
							if (cancellationToken.IsCancellationRequested)
								return false;
							unsafe
							{
								var runner = JxlThreads.JxlResizableParallelRunnerCreate(null);
								var decoder = Jxl.JxlDecoderCreate(null);
								try
								{
									Jxl.JxlDecoderSetParallelRunner(decoder, JxlThreads.JxlResizableParallelRunner, runner);
									if (cancellationToken.IsCancellationRequested)
										return false;
									fixed (byte* input = bytes)
									{
										Jxl.JxlDecoderSetInput(decoder, input, (nuint)bytes.Length);
										Jxl.JxlDecoderCloseInput(decoder);
										Jxl.JxlDecoderSubscribeEvents(decoder, (int)(JxlDecoderStatus.JXL_DEC_BASIC_INFO | JxlDecoderStatus.JXL_DEC_FRAME | JxlDecoderStatus.JXL_DEC_FULL_IMAGE));

										var status = Jxl.JxlDecoderProcessInput(decoder);

										if (cancellationToken.IsCancellationRequested)
											return false;
										if (status != JxlDecoderStatus.JXL_DEC_BASIC_INFO)
											return false;

										var info = new JxlBasicInfo();
										Jxl.JxlDecoderGetBasicInfo(decoder, &info);

										JxlThreads.JxlResizableParallelRunnerSetThreads(runner, JxlThreads.JxlResizableParallelRunnerSuggestThreads(info.xsize, info.ysize));

										status = Jxl.JxlDecoderProcessInput(decoder);

										if (cancellationToken.IsCancellationRequested)
											return false;
										if (status != JxlDecoderStatus.JXL_DEC_FRAME)
											return false;

										byte[] buffer = new byte[info.xsize * info.ysize * 4];
										fixed (byte* output = buffer)
										{
											var pixelFormat = new JxlPixelFormat();
											pixelFormat.data_type = JxlDataType.JXL_TYPE_UINT8;
											pixelFormat.endianness = JxlEndianness.JXL_NATIVE_ENDIAN;
											pixelFormat.num_channels = 4;
											pixelFormat.align = 0;

											nuint size = new();
											Jxl.JxlDecoderImageOutBufferSize(decoder, &pixelFormat, &size);

											if (info.xsize * info.ysize * sizeof(byte) * 4 != size.ToUInt64())
											{
												return false;
											}
											if ((ulong)(buffer.Length * sizeof(byte)) != size.ToUInt64())
											{
												return false;
											}

											if (cancellationToken.IsCancellationRequested)
												return false;

											Jxl.JxlDecoderSetImageOutBuffer(decoder, &pixelFormat, output, (nuint)(buffer.Length * sizeof(byte)));

											status = Jxl.JxlDecoderProcessInput(decoder);

											if (cancellationToken.IsCancellationRequested)
												return false;
											if (status != JxlDecoderStatus.JXL_DEC_FULL_IMAGE)
												return false;

											encoder.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Ignore, info.xsize, info.ysize, 96, 96, buffer);

											status = Jxl.JxlDecoderProcessInput(decoder);

											// JXL_DEC_FRAME means animation, we don't support that so display a single frame instead
											if (status != JxlDecoderStatus.JXL_DEC_SUCCESS && status != JxlDecoderStatus.JXL_DEC_FRAME)
												return false;
										}
									}
								}
								finally
								{
									JxlThreads.JxlResizableParallelRunnerDestroy(runner);
									Jxl.JxlDecoderDestroy(decoder);
								}
							}
							return true;
						});
						if (ok)
						{
							if (cancellationToken.IsCancellationRequested)
								return null;
							await encoder.FlushAsync();
							await image.SetSourceAsync(converted);
						}
						else
						{
							return null;
						}
					}
				}
				else
				{
					if (cancellationToken.IsCancellationRequested)
						return null;
					using var ms = new MemoryStream(bytes, 0, bytes.Length, false, true);
					using var stream = ms.AsRandomAccessStream();
					await image.SetSourceAsync(stream);
				}
			}
			catch
			{
				return null;
			}
			return image;
		}

		public override async Task<Size> GetImageSize(byte[]? bytes)
		{
			if (bytes == null)
				return Size.Empty;
			if (bytes.Length == 0)
				return Size.Empty;
			var size = await base.GetImageSize(bytes);
			if (size.IsEmpty)
			{
				try
				{
					using var ms = new MemoryStream(bytes, 0, bytes.Length, false, true);
					using var stream = ms.AsRandomAccessStream();
					var decoder = await BitmapDecoder.CreateAsync(stream);
					return new Size((int)decoder.PixelWidth, (int)decoder.PixelHeight);
				}
				catch
				{
					return Size.Empty;
				}
			}
			return size;
		}

		private bool IsJxl(byte[] bytes)
		{
			var jxlCodestream = bytes[0] == 0xff && bytes[1] == 0x0A;
			var jxlContainer = bytes[0] == 0 && bytes[1] == 0 && bytes[2] == 0 && bytes[3] == 0xC && bytes[4] == 'J' && bytes[5] == 'X' && bytes[6] == 'L' && bytes[7] == ' ' && bytes[8] == 0xD && bytes[9] == 0xA && bytes[10] == 0x87 && bytes[11] == 0xA;

			return jxlCodestream || jxlContainer;
		}

	}
}
