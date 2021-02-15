using LRReader.Shared.Models.Main;
using Microsoft.Toolkit.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LRReader.UWP.Views.Dialogs
{
	public sealed partial class ProgressConflict : ContentDialog
	{

		public ConflictMode Mode;
		
		public ProgressConflict(int local, int remote, int total)
		{
			this.InitializeComponent();

			var lang = ResourceLoader.GetForCurrentView("Dialogs");
			ProgressOptions.Items.Add(lang.GetString("ProgressConflict/Local").AsFormat(local, total));
			ProgressOptions.Items.Add(lang.GetString("ProgressConflict/Remote").AsFormat(remote, total));

			/*
			<x:String>Use local (1/20)</x:String>
			<x:String>Use remote (5/20)</x:String>
			*/
		}

	}
}
