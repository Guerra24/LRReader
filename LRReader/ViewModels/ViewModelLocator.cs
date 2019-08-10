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
			ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

			SimpleIoc.Default.Register<INavigationService, NavigationService>();
			SimpleIoc.Default.Register<ArchivesPageViewModel>();
			SimpleIoc.Default.Register<ArchivePageViewModel>();
			SimpleIoc.Default.Register<ReaderPageViewModel>();
		}

		public ArchivesPageViewModel ArchivesPageInstance
		{
			get
			{
				return ServiceLocator.Current.GetInstance<ArchivesPageViewModel>();
			}
		}

		public ArchivePageViewModel ArchivePageInstance
		{
			get
			{
				return ServiceLocator.Current.GetInstance<ArchivePageViewModel>();
			}
		}
		public ReaderPageViewModel ReaderPageInstance
		{
			get
			{
				return ServiceLocator.Current.GetInstance<ReaderPageViewModel>();
			}
		}
	}
}
