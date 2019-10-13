using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Internal
{
	public class DownloadPayload
	{
		public byte[] Data { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }
	}
}
