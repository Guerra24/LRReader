using Avalonia.Markup.Xaml;
using LRReader.Avalonia.Resources;
using System;
using System.Text.RegularExpressions;

namespace LRReader.Avalonia.Extensions;

public sealed class LocalizedString : MarkupExtension
{
	public string Key { get; set; } = null!;

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		var split = Regex.Split(Key, @"\/(.*?)\/(.*)");
		return ResourceLoader.GetForCurrentView(split[1]).GetString(split[2]);
	}
}

public sealed class EnumValue : MarkupExtension
{

	public Type Type { get; set; } = null!;

	public string Member { get; set; } = null!;

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return Enum.Parse(Type, Member);
	}
}
