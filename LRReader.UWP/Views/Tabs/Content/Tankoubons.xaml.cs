﻿using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using RefreshContainer = Microsoft.UI.Xaml.Controls.RefreshContainer;
using RefreshRequestedEventArgs = Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs;

namespace LRReader.UWP.Views.Tabs.Content
{
	public sealed partial class Tankoubons : UserControl
	{
		public TankoubonsViewModel Data;

		private bool loaded;

		public Tankoubons()
		{
			this.InitializeComponent();
			Data = Service.Services.GetRequiredService<TankoubonsViewModel>();
		}

		private async void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (loaded)
				return;
			loaded = true;
			await Data.Refresh();
		}

		private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
		{
			using (var deferral = args.GetDeferral())
				await Data.Refresh();
		}

		private async void Refresh_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) => await Data.Refresh();

		private async void PagerControl_SelectedIndexChanged(PagerControl sender, PagerControlSelectedIndexChangedEventArgs args)
		{
			if (loaded)
				await Data.LoadPage(args.NewPageIndex);
		}

	}

	public partial class TankoubonTemplateSelector : DataTemplateSelector
	{
		public DataTemplate StaticTemplate { get; set; } = null!;
		public DataTemplate AddNewTemplate { get; set; } = null!;

		protected override DataTemplate SelectTemplateCore(object item)
		{
			if (item is AddNewTankoubon)
				return AddNewTemplate;
			if (item is Shared.Models.Main.Tankoubon)
				return StaticTemplate;
			return base.SelectTemplateCore(item);
		}
		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			return SelectTemplateCore(item);
		}
	}

}
