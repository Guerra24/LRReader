using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Controls
{
	public partial class ArchiveTemplateSelector : DataTemplateSelector
	{
		public DataTemplate CompactTemplate { get; set; } = null!;
		public DataTemplate FullTemplate { get; set; } = null!;
		public DataTemplate ThumbnailOnlyTemplate { get; set; } = null!;

		protected override DataTemplate SelectTemplateCore(object item)
		{
			//if (false)
			//return CompactTemplate;
			//else
			return FullTemplate;
		}
		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			return SelectTemplateCore(item);
		}
	}
}
