using LRReader.Shared.Internal;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LRReader.Shared.Tools
{

	public enum DeduplicatorStatus
	{
		[Description("Tools/Deduplicator/GenerateThumbnails")]
		GenerateThumbnails,
		[Description("Tools/Deduplicator/PreloadAndDecode")]
		PreloadAndDecode,
		[Description("Tools/Deduplicator/Comparing")]
		Comparing,
		[Description("Tools/Deduplicator/Completed")]
		Completed,
		[Description("Tools/Deduplicator/Ready")]
		Ready
	}

	public class DeduplicatorParams : IToolParams
	{
		public int PixelThreshold { get; }
		public float PercentDifference { get; }
		public int Width { get; }
		public float AspectRatioLimit { get; }
		public int Delay { get; }

		public DeduplicatorParams(int pixelThreshold = 30, float percentDifference = 0.2f, int width = 8, float aspectRatioLimit = 0.1f, int delay = 0)
		{
			PixelThreshold = pixelThreshold;
			PercentDifference = percentDifference;
			Width = width;
			AspectRatioLimit = aspectRatioLimit;
			Delay = delay;
		}
	}

	public class DeduplicationTool : Tool<DeduplicatorStatus, DeduplicatorParams, List<ArchiveHit>>
	{
		private readonly ImagesService Images;
		private readonly ArchivesService Archives;
		private readonly PlatformService Platform;

		public DeduplicationTool(ImagesService images, ArchivesService archives, PlatformService platform) : base(platform)
		{
			Images = images;
			Archives = archives;
			Platform = platform;
		}

		protected override async Task<ToolResult<List<ArchiveHit>>> Process(DeduplicatorParams @params, int threads)
		{
			var factory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(threads));
			earlyExit = false;

			int pixelThreshold = @params.PixelThreshold;
			float percentDifference = @params.PercentDifference;
			int width = @params.Width;
			float aspectRatioLimit = @params.AspectRatioLimit;
			int delay = @params.Delay;

			var archives = Archives.Archives;
			var thumbnailJob = await ArchivesProvider.RegenerateThumbnails();

			UpdateProgress(DeduplicatorStatus.GenerateThumbnails, archives.Count, -2, 3, 0);
			while (true)
			{
				await Task.Delay(1000);
				if ((await ServerProvider.GetMinionStatus(thumbnailJob.job)).state.Equals("finished"))
					break;
			}

			UpdateProgress(DeduplicatorStatus.PreloadAndDecode, archives.Count, 0, 3, 1);
			await Task.Delay(1000);

			int count = 0;
			var tmp = (await Task.WhenAll(archives.Select(pair => factory.StartNew(() =>
			{
				int tries = 5;
				Image<Rgba32>? image = null;
				while (tries > 0 && !earlyExit)
				{
					Thread.Sleep(delay * (6 - tries)); // TODO Good ol' Thread.Sleep
					var bytes = Task.Run(async () => await Images.GetThumbnailCached(pair.Key, ignoreCache: true)).GetAwaiter().GetResult();
					if (bytes != null)
					{
						image = Image.Load(bytes);
						image.Mutate(i => i.Resize(width, 0));
						break;
					}
					else
					{
						tries--;
					}
				}
				if (image == null)
					earlyExit = true;
				int itemCount = Interlocked.Increment(ref count);
				UpdateProgress(DeduplicatorStatus.PreloadAndDecode, archives.Count, itemCount);
				return new Tuple<string, Image<Rgba32>?>(pair.Key, image);
			})))).AsEnumerable().ToList();
			tmp.RemoveAll(pair => pair.Item2 == null);

			if (earlyExit)
				return EarlyExit(Platform.GetLocalizedString("Tools/Deduplicator/InvalidThumb/Title"), Platform.GetLocalizedString("Tools/Deduplicator/InvalidThumb/Message"));

			var decodedThumbnails = tmp.ToDictionary(pair => pair.Item1, pair => pair.Item2);
			tmp.Clear();

			UpdateProgress(DeduplicatorStatus.PreloadAndDecode, archives.Count, count);
			await Task.Delay(1000);

			count = 0;

			UpdateProgress(DeduplicatorStatus.Comparing, decodedThumbnails.Count, 0, 3, 2);
			await Task.Delay(1000);
			var start = DateTime.Now;

			var hits = new ConcurrentBag<ArchiveHit>();
			int maxItems = decodedThumbnails.Count;
			await Task.Run(async () =>
			{
				while (decodedThumbnails.Count != 0)
				{
					var sourcePair = decodedThumbnails.First();
					decodedThumbnails.Remove(sourcePair.Key);
					using (var source = sourcePair.Value)
					{
						await Task.WhenAll(decodedThumbnails.Select(targetPair => factory.StartNew(() =>
						{
							var target = targetPair.Value;
							if (Math.Abs((float)source.Height / source.Width - (float)target.Height / target.Width) > aspectRatioLimit)
								return;

							int differences = 0;
							for (int y = 0; y < Math.Min(source.Height, target.Height); y++)
							{
								Span<Rgba32> sourcePixelRow = source.GetPixelRowSpan(y);
								Span<Rgba32> targetPixelRow = target.GetPixelRowSpan(y);
								for (int x = 0; x < source.Width; x++)
								{
									float diff = GetManhattanDistanceInRgbSpace(ref sourcePixelRow[x], ref targetPixelRow[x]) / 765f; //255+255+255
									if (diff > pixelThreshold / 765f)
										differences++;
								}
							}
							float diffPixels = differences;
							diffPixels /= source.Width * source.Height;
							if (diffPixels < percentDifference)
								hits.Add(new ArchiveHit { Left = Archives.GetArchive(sourcePair.Key), Right = Archives.GetArchive(targetPair.Key) });
						})));
					}
					int itemCount = Interlocked.Increment(ref count);

					// Inaccurate AF
					var delta = DateTime.Now.Subtract(start);
					long time = (maxItems - itemCount) * (delta.Ticks / Math.Max(itemCount, 1));
					UpdateProgress(DeduplicatorStatus.Comparing, maxItems, itemCount, time: time);
				}
			});
			UpdateProgress(DeduplicatorStatus.Comparing, maxItems, count, time: 0);
			await Task.Delay(1000);
			UpdateProgress(DeduplicatorStatus.Completed, 0, 0, 3, 3);
			await Task.Delay(1000);
			UpdateProgress(DeduplicatorStatus.Completed, 0, 0, 0, 0);
			return new ToolResult<List<ArchiveHit>> { Data = hits.ToList(), Ok = true };
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int GetManhattanDistanceInRgbSpace(ref Rgba32 a, ref Rgba32 b)
		{
			return Diff(a.R, b.R) + Diff(a.G, b.G) + Diff(a.B, b.B);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int Diff(ushort a, ushort b) => Math.Abs(a - b);
	}

}
