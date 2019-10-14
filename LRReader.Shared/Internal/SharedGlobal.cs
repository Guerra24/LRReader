using LRReader.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace LRReader.Shared.Internal
{
	public class SharedGlobal
	{
		public static LRRApi LRRApi { get; set; }
		public static SharedEventManager EventManager { get; set; }
	}
}
