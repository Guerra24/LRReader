namespace LRReader.UWP.Util
{
	public class IgnoreFocusKeyboardAccelerator : KeyboardAccelerator
	{
		public new event TypedEventHandler<KeyboardAccelerator, KeyboardAcceleratorInvokedEventArgs> Invoked = null!;

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
