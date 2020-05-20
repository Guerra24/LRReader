using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using LRReader.Internal;
using LRReader.Shared.Internal;
using Windows.Storage;

namespace LRReader.UWP.ViewModels
{
	public class ViewModelLocator
	{
		public ViewModelLocator()
		{
			Global.Init(); // Init global static data

			SimpleIoc.Default.Register<ArchivesPageViewModel>();
			SimpleIoc.Default.Register<SettingsPageViewModel>();
			SimpleIoc.Default.Register<HostTabPageViewModel>();
			SimpleIoc.Default.Register<BookmarksTabViewModel>();
			SimpleIoc.Default.Register<FirstRunPageViewModel>();
			SimpleIoc.Default.Register<CategoriesViewModel>();
		}

		public ArchivesPageViewModel ArchivesPageInstance => SimpleIoc.Default.GetInstance<ArchivesPageViewModel>();
		public SettingsPageViewModel SettingsPageInstance => SimpleIoc.Default.GetInstance<SettingsPageViewModel>();
		public HostTabPageViewModel HostTabPageInstace => SimpleIoc.Default.GetInstance<HostTabPageViewModel>();
		public BookmarksTabViewModel BookmarksTabInstance => SimpleIoc.Default.GetInstance<BookmarksTabViewModel>();
		public FirstRunPageViewModel FirstRunPageInstance => SimpleIoc.Default.GetInstance<FirstRunPageViewModel>();
		public CategoriesViewModel CategoriesTabInstance => SimpleIoc.Default.GetInstance<CategoriesViewModel>();
	}
}
