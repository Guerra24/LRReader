using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Internal
{
	public delegate void ShowError(string title, string content);

	public class EventManager
	{
		public event ShowError ShowErrorEvent;

		public void ShowError(string title, string content)
		{
			ShowErrorEvent(title, content);
		}
	}
}
