using LRReader.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace LRReader.Shared.Models.Main
{
	public class DatabaseCleanResult : GenericApiResult
	{
		public int deleted { get; set; }
		public int unlinked { get; set; }
	}
}
