﻿using LRReader.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace LRReader.Shared.Internal
{
	public class SharedGlobal
	{
		public static LRRApi LRRApi { get; set; }
		public static SharedEventManager EventManager { get; set; }
		public static ISettingsStorage SettingsStorage { get; set; }
		public static SettingsManager SettingsManager { get; set; }
	}
}
