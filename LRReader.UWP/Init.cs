using LRReader.Internal;
using LRReader.Shared.Internal;
using LRReader.UWP.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.UWP
{
	public static class Init
	{
		public static void InitObjects()
		{
			SharedGlobal.SettingsStorage = new SettingsStorage();
			Global.Init(); // Init global static data
		}
	}
}
