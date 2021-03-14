using LRReader.Shared.Models.Main;
using LRReader.UWP.Extensions;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

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
