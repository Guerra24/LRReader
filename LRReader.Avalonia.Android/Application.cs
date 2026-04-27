using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;

namespace LRReader.Avalonia.Android
{
    [Application]
    public class Application : AvaloniaAndroidApplication<App>
    {
        protected Application(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
			Init.EarlyInit();
		}

        protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        {
			return base.CustomizeAppBuilder(builder);
        }
    }
}
