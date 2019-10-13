using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using CommonServiceLocator;
using LRReader.Internal;
using Windows.Storage;

namespace LRReader.ViewModels
{
	public class ViewModelLocator
	{
		public ViewModelLocator()
		{

			Global.Init(); // Init global static data

			ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

			SimpleIoc.Default.Register<ArchivesPageViewModel>();
			SimpleIoc.Default.Register<SettingsPageViewModel>();
			SimpleIoc.Default.Register<StatisticsPageViewModel>();
			SimpleIoc.Default.Register<HostTabPageViewModel>();
		}

		public ArchivesPageViewModel ArchivesPageInstance
		{
			get => ServiceLocator.Current.GetInstance<ArchivesPageViewModel>();
		}
		public SettingsPageViewModel SettingsPageInstance
		{
			get => ServiceLocator.Current.GetInstance<SettingsPageViewModel>();
		}
		public StatisticsPageViewModel StatisticsPageInstance
		{
			get => ServiceLocator.Current.GetInstance<StatisticsPageViewModel>();
		}
		public HostTabPageViewModel HostTabPageInstace
		{
			get => ServiceLocator.Current.GetInstance<HostTabPageViewModel>();
		}
	}
}
