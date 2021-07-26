using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.Tools;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels.Tools
{
	public class DeduplicatorToolViewModel : ToolViewModel<DeduplicatorStatus>
	{
		private readonly DeduplicationTool Deduplicator;
		private readonly ArchivesService Archives;
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
		private int _resolution = 8;
		public int Resolution
		{
			get => _resolution;
			set => SetProperty(ref _resolution, value);
		}
		private bool _grayscale;
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

		private AsyncRelayCommand<string> DeleteArchiveCommand;

		public DeduplicatorToolViewModel(DeduplicationTool deduplicator, IDispatcherService dispatcher, ArchivesService archives)
		{
			Deduplicator = deduplicator;
			Dispatcher = dispatcher;
			Archives = archives;
			DeleteArchiveCommand = new AsyncRelayCommand<string>(DeleteArchive);
		}

		protected override async Task Execute()
		{
			Items.Clear();
			var hits = await Deduplicator.Execute(new DeduplicatorParams(PixelThreshold, PercentDifference / 100f, Grayscale, Resolution, AspectRatioLimit), Threads, Progress);
			if (hits != null)
			{
				await Task.Run(async () =>
				{
					foreach (var hit in hits)
					{
						hit.Delete = DeleteArchiveCommand;
						await Dispatcher.RunAsync(() => Items.Add(hit));
					}
				});
				ToolStatus = null;
			}
		}

		private async Task DeleteArchive(string arcid)
		{
			if (await Archives.DeleteArchive(arcid))
				foreach (var item in Items.Where(hit => hit.Left.arcid.Equals(arcid) || hit.Right.arcid.Equals(arcid)).ToList())
					Items.Remove(item);
		}

	}
}
