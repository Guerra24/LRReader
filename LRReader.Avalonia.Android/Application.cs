using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using LRReader.Shared.Services;

namespace LRReader.Avalonia.Android
{
	[Application(UsesCleartextTraffic = true)]
	public class Application : AvaloniaAndroidApplication<App>, IEntryPoint
	{
		protected Application(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			Init.EarlyInit(this);
		}

		protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
		{
			return base.CustomizeAppBuilder(builder)
			.With(new SkiaOptions
			{
				MaxGpuResourceSizeBytes = 268435456 // 256mib
			})
			/*.With(new AndroidPlatformOptions
			{
				RenderingMode = [AndroidRenderingMode.Vulkan, AndroidRenderingMode.Egl, AndroidRenderingMode.Software]
			})*/;
		}
	}
}
