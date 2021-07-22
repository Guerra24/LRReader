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

namespace LRReader.Shared.ViewModels.Tools
{

	public abstract class ToolViewModel<T> : ObservableObject
	{
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

		protected Progress<ToolProgress<T>> Progress;

		public ToolViewModel()
		{
			ExecuteCommand = new AsyncRelayCommand(Execute);
			Progress = new Progress<ToolProgress<T>>(p =>
			{
				ToolStatus = p.Status;
				MaxProgress = p.MaxProgress;
				CurrentProgress = p.CurrentProgress;
				MaxSteps = p.MaxSteps;
				CurrentStep = p.CurrentStep;
				EstimatedTime = TimeSpan.FromTicks(p.Time).ToString(@"hh\:mm\:ss");
			});
		}

		protected abstract Task Execute();

	}

}
