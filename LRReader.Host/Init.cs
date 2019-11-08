using LRReader.Host.Impl;
using LRReader.Shared.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace LRReader.Host
{
	public static class Init
	{
		public static void InitObjects()
		{
			SharedGlobal.SettingsStorage = new SettingsStorage();
		}
	}
}
