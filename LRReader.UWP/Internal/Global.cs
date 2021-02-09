using LRReader.Shared.Internal;
using LRReader.Shared.Models.Api;
using LRReader.UWP.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Internal
{
	public class Global : SharedGlobal
	{

		public new static EventManager EventManager { get; set; }
		public static ImageProcessing ImageProcessing { get; set; }

		public static void Init()
		{
			LRRApi = new LRRApi();
			SharedGlobal.EventManager = EventManager = new EventManager();
			SettingsManager = new SettingsManager();
			ArchivesManager = new ArchivesManager();
			ImageProcessing = new ImageProcessing();
		}

	}
}
