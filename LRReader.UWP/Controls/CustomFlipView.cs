using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace LRReader.Controls
{
	public class CustomFlipView : FlipView
	{
		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			Button prev = GetTemplateChild("PreviousButtonHorizontal") as Button;
			prev.Visibility = Visibility.Collapsed;
			prev.Width = 0;

			Button next = GetTemplateChild("NextButtonHorizontal") as Button;
			next.Visibility = Visibility.Collapsed;
			next.Width = 0;
		}

	}
}
