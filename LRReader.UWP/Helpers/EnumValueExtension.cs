using System;
using Windows.UI.Xaml.Markup;

namespace LRReader.UWP.Helpers
{
	[MarkupExtensionReturnType(ReturnType = typeof(object))]
	public partial class EnumValueExtension : MarkupExtension
	{
		public Type Type { get; set; } = null!;

		public string Member { get; set; } = null!;

		protected override object ProvideValue()
		{
			return Enum.Parse(Type, Member);
		}
	}
}
