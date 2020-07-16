using LRReader.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace LRReader.Shared.Models.Main
{
	public class ShinobuStatus : GenericApiResult
	{
		public int is_alive { get; set; }
		public int pid { get; set; }
	}
}
