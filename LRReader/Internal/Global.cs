using LRReader.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Internal
{
	public static class Global
	{
		public static LRRApi LRRApi { get; set; }
		public static ImageManager ImageManager { get; set; }
		public static EventManager EventManager { get; set; }
		public static SettingsManager SettingsManager { get; set; }

		public static void Init()
		{
			LRRApi = new LRRApi();
			ImageManager = new ImageManager();
			ImageManager.Init();
			EventManager = new EventManager();
			SettingsManager = new SettingsManager();
		}

	}
}
