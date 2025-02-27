using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.Tools;
using LRReader.Shared.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;

namespace LRReader.Shared.ViewModels.Tools
{
	public partial class DeduplicatorHiddenViewModel : ObservableObject
	{
		private readonly SettingsService Settings;

		public ObservableCollection<ArchiveHit> HiddenArchives = new ObservableCollection<ArchiveHit>();

		public DeduplicatorHiddenViewModel(SettingsService settings)
		{
			Settings = settings;
		}

		public void Refresh()
		{
			HiddenArchives.Clear();
			foreach (var item in Settings.Profile.MarkedAsNonDuplicated)
				HiddenArchives.Add(item);
		}

		[RelayCommand]
		private void Remove(ArchiveHit item)
		{
			HiddenArchives.Remove(item);
			Settings.Profile.MarkedAsNonDuplicated.Remove(item);
			Settings.SaveProfiles();
		}
	}

	[UnconditionalSuppressMessage("AOT", "IL3050")]
	public partial class DeduplicatorToolViewModel : ToolViewModel<DeduplicatorStatus>
	{
		private readonly SettingsService Settings;
		private readonly DeduplicationTool Deduplicator;
		private readonly ArchivesService Archives;
		private readonly IDispatcherService Dispatcher;

		[ObservableProperty]
		private int _pixelThreshold = 30;
		[ObservableProperty]
		private int _percentDifference = 20;
		[ObservableProperty]
		private int _resolution = 8;
		private float _aspectRatioLimit = 0.1f;
		public float AspectRatioLimit
		{
			get => _aspectRatioLimit;
			set => SetProperty(ref _aspectRatioLimit, (float)Math.Round(value, 2));
		}
		[ObservableProperty]
		private bool _skipMissing;
		[ObservableProperty]
		private int _delay = 25;

		public ObservableCollection<ArchiveHit> Items = new ObservableCollection<ArchiveHit>();
		public ObservableCollection<Archive> Missing = new ObservableCollection<Archive>();

		public ArchiveHitPreviewViewModel LeftArchive, RightArchive;

		private ArchiveHit _current = null!;

		[ObservableProperty]
		private bool _canClosePreviews;

		public DeduplicatorToolViewModel(SettingsService settings, DeduplicationTool deduplicator, IDispatcherService dispatcher, ArchivesService archives, PlatformService platform) : base(platform)
		{
			ToolStatus = DeduplicatorStatus.Ready;
			Settings = settings;
			Deduplicator = deduplicator;
			Dispatcher = dispatcher;
			Archives = archives;
			LeftArchive = Service.Services.GetRequiredService<ArchiveHitPreviewViewModel>();
			RightArchive = Service.Services.GetRequiredService<ArchiveHitPreviewViewModel>();
		}

		protected override async Task Execute()
		{
			// TODO Clean this
			ErrorTitle = null;
			ErrorDescription = null;
			Items.Clear();
			Missing.Clear();
			OnPropertyChanged("Missing");
			var hits = await Deduplicator.Execute(new DeduplicatorParams(PixelThreshold, PercentDifference / 100f, Resolution, AspectRatioLimit, Delay, SkipMissing), Threads, Progress);
			if (hits.Ok)
			{
				await Task.Run(async () =>
				{
					foreach (var hit in hits.Data)
						await Dispatcher.RunAsync(() => Items.Add(hit));
				});
				ToolStatus = DeduplicatorStatus.Ready;
			}
			else
			{
				ErrorTitle = hits.Title;
				ErrorDescription = hits.Description;
			}
			if (hits.Error != null)
			{
				foreach (var item in hits.Error)
				{
					var archive = Archives.GetArchive(item);
					if (archive != null)
						Missing.Add(archive);
				}
				OnPropertyChanged("Missing");
			}
		}

		[RelayCommand]
		private async Task DeleteArchive(string arcid)
		{
			if (await Archives.DeleteArchive(arcid))
				foreach (var item in Items.Where(hit => hit.Left.Equals(arcid) || hit.Right.Equals(arcid)).ToList())
					Items.Remove(item);
		}

		[RelayCommand]
		private void MarkNonDup() => MarkNonDupValue(_current);

		[RelayCommand]
		private void MarkNonDupValue(ArchiveHit archiveHit)
		{
			Settings.Profile.MarkedAsNonDuplicated.Add(archiveHit);
			Items.Remove(archiveHit);
			Settings.SaveProfiles();
		}

		public async Task LoadArchives(ArchiveHit hit)
		{
			CanClosePreviews = false;
			try
			{
				_current = hit;
				var lArchive = Archives.GetArchive(hit.Left);
				var rArchive = Archives.GetArchive(hit.Right);
				if (lArchive is null || rArchive is null)
					return;
				LeftArchive.Archive = lArchive;
				RightArchive.Archive = rArchive;
				await Task.WhenAll(LeftArchive.Reload(), RightArchive.Reload());
			}
			catch { }
			finally
			{
				CanClosePreviews = true;
			}
		}

	}
}
