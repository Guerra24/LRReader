using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.Tools;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels
{

	public enum Tools
	{
		Deduplicate
	}

	public class ToolsViewModel : ObservableObject
	{
		private readonly DeduplicationTool Deduplicator;
		private readonly IDispatcherService Dispatcher;

		public AsyncRelayCommand<Tools> ExecuteCommand { get; }

		private int _maxProgress;
		public int MaxProgress
		{
			get => _maxProgress;
			set => SetProperty(ref _maxProgress, value == -1 ? _maxProgress : value);
		}
		private int _currentProgress;
		public int CurrentProgress
		{
			get => _currentProgress;
			set => SetProperty(ref _currentProgress, value == -1 ? _currentProgress : value);
		}
		private int _maxSteps;
		public int MaxSteps
		{
			get => _maxSteps;
			set => SetProperty(ref _maxSteps, value == -1 ? _maxSteps : value);
		}
		private int _currentStep;
		public int CurrentStep
		{
			get => _currentStep;
			set => SetProperty(ref _currentStep, value == -1 ? _currentStep : value);
		}
		private object _toolStatus;
		public object ToolStatus
		{
			get => _toolStatus;
			set => SetProperty(ref _toolStatus, value);
		}
		private string _estimatedTime;
		public string EstimatedTime
		{
			get => _estimatedTime;
			set => SetProperty(ref _estimatedTime, value);
		}
		// Tool Params
		private int _pixelThreshold = 10;
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

		public ObservableCollection<ArchiveHit> Items = new ObservableCollection<ArchiveHit>();

		public ToolsViewModel(DeduplicationTool deduplicator, IDispatcherService dispatcher)
		{
			Deduplicator = deduplicator;
			Dispatcher = dispatcher;

			ExecuteCommand = new AsyncRelayCommand<Tools>(Execute);
		}

		private async Task Execute(Tools tool)
		{
			Items.Clear();
			switch (tool)
			{
				case Tools.Deduplicate:
					var hits = await Deduplicator.DeduplicateArchives(new Progress<ToolProgress<DeduplicatorStatus>>(p =>
					{
						ToolStatus = p.Status;
						MaxProgress = p.MaxProgress;
						CurrentProgress = p.CurrentProgress;
						MaxSteps = p.MaxSteps;
						CurrentStep = p.CurrentStep;
						EstimatedTime = TimeSpan.FromTicks(p.Time).ToString(@"hh\:mm\:ss");
					}), PixelThreshold, PercentDifference / 100f, Grayscale, Resolution);
					await Task.Run(async () =>
					{
						foreach (var hit in hits)
							await Dispatcher.RunAsync(() => Items.Add(hit));
					});
					ToolStatus = null;
					break;
			};
		}

	}

}
