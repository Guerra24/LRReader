using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using LRReader.Avalonia.Android.Services;
using LRReader.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LRReader.Avalonia.Android
{
	[Application(UsesCleartextTraffic = true)]
	public class Application : AvaloniaAndroidApplication<App>, IEntryPoint
	{
		protected Application(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			Init.EarlyInit(this, (collection) =>
			{
#if NIGHTLY_ANDROID
				collection.Replace(ServiceDescriptor.Singleton<UpdatesService, NightlyAndroidUpdatesService>());
#endif
			});
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
