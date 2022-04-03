using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Avalonia.Markup.Xaml;

namespace LRReader.Avalonia
{
	class ResourceLoader
	{

		private Dictionary<string, string> Lang = new Dictionary<string, string>();

		public ResourceLoader(string file)
		{
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"LRReader.Avalonia.Strings.en.{file}.resw");
			if (stream == null)
				return;
			using (stream)
			{
				var xml = new XmlTextReader(stream);
				while (xml.Read())
				{
					switch (xml.NodeType)
					{
						case XmlNodeType.Element:
							if (xml.Name.Equals("data"))
							{
								var key = xml.GetAttribute("name");
								xml.Read();
								xml.Read();
								Lang.Add(key!.Replace('.', '/'), xml.ReadString());
							}
							break;
					}
				}
			}
		}

		public string GetString(string key)
		{
			if (!Lang.TryGetValue(key, out var value))
				return key;
			return value;
		}

		private static Dictionary<string, ResourceLoader> Resources = new Dictionary<string, ResourceLoader>();

		public static ResourceLoader GetForCurrentView(string file)
		{
			if (!Resources.TryGetValue(file, out var loader))
			{
				loader = new ResourceLoader(file);
				Resources.Add(file, loader);
			}
			return loader;
		}

	}

	public sealed class LocalizedString : MarkupExtension
	{
		public string Key { get; set; } = null!;

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			var split = Regex.Split(Key, @"\/(.*?)\/(.*)");
			return ResourceLoader.GetForCurrentView(split[1]).GetString(split[2]);
		}
	}
}
