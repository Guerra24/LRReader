using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LRReader.Shared.Services;
using LRReader.Shared.Tools;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels.Tools
{

	public abstract partial class ToolViewModel<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : ObservableObject  where T : Enum
	{
		protected readonly PlatformService Platform;

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
		[ObservableProperty]
		[NotifyPropertyChangedFor(nameof(ToolStatusMessage))]
		private T _toolStatus = default!;
		public string ToolStatusMessage => Platform.GetLocalizedString(typeof(T).GetMember(ToolStatus!.ToString()!)[0].GetCustomAttribute<DescriptionAttribute>(false)?.Description ?? "");
		[ObservableProperty]
		private string _estimatedTime = "";
		[ObservableProperty]
		private int _threads = Math.Max(Environment.ProcessorCount, 1);
		[ObservableProperty]
		private string? _errorTitle;
		[ObservableProperty]
		private string? _errorDescription;

		protected Progress<ToolProgress<T>> Progress;

		public ToolViewModel(PlatformService platform)
		{
			Platform = platform;
			Progress = new Progress<ToolProgress<T>>(p =>
			{
				ToolStatus = p.Status!;
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

		[RelayCommand]
		protected abstract Task Execute();

	}

}
