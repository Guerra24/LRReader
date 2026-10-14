using Avalonia;
using Avalonia.Browser;
using LRReader.Shared.Services;

namespace LRReader.Avalonia.Browser;

internal sealed partial class Program
{
	private static async Task Main(string[] args)
	{
		try
		{
			Init.EarlyInit(new DummyEntryPoint());
			await BuildAvaloniaApp().StartBrowserAppAsync("out");
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
	}

	public static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>();
}

internal class DummyEntryPoint : IEntryPoint { }