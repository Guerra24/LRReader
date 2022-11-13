#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using LRReader.UWP.Extensions;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace LRReader.UWP.Views.Tabs.Content
{
	public sealed partial class ArchiveEdit : UserControl
	{
		private static AnimationBuilder FadeIn = AnimationBuilder.Create().Opacity(to: 1, duration: TimeSpan.FromMilliseconds(150), easingMode: EasingMode.EaseIn);
		private static AnimationBuilder FadeOut = AnimationBuilder.Create().Opacity(to: 0, duration: TimeSpan.FromMilliseconds(150), easingMode: EasingMode.EaseOut);

		public ArchiveEditViewModel Data;

		public ArchiveEdit()
		{
			this.InitializeComponent();
			Data = (ArchiveEditViewModel)DataContext;
			Data.Show += Show;
			Data.Hide += Hide;
		}

		private Task Show(bool animate)
		{
			if (animate)
				Thumbnail.Start(FadeIn);
			else
				Thumbnail.SetVisualOpacity(1);
			return Task.CompletedTask;
		}

		private async Task Hide(bool animate)
		{
			if (animate)
				await Thumbnail.StartAsync(FadeOut);
			else
				Thumbnail.SetVisualOpacity(0);
		}

		public async void LoadArchive(Archive archive) => await Data.LoadArchive(archive);

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
		public DataTemplate EditableTemplate { get; set; } = null!;
		public DataTemplate AddTemplate { get; set; } = null!;
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
