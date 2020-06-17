using LRReader.Internal;
using LRReader.Shared.Models.Main;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LRReader.UWP.Views.Tabs
{
	public sealed partial class CategoryEditTab : CustomTab
	{
		private Category category;

		private bool loaded;

		public CategoryEditTab(Category category)
		{
			this.InitializeComponent();
			this.category = category;
			CustomTabId = "Edit_" + category.id;
			Header = category.name;
		}

		private void TabViewItem_Loaded(object sender, RoutedEventArgs e)
		{
			if (loaded)
				return;
			loaded = true;
			TabContent.LoadCategory(category);
		}
	}
}
