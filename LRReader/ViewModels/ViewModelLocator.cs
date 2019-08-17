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
			SimpleIoc.Default.Register<ArchivePageViewModel>();
			SimpleIoc.Default.Register<ReaderPageViewModel>();
			SimpleIoc.Default.Register<SettingsPageViewModel>();
			SimpleIoc.Default.Register<StatisticsPageViewModel>();
		}

		public ArchivesPageViewModel ArchivesPageInstance
		{
			get => ServiceLocator.Current.GetInstance<ArchivesPageViewModel>();
		}
		public ArchivePageViewModel ArchivePageInstance
		{
			get => ServiceLocator.Current.GetInstance<ArchivePageViewModel>();
		}
		public ReaderPageViewModel ReaderPageInstance
		{
			get => ServiceLocator.Current.GetInstance<ReaderPageViewModel>();
		}
		public SettingsPageViewModel SettingsPageInstance
		{
			get => ServiceLocator.Current.GetInstance<SettingsPageViewModel>();
		}
		public StatisticsPageViewModel StatisticsPageInstance
		{
			get => ServiceLocator.Current.GetInstance<StatisticsPageViewModel>();
		}
	}
}
