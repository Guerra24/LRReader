namespace LRReader.Shared.Models
{
	public interface ICustomTab
	{
		object CustomTabControl { get; set; }

		string CustomTabId { get; set; }

		bool IsClosable { get; set; }

		void Unload();
	}
}
