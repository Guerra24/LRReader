using System;
using System.Collections.Generic;
using System.Text;

namespace LRReader.Shared.Models.Main
{
	public class MinionStatus
	{
		public List<string> args { get; set; }
		public int attempts { get; set; }
		public List<object> children { get; set; }
		public string created { get; set; }
		public string delayed { get; set; }
		public object expires { get; set; }
		public string finished { get; set; }
		public int id { get; set; }
		public int lax { get; set; }
		public object notes { get; set; } // Custom class
		public List<object> parents { get; set; }
		public int priority { get; set; }
		public string queue { get; set; }
		public object result { get; set; } // Custom Class
		public object retried { get; set; }
		public int retries { get; set; }
		public string started { get; set; }
		public string state { get; set; }
		public string task { get; set; }
		public string time { get; set; }
		public int? worker { get; set; }
	}
}
