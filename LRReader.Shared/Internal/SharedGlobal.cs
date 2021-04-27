using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using System;

namespace LRReader.Shared.Internal
{
	public class SharedGlobal
	{
		public static UpdatesManager UpdatesManager { get; } = new UpdatesManager();
	}

}
