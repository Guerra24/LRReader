using LRReader.Shared.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace LRReader.UWP.ViewModels
{
	public class FirstRunPageViewModel : ObservableObject
	{
		public SettingsService SettingsManager
		{
			get => Service.Settings;
		}
	}
}
