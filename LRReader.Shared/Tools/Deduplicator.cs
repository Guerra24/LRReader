using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LRReader.Shared.Tools
{
	public class DeduplicationTool
	{
		private readonly ImagesService Images;
		private readonly ArchivesService Archives;

		private Subject<ToolProgress<DeduplicatorStatus>> progressFilter;

		public DeduplicationTool(ImagesService images, ArchivesService archives)
		{
			Images = images;
			Archives = archives;
		}

		public async Task<List<ArchiveHit>> DeduplicateArchives(IProgress<ToolProgress<DeduplicatorStatus>> progress = null, int pixelThreshold = 30, float percentDifference = 0.2f, bool grayscale = false, int width = 125, float aspectRatioLimit = 0.1f)
		{
			// Tweak values
			// Find better names for params

			progressFilter = new Subject<ToolProgress<DeduplicatorStatus>>();
			progressFilter.Window(TimeSpan.FromMilliseconds(1000)).SelectMany(i => i.TakeLast(1)).Subscribe(p => progress?.Report(p));

			var archives = Archives.Archives;
			UpdateProgress(new ToolProgress<DeduplicatorStatus>(DeduplicatorStatus.PreloadAndDecode, archives.Count, 0, 3, 0));
			await Task.Delay(2000);

			int count = 0;
			var decodedThumbnails = (await Task.WhenAll(archives.Select(pair => Task.Run(async () =>
			{
				var image = Image.Load(await Images.GetThumbnailCached(pair.Key));
				image.Mutate(i => i.Grayscale().Resize(width, 0));
				UpdateProgress(new ToolProgress<DeduplicatorStatus>(DeduplicatorStatus.PreloadAndDecode, archives.Count, Interlocked.Increment(ref count)));
				return new Tuple<string, Image<Rgba32>>(pair.Key, image);
			})))).AsEnumerable().ToDictionary(pair => pair.Item1, pair => pair.Item2);

			UpdateProgress(new ToolProgress<DeduplicatorStatus>(DeduplicatorStatus.PreloadAndDecode, archives.Count, count));
			await Task.Delay(2000);

			count = 0;
			var comparatorDict = new ConcurrentDictionary<Tuple<string, string>, float>();

			UpdateProgress(new ToolProgress<DeduplicatorStatus>(DeduplicatorStatus.Comparing, decodedThumbnails.Count, 0, 3, 1));
			await Task.Delay(2000);
			var start = DateTime.Now;
			foreach (var sourcePair in decodedThumbnails)
			{
				var source = sourcePair.Value;
				await Task.WhenAll(decodedThumbnails.Select(targetPair => Task.Run(() =>
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
							//if (grayscale)
							//{
							byte diff = (byte)Math.Abs(sourcePixelRow[x].R - targetPixelRow[x].R);
							if (diff > pixelThreshold)
								differences++;
							/*}
							else
							{
								float diff = GetManhattanDistanceInRgbSpace(ref sourcePixelRow[x], ref targetPixelRow[x]) / 765f; //255+255+255
								if (diff > pixelThreshold / 765f)
									differences++;
							}*/
						}
					}
					float diffPixels = differences;
					diffPixels /= source.Width * source.Height;
					comparatorDict.TryAdd(fullKey, diffPixels);
				})));
				// Inaccurate AF
				var delta = DateTime.Now.Subtract(start);
				long time = (decodedThumbnails.Count - count) * (delta.Ticks / Math.Max(count, 1));
				UpdateProgress(new ToolProgress<DeduplicatorStatus>(DeduplicatorStatus.Comparing, decodedThumbnails.Count, Interlocked.Increment(ref count), time: time));
			}
			UpdateProgress(new ToolProgress<DeduplicatorStatus>(DeduplicatorStatus.Comparing, decodedThumbnails.Count, count, time: 0));
			await Task.Delay(2000);

			count = 0;

			UpdateProgress(new ToolProgress<DeduplicatorStatus>(DeduplicatorStatus.Cleanup, decodedThumbnails.Count, 0, 3, 2));
			await Task.Delay(2000);
			await Task.WhenAll(decodedThumbnails.Select(thumb => Task.Run(() =>
			{
				thumb.Value.Dispose();
				UpdateProgress(new ToolProgress<DeduplicatorStatus>(DeduplicatorStatus.Cleanup, decodedThumbnails.Count, Interlocked.Increment(ref count)));
			})));
			var hits = new List<ArchiveHit>();
			foreach (var comp in comparatorDict)
			{
				if (comp.Value < percentDifference)
					hits.Add(new ArchiveHit { Left = Archives.GetArchive(comp.Key.Item1), Right = Archives.GetArchive(comp.Key.Item2), Percent = comp.Value });
			}
			UpdateProgress(new ToolProgress<DeduplicatorStatus>(DeduplicatorStatus.Cleanup, decodedThumbnails.Count, count, 3, 3));
			await Task.Delay(2000);
			UpdateProgress(new ToolProgress<DeduplicatorStatus>(DeduplicatorStatus.Completed, 0, 0, 0, 0));
			progressFilter.OnCompleted();
			return hits;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void UpdateProgress(ToolProgress<DeduplicatorStatus> progress) => progressFilter.OnNext(progress);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int GetManhattanDistanceInRgbSpace(ref Rgba32 a, ref Rgba32 b)
		{
			return Diff(a.R, b.R) + Diff(a.G, b.G) + Diff(a.B, b.B);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int Diff(ushort a, ushort b) => Math.Abs(a - b);
	}

	public enum DeduplicatorStatus
	{
		PreloadAndDecode, Comparing, Cleanup, Completed
	}

}
