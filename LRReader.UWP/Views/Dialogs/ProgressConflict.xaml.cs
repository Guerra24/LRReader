using LRReader.Shared.Extensions;
using LRReader.Shared.Models;
using LRReader.Shared.Services;
using LRReader.UWP.Extensions;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Dialogs
{
	public sealed partial class ProgressConflict : ContentDialog, IProgressConflictDialog
	{

		public ProgressConflict(int local, int remote, int total)
		{
			this.InitializeComponent();
			RequestedTheme = Service.Platform.Theme.ToXamlTheme();

			var lang = ResourceLoader.GetForCurrentView("Dialogs");
			ProgressOptions.Items.Add(lang.GetString("ProgressConflict/Local").AsFormat(local, total));
			ProgressOptions.Items.Add(lang.GetString("ProgressConflict/Remote").AsFormat(remote, total));

			/*
			<x:String>Use local (1/20)</x:String>
			<x:String>Use remote (5/20)</x:String>
			*/
		}

		public ConflictMode Mode { get; set; }

		public new async Task<IDialogResult> ShowAsync() => (IDialogResult)(int)await base.ShowAsync();

	}
}
