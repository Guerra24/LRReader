using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LRReader.Shared.Internal;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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
		public bool SkipMissing { get; }

		public DeduplicatorParams(int pixelThreshold = 30, float percentDifference = 0.2f, int width = 8, float aspectRatioLimit = 0.1f, int delay = 0, bool skipMissing = false)
		{
			PixelThreshold = pixelThreshold;
			PercentDifference = percentDifference;
			Width = width;
			AspectRatioLimit = aspectRatioLimit;
			Delay = delay;
			SkipMissing = skipMissing;
		}
	}

	public class DeduplicationTool : Tool<DeduplicatorStatus, DeduplicatorParams, List<ArchiveHit>, List<string>>
	{
		private readonly SettingsService Settings;
		private readonly ImagesService Images;
		private readonly ArchivesService Archives;
		private readonly PlatformService Platform;
		private readonly ILogger<DeduplicationTool> Logger;

		public DeduplicationTool(SettingsService settings, ImagesService images, ArchivesService archives, PlatformService platform, ILogger<DeduplicationTool> logger) : base(platform)
		{
			Settings = settings;
			Images = images;
			Archives = archives;
			Platform = platform;
			Logger = logger;
		}

		protected override async Task<ToolResult<List<ArchiveHit>, List<string>>> Process(DeduplicatorParams @params, int threads)
		{
			var factory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(threads));

			int pixelThreshold = @params.PixelThreshold;
			float percentDifference = @params.PercentDifference;
			int width = @params.Width;
			float aspectRatioLimit = @params.AspectRatioLimit;
			int delay = @params.Delay;
			bool skipMissing = @params.SkipMissing;

			var archives = Archives.Archives;
			var thumbnailJob = await ArchivesProvider.RegenerateThumbnails();
			if (thumbnailJob is null)
				return EarlyExit(Platform.GetLocalizedString("Tools/Deduplicator/NoThumbTask/Title"), Platform.GetLocalizedString("Tools/Deduplicator/NoThumbTask/Message"));

			UpdateProgress(DeduplicatorStatus.GenerateThumbnails, archives.Count, -2, 3, 0);
			while (true)
			{
				await Task.Delay(1000);
				if ((await ServerProvider.GetMinionStatus(thumbnailJob.job))?.state?.Equals("finished") ?? true)
					break;
			}

			UpdateProgress(DeduplicatorStatus.PreloadAndDecode, archives.Count, 0, 3, 1);
			await Task.Delay(1000);

			var tmpTimer = DateTime.Now;

			int count = 0;
			var tmp = (await Task.WhenAll(archives.Select(pair => factory.StartNew(() =>
			{
				int tries = 5;
				Image<Rgb24>? image = null;
				try
				{
					while (tries > 0)
					{
						Thread.Sleep(delay * (6 - tries)); // TODO Good ol' Thread.Sleep
						Logger.LogInformation("LoadThumb {0} {1}", pair.Key, tries);
						var bytes = Images.GetThumbnailCached(pair.Key).ConfigureAwait(false).GetAwaiter().GetResult();
						if (bytes != null)
						{
							image = Image.Load<Rgb24>(bytes);
							image.Mutate(i => i.Resize(width, 0));
							break;
						}
						else
						{
							tries--;
						}
					}
					int itemCount = Interlocked.Increment(ref count);
					if (DateTime.Now - tmpTimer > TimeSpan.FromSeconds(1))
					{
						tmpTimer = DateTime.Now;
						UpdateProgress(DeduplicatorStatus.PreloadAndDecode, archives.Count, itemCount);
					}
				}
				catch (Exception e)
				{
					Logger.LogInformation(e, "LoadThumb {0}", pair.Key);
				}
				return new Tuple<string, Image<Rgb24>?>(pair.Key, image);
			})))).AsEnumerable().ToList();

			List<string> removed = new List<string>();
			tmp.RemoveAll(pair =>
			{
				if (pair.Item2 == null)
					removed.Add(pair.Item1);
				return pair.Item2 == null;
			});

			if (removed.Count > 0 && !skipMissing)
				return EarlyExit(Platform.GetLocalizedString("Tools/Deduplicator/InvalidThumb/Title"), Platform.GetLocalizedString("Tools/Deduplicator/InvalidThumb/Message"), removed);

			var decodedThumbnails = tmp.ToDictionary(pair => pair.Item1, pair => pair.Item2);
			tmp.Clear();

			UpdateProgress(DeduplicatorStatus.PreloadAndDecode, archives.Count, count);
			await Task.Delay(1000);

			count = 0;

			UpdateProgress(DeduplicatorStatus.Comparing, decodedThumbnails.Count, 0, 3, 2);
			await Task.Delay(1000);

			tmpTimer = DateTime.Now;
			var markedNonDuplicated = Settings.Profile.MarkedAsNonDuplicated;
			var hits = new ConcurrentBag<ArchiveHit>();
			int maxItems = decodedThumbnails.Count;
			await Task.Run(async () =>
			{
				while (decodedThumbnails.Count != 0)
				{
					var start = DateTime.Now;
					var sourcePair = decodedThumbnails.First();
					decodedThumbnails.Remove(sourcePair.Key);
					using (var source = sourcePair.Value)
					{
						await Task.WhenAll(decodedThumbnails.Select(targetPair => factory.StartNew(() =>
						{
							var target = targetPair.Value;

							if (Math.Abs((float)source!.Height / source.Width - (float)target!.Height / target.Width) > aspectRatioLimit)
								return;

							var hit = new ArchiveHit { Left = sourcePair.Key, Right = targetPair.Key };
							if (markedNonDuplicated.Contains(hit))
								return;

							int differences = 0;

							source.ProcessPixelRows(target, (sourceAcc, targetAcc) =>
							{
								for (int y = 0; y < Math.Min(sourceAcc.Height, targetAcc.Height); y++)
								{
									Span<Rgb24> sourcePixelRow = sourceAcc.GetRowSpan(y);
									Span<Rgb24> targetPixelRow = targetAcc.GetRowSpan(y);
									for (int x = 0; x < sourceAcc.Width; x++)
									{
										float diff = GetManhattanDistanceInRgbSpace(ref sourcePixelRow[x], ref targetPixelRow[x]) / 765f; //255+255+255
										if (diff > pixelThreshold / 765f)
											differences++;
									}
								}
							});

							float diffPixels = differences;
							diffPixels /= source.Width * source.Height;
							if (diffPixels < percentDifference)
								hits.Add(hit);
						})));
					}
					int itemCount = Interlocked.Increment(ref count);
					if (DateTime.Now - tmpTimer > TimeSpan.FromSeconds(1))
					{
						tmpTimer = DateTime.Now;
						var delta = DateTime.Now.Subtract(start).Ticks;
						long time = (maxItems - itemCount) * delta;
						UpdateProgress(DeduplicatorStatus.Comparing, maxItems, itemCount, time: time);
					}
				}
			});
			UpdateProgress(DeduplicatorStatus.Comparing, maxItems, count, time: 0);
			await Task.Delay(1000);
			UpdateProgress(DeduplicatorStatus.Completed, 0, 0, 3, 3);
			await Task.Delay(1000);
			UpdateProgress(DeduplicatorStatus.Completed, 0, 0, 0, 0);

			return new ToolResult<List<ArchiveHit>, List<string>> { Data = hits.ToList(), Ok = true, Error = removed.Count > 0 ? removed : null };
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int GetManhattanDistanceInRgbSpace(ref Rgb24 a, ref Rgb24 b)
		{
			return Diff(a.R, b.R) + Diff(a.G, b.G) + Diff(a.B, b.B);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int Diff(ushort a, ushort b) => Math.Abs(a - b);
	}

}
