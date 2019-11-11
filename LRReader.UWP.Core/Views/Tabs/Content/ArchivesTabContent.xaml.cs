using LRReader.Internal;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LRReader.Views.Tabs.Content
{
	public sealed partial class ArchivesTabContent : UserControl
	{

		private ArchivesPageViewModel Data;

		private bool loaded;

		private string query = "";

		public ArchivesTabContent()
		{
			this.InitializeComponent();
			Data = DataContext as ArchivesPageViewModel;
		}

		private async void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (loaded)
				return;
			loaded = true;
			await Data.Refresh();
			await Data.LoadTagStats();
		}

		private void ArchivesGrid_ItemClick(object sender, ItemClickEventArgs e)
		{
			Global.EventManager.AddTab(new ArchiveTab(e.ClickedItem as Archive), Global.SettingsManager.SwitchTabArchive);
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			await Data.Refresh();
			await Data.LoadTagStats();
			HandleSearch();
		}

		public void SearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
			{
				Data.Suggestions.Clear();
				if (!string.IsNullOrEmpty(sender.Text))
				{
					string text;
					var sQuery = sender.Text.ToUpper();
					if (sender.Text.Length > query.Length)
					{
						text = sQuery.Substring(query.Length).TrimStart();
					}
					else
					{
						text = sQuery.Split(" ").Last();
						query = sender.Text.Substring(0, Math.Max(0, sQuery.LastIndexOf(" ")));
					}
					foreach (var t in Data.TagStats.Where(t => t.GetNamespacedTag().ToUpper().Contains(text)))
					{
						Data.Suggestions.Add(query.TrimEnd() + (string.IsNullOrEmpty(query) ? "" : " ") + t.GetNamespacedTag());
					}
				}
				else
				{
					query = "";
					HandleSearch();
				}
			}
		}

		public void SearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{
			if (args.ChosenSuggestion != null)
			{
				query = args.ChosenSuggestion as string;
				HandleSearch();
			}
			else
			{
				query = args.QueryText;
				HandleSearch();
			}
		}

		private async void HandleSearch()
		{
			Data.Query = query;
			await Data.ReloadSearch();
		}

		private void RandomButton_Click(object sender, RoutedEventArgs e)
		{
			var random = new Random();
			var list = ArchivesGrid.ItemsSource as IEnumerable<Archive>;
			var item = list.ElementAt(random.Next(list.Count()));
			Global.EventManager.AddTab(new ArchiveTab(item));
		}

		private async void NewOnlyButton_Click(object sender, RoutedEventArgs e)
		{
			await Data.ReloadSearch();
		}

		private void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
		{
			using (var deferral = args.GetDeferral())
			{
				HandleSearch();
			}
		}

		private void Refresh_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			HandleSearch();
			args.Handled = true;
		}

		private async void RefreshFull_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			await Data.Refresh();
			await Data.LoadTagStats();
			args.Handled = true;
		}

		private async void PrevButton_Click(object sender, RoutedEventArgs e)
		{
			await Data.PrevPage();
		}

		private async void NextButton_Click(object sender, RoutedEventArgs e)
		{
			await Data.NextPage();
		}
	}
}
