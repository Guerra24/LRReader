using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Shared.Internal
{
	public delegate void ShowError(string title, string content);
	public delegate void RebuildReaderImagesSet();

	public class SharedEventManager
	{
		public event ShowError ShowErrorEvent;
		public event RebuildReaderImagesSet RebuildReaderImagesSetEvent;

		public void ShowError(string title, string content)
		{
			ShowErrorEvent?.Invoke(title, content);
		}

		public void RebuildReaderImagesSet()
		{
			RebuildReaderImagesSetEvent?.Invoke();
		}

	}
}
