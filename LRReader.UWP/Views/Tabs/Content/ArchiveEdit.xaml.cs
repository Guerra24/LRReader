using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Tabs.Content
{
	public sealed partial class ArchiveEdit : UserControl
	{

		public ArchiveEditViewModel Data;

		public ArchiveEdit()
		{
			this.InitializeComponent();
			Data = DataContext as ArchiveEditViewModel;
		}

		public async void LoadArchive(Archive archive) => await Data.LoadArchive(archive);

		private void EditTag_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{

		}

		private void EditTag_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
			{
				var items = new List<string>();
				if (!string.IsNullOrEmpty(sender.Text))
				{
					var text = sender.Text.ToLower();
					foreach (var t in Service.Archives.TagStats.Where(t =>
					{
						var names = t.@namespace.ToLower();
						return t.GetNamespacedTag().ToLower().Contains(text) && !names.Equals("date_added") && !names.Equals("source");
					}))
					{
						items.Add(t.GetNamespacedTag());
					}
				}
				sender.ItemsSource = items;
			}
		}
	}

	public class TagTemplateSelector : DataTemplateSelector
	{
		public DataTemplate EditableTemplate { get; set; }
		public DataTemplate AddTemplate { get; set; }
		protected override DataTemplate SelectTemplateCore(object item)
		{
			if (item is AddTag)
				return AddTemplate;
			if (item is EditableTag)
				return EditableTemplate;
			return base.SelectTemplateCore(item);
		}
		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			return SelectTemplateCore(item);
		}
	}
}
