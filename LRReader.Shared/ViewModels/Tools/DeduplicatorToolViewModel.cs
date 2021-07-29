using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.Tools;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
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
		private float _aspectRatioLimit = 0.1f;
		public float AspectRatioLimit
		{
			get => _aspectRatioLimit;
			set => SetProperty(ref _aspectRatioLimit, (float)Math.Round(value, 2));
		}
		private int _delay = 25;
		public int Delay
		{
			get => _delay;
			set => SetProperty(ref _delay, value);
		}

		public ObservableCollection<ArchiveHit> Items = new ObservableCollection<ArchiveHit>();

		private AsyncRelayCommand<string> DeleteArchiveCommand;

		public DeduplicatorToolViewModel(DeduplicationTool deduplicator, IDispatcherService dispatcher, ArchivesService archives, IPlatformService platform) : base(platform)
		{
			ToolStatus = DeduplicatorStatus.Ready;
			Deduplicator = deduplicator;
			Dispatcher = dispatcher;
			Archives = archives;
			DeleteArchiveCommand = new AsyncRelayCommand<string>(DeleteArchive);
		}

		protected override async Task Execute()
		{
			// TODO Clean this
			ErrorTitle = null;
			ErrorDescription = null;
			Items.Clear();
			var hits = await Deduplicator.Execute(new DeduplicatorParams(PixelThreshold, PercentDifference / 100f, Resolution, AspectRatioLimit, Delay), Threads, Progress);
			if (hits.Ok)
			{
				await Task.Run(async () =>
				{
					foreach (var hit in hits.Data)
					{
						hit.Delete = DeleteArchiveCommand;
						await Dispatcher.RunAsync(() => Items.Add(hit));
					}
				});
				ToolStatus = DeduplicatorStatus.Ready;
			}
			else
			{
				ErrorTitle = hits.Title;
				ErrorDescription = hits.Description;
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
