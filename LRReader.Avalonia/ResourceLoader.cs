using Avalonia;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Resources;
using System.Xml;

namespace LRReader.Avalonia
{
	class ResourceLoader
	{

		private Dictionary<string, string> Lang = new Dictionary<string, string>();

		public ResourceLoader(string file)
		{
			var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
			Uri uri = new Uri($"avares://LRReader.Avalonia/Strings/en/{file}.resw");
			if (assets.Exists(uri))
			{
				var xml = new XmlTextReader(assets.Open(uri));
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
								Lang.Add(key, xml.ReadString());
							}
							break;
					}
				}
			}
		}

		public string GetString(string key)
		{
			string value;
			if (!Lang.TryGetValue(key, out value))
				return key;
			return value;
		}

		private static Dictionary<string, ResourceLoader> Resources = new Dictionary<string, ResourceLoader>();

		public static ResourceLoader GetForCurrentView(string file)
		{
			ResourceLoader loader;
			if (!Resources.TryGetValue(file, out loader))
			{
				loader = new ResourceLoader(file);
				Resources.Add(file, loader);
			}
			return loader;
		}

	}
}
