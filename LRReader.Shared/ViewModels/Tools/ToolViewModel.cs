using LRReader.Shared.Services;
using LRReader.Shared.Tools;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace LRReader.Shared.ViewModels.Tools
{

	public abstract class ToolViewModel<T> : ObservableObject
	{
		protected readonly PlatformService Platform;

		public AsyncRelayCommand ExecuteCommand { get; }

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
			set
			{
				SetProperty(ref _currentProgress, value == -1 ? _currentProgress : value);
				OnPropertyChanged("Indeterminate");
			}
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
			set
			{
				SetProperty(ref _currentStep, value == -1 ? _currentStep : value);
				OnPropertyChanged("CurrentStepPlusOne");
			}
		}
		public int CurrentStepPlusOne => Math.Min(CurrentStep + 1, MaxSteps);
		public bool Indeterminate => CurrentProgress == -2;
		private object? _toolStatus;
		[DisallowNull]
		public object? ToolStatus
		{
			get
			{
				return Platform.GetLocalizedString(_toolStatus?.GetType().GetMember(_toolStatus.ToString())[0].GetCustomAttribute<DescriptionAttribute>(false).Description ?? "");
			}

			set => SetProperty(ref _toolStatus, value);
		}
		private string? _estimatedTime;
		[DisallowNull]
		public string? EstimatedTime
		{
			get => _estimatedTime;
			set => SetProperty(ref _estimatedTime, value);
		}
		private int _threads = Math.Max(Environment.ProcessorCount, 1);
		public int Threads
		{
			get => _threads;
			set => SetProperty(ref _threads, value);
		}
		private string? _errorTitle;
		public string? ErrorTitle
		{
			get => _errorTitle;
			set => SetProperty(ref _errorTitle, value);
		}
		private string? _errorDescription;
		public string? ErrorDescription
		{
			get => _errorDescription;
			set => SetProperty(ref _errorDescription, value);
		}

		protected Progress<ToolProgress<T>> Progress;

		public ToolViewModel(PlatformService platform)
		{
			Platform = platform;
			ExecuteCommand = new AsyncRelayCommand(Execute);
			Progress = new Progress<ToolProgress<T>>(p =>
			{
				ToolStatus = p.Status;
				MaxProgress = p.MaxProgress;
				CurrentProgress = p.CurrentProgress;
				MaxSteps = p.MaxSteps;
				CurrentStep = p.CurrentStep;
				if (p.Time > 0)
					EstimatedTime = TimeSpan.FromTicks(p.Time).ToString(@"hh\:mm\:ss");
				else
					EstimatedTime = "";
			});
		}

		protected abstract Task Execute();

	}

}
