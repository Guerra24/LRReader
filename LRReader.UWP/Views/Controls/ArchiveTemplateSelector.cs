using LRReader.Shared.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Controls
{
	public partial class ArchiveTemplateSelector : DataTemplateSelector
	{
		public DataTemplate CompactTemplate { get; set; } = null!;
		public DataTemplate FullTemplate { get; set; } = null!;
		public DataTemplate ThumbnailOnlyTemplate { get; set; } = null!;
		public ArchiveStyle Style { get; set; }

		protected override DataTemplate SelectTemplateCore(object item)
		{
			switch (Style)
			{
				case ArchiveStyle.Default:
					return FullTemplate;
				case ArchiveStyle.ThumbnailOnly:
					return ThumbnailOnlyTemplate;
				case ArchiveStyle.Compact:
					return CompactTemplate;
				default:
					return FullTemplate;
			}
		}
		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			return SelectTemplateCore(item);
		}
	}
}
