using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.Tools;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels.Tools
{
	public class DeduplicatorToolViewModel : ToolViewModel<DeduplicatorStatus>
	{
		private readonly DeduplicationTool Deduplicator;
		private readonly IDispatcherService Dispatcher;

		private int _pixelThreshold = 30;
		public int PixelThreshold
		{
			get => _pixelThreshold;
			set => SetProperty(ref _pixelThreshold, value);
		}
		private int _percentDifference = 20;
		public int PercentDifference
		{
			get => _percentDifference;
			set => SetProperty(ref _percentDifference, value);
		}
		private int _resolution = 125;
		public int Resolution
		{
			get => _resolution;
			set => SetProperty(ref _resolution, value);
		}
		private bool _grayscale = true;
		public bool Grayscale
		{
			get => _grayscale;
			set => SetProperty(ref _grayscale, value);
		}
		private float _aspectRatioLimit = 0.1f;
		public float AspectRatioLimit
		{
			get => _aspectRatioLimit;
			set => SetProperty(ref _aspectRatioLimit, (float)Math.Round(value, 2));
		}

		public ObservableCollection<ArchiveHit> Items = new ObservableCollection<ArchiveHit>();

		public DeduplicatorToolViewModel(DeduplicationTool deduplicator, IDispatcherService dispatcher)
		{
			Deduplicator = deduplicator;
			Dispatcher = dispatcher;
		}

		protected override async Task Execute()
		{
			Items.Clear();
			var hits = await Deduplicator.Execute(new DeduplicatorParams(PixelThreshold, PercentDifference / 100f, Grayscale, Resolution, AspectRatioLimit), Progress);
			await Task.Run(async () =>
			{
				foreach (var hit in hits)
					await Dispatcher.RunAsync(() => Items.Add(hit));
			});
			ToolStatus = null;
		}

	}
}
