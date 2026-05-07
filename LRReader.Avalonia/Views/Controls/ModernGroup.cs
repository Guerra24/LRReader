using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using LRReader.Avalonia.Extensions;

namespace LRReader.Avalonia.Views.Controls
{
	public sealed partial class ModernGroup : TemplatedControl
	{
		public ModernGroup()
		{
			Items = [];
		}

		protected override void OnLoaded(RoutedEventArgs e)
		{
			base.OnLoaded(e);

			this.SetRepositionAnimation();
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
