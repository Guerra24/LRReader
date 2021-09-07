using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace LRReader.UWP.Util
{
	public class IgnoreFocusKeyboardAccelerator : KeyboardAccelerator
	{
		public new event TypedEventHandler<KeyboardAccelerator, KeyboardAcceleratorInvokedEventArgs> Invoked;

		public IgnoreFocusKeyboardAccelerator()
		{
			base.Invoked += IgnoreFocusKeyboardAccelerator_Invoked;
		}

		private void IgnoreFocusKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs e)
		{
			if (FocusManager.GetFocusedElement().GetType() != typeof(TextBox))
				Invoked?.Invoke(sender, e);
		}
	}
}
