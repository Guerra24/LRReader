using Avalonia.Controls.Primitives;
using Avalonia.Metadata;

namespace LRReader.Avalonia.Views.Controls
{
	public sealed partial class ModernGroup : TemplatedControl
	{
		public ModernGroup()
		{
			Items = [];
		}

		[Content]
		public IList<object> Items
		{
			get => GetValue(ItemsProperty);
			set => SetValue(ItemsProperty, value);
		}

		public string Title
		{
			get => GetValue(TitleProperty);
			set => SetValue(TitleProperty, value);
		}

		public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<ModernGroup, string>("Title", "");
		public static readonly StyledProperty<IList<object>> ItemsProperty = AvaloniaProperty.Register<ModernGroup, IList<object>>("Items");
	}
}
