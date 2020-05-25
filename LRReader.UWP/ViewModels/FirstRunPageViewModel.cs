using GalaSoft.MvvmLight;
using LRReader.Internal;
using LRReader.Shared.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.UWP.ViewModels
{
	public class FirstRunPageViewModel : ViewModelBase
	{
		public SettingsManager SettingsManager
		{
			get => Global.SettingsManager;
		}
	}
}
