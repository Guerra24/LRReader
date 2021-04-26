namespace LRReader.Shared
{
	public interface ICustomTab
	{
		object CustomTabControl { get; set; }

		string CustomTabId { get; set; }

		void Unload();
	}
}
