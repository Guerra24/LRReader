using LRReader.Shared.Internal;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using Microsoft.AppCenter.Crashes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LRReader.Shared.Tools
{

	public enum DeduplicatorStatus
	{
		PreloadAndDecode, Comparing, Cleanup, Completed
	}

	public class DeduplicatorParams : IToolParams
	{
		public int PixelThreshold { get; }
		public float PercentDifference { get; }
		public bool Grayscale { get; }
		public int Width { get; }
		public float AspectRatioLimit { get; }

		public DeduplicatorParams(int pixelThreshold = 30, float percentDifference = 0.2f, bool grayscale = false, int width = 8, float aspectRatioLimit = 0.1f)
		{
			PixelThreshold = pixelThreshold;
			PercentDifference = percentDifference;
			Grayscale = grayscale;
			Width = width;
			AspectRatioLimit = aspectRatioLimit;
		}
	}

	public class DeduplicationTool : Tool<DeduplicatorStatus, DeduplicatorParams, List<ArchiveHit>>
	{
		private readonly ImagesService Images;
		private readonly ArchivesService Archives;

		public DeduplicationTool(ImagesService images, ArchivesService archives)
		{
			Images = images;
			Archives = archives;
		}

		protected override async Task<List<ArchiveHit>> Process(DeduplicatorParams @params, int threads)
		{
			var factory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(threads));

			int pixelThreshold = @params.PixelThreshold;
			float percentDifference = @params.PercentDifference;
			bool grayscale = @params.Grayscale;
			int width = @params.Width;
			float aspectRatioLimit = @params.AspectRatioLimit;
			// Tweak values
			// Find better names for params
			var archives = Archives.Archives;
			UpdateProgress(DeduplicatorStatus.PreloadAndDecode, archives.Count, 0, 3, 0);
			await Task.Delay(2000);

			int count = 0;
			var tmp = (await Task.WhenAll(archives.Select(pair => factory.StartNew(async () =>
			{
				try
				{
					var image = Image.Load(await Images.GetThumbnailCached(pair.Key));
					image.Mutate(i => i.Resize(width, 0));
					UpdateProgress(DeduplicatorStatus.PreloadAndDecode, archives.Count, Interlocked.Increment(ref count));
					return new Tuple<string, Image<Rgba32>>(pair.Key, image);
				}
				catch (Exception e)
				{
					Crashes.TrackError(e);
					return null;
				}
			}).Unwrap()))).AsEnumerable().ToList();
			tmp.RemoveAll(pair => pair == null);

			var decodedThumbnails = tmp.ToDictionary(pair => pair.Item1, pair => pair.Item2);

			UpdateProgress(DeduplicatorStatus.PreloadAndDecode, archives.Count, count);
			GC.Collect(); // TODO GC
			GC.WaitForPendingFinalizers();
			GC.Collect();
			await Task.Delay(2000);

			count = 0;
			var comparatorDict = new ConcurrentDictionary<Tuple<string, string>, float>();

			UpdateProgress(DeduplicatorStatus.Comparing, decodedThumbnails.Count, 0, 3, 1);
			await Task.Delay(2000);
			var start = DateTime.Now;
			foreach (var sourcePair in decodedThumbnails)
			{
				var source = sourcePair.Value;
				await Task.WhenAll(decodedThumbnails.Select(targetPair => factory.StartNew(() =>
				{
					try
					{
						var fullKey = new Tuple<string, string>(sourcePair.Key, targetPair.Key);
						var fullKeyReversed = new Tuple<string, string>(targetPair.Key, sourcePair.Key);
						if (sourcePair.Key.Equals(targetPair.Key) || comparatorDict.ContainsKey(fullKey) || comparatorDict.ContainsKey(fullKeyReversed))
							return;
						var target = targetPair.Value;

						if (Math.Abs((float)source.Height / source.Width - (float)target.Height / target.Width) > aspectRatioLimit)
						{
							comparatorDict.TryAdd(fullKey, 1);
							return;
						}

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
						comparatorDict.TryAdd(fullKey, diffPixels);
					}
					catch (Exception e)
					{
						Crashes.TrackError(e);
					}
				})));
				GC.Collect(); // TODO GC
							  // Inaccurate AF
				var delta = DateTime.Now.Subtract(start);
				long time = (decodedThumbnails.Count - count) * (delta.Ticks / Math.Max(count, 1));
				UpdateProgress(DeduplicatorStatus.Comparing, decodedThumbnails.Count, Interlocked.Increment(ref count), time: time);
			}
			UpdateProgress(DeduplicatorStatus.Comparing, decodedThumbnails.Count, count, time: 0);
			await Task.Delay(2000);

			GC.Collect(); // TODO GC
			GC.WaitForPendingFinalizers();
			GC.Collect();

			count = 0;
			UpdateProgress(DeduplicatorStatus.Cleanup, decodedThumbnails.Count, 0, 3, 2);
			await Task.Delay(2000);
			await Task.WhenAll(decodedThumbnails.Select(thumb => factory.StartNew(() =>
			{
				thumb.Value.Dispose();
				UpdateProgress(DeduplicatorStatus.Cleanup, decodedThumbnails.Count, Interlocked.Increment(ref count));
			})));
			UpdateProgress(DeduplicatorStatus.Cleanup, decodedThumbnails.Count, count, 3, 3);
			decodedThumbnails.Clear();
			await Task.Delay(2000);

			var hits = new List<ArchiveHit>();
			foreach (var comp in comparatorDict)
			{
				if (comp.Value < percentDifference)
					hits.Add(new ArchiveHit { Left = Archives.GetArchive(comp.Key.Item1), Right = Archives.GetArchive(comp.Key.Item2), Percent = comp.Value });
			}
			comparatorDict.Clear();
			UpdateProgress(DeduplicatorStatus.Completed, 0, 0, 0, 0);
			return hits;
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
