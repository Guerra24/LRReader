using System;
using Avalonia;
using LRReader.Shared.Services;

namespace LRReader.Avalonia.Desktop;

static class Program
{
	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.
	[STAThread]
	public static void Main(string[] args)
	{
		Init.EarlyInit(new DummyEntryPoint());
		BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
	}

	// Avalonia configuration, don't remove; also used by visual designer.
	public static AppBuilder BuildAvaloniaApp() =>
		AppBuilder.Configure<App>()
		.UsePlatformDetect()
		.With(new SkiaOptions
		{
			MaxGpuResourceSizeBytes = 268435456 // 256mib
		})
		/*.With(new Win32PlatformOptions
		{
			RenderingMode = [Win32RenderingMode.Vulkan, Win32RenderingMode.AngleEgl, Win32RenderingMode.Software]
		})
		.With(new X11PlatformOptions
		{
			RenderingMode = [X11RenderingMode.Vulkan, X11RenderingMode.Glx, X11RenderingMode.Software]
		})*/
#if DEBUG
		.WithDeveloperTools()
#endif
		.LogToTrace();
}

internal class DummyEntryPoint : IEntryPoint { }
