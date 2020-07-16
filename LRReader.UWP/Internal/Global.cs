using LRReader.Shared.Internal;
using LRReader.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Internal
{
	public class Global : SharedGlobal
	{

		public static ImageManager ImageManager { get; set; }
		public new static EventManager EventManager { get; set; }

		public static void Init()
		{
			LRRApi = new LRRApi();
			ImageManager = new ImageManager();
			ImageManager.Init();
			SharedGlobal.EventManager = EventManager = new EventManager();
			SettingsManager = new SettingsManager();
			ArchivesManager = new ArchivesManager();
		}

	}
}
