using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Internal
{
	public delegate void ShowError(string title, string content);
	public delegate void ShowHeader(bool value);

	public class EventManager
	{
		public event ShowError ShowErrorEvent;
		public event ShowHeader ShowHeaderEvent;

		public void ShowError(string title, string content)
		{
			ShowErrorEvent(title, content);
		}

		public void ShowHeader(bool value)
		{
			ShowHeaderEvent(value);
		}
	}
}
