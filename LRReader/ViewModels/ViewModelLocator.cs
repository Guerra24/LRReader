using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using CommonServiceLocator;

namespace LRReader.ViewModels
{
	public class ViewModelLocator
	{
		public ViewModelLocator()
		{
			ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

			SimpleIoc.Default.Register<INavigationService, NavigationService>();
			SimpleIoc.Default.Register<MainPageViewModel>();
		}

		public MainPageViewModel MainPageInstance
		{
			get
			{
				return ServiceLocator.Current.GetInstance<MainPageViewModel>();
			}
		}
	}
}
