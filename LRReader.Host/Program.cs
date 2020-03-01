using System;
using System.Collections.Generic;
using System.Text;

namespace LRReader.Host
{
	public class Program
	{
        [System.STAThreadAttribute()]
        public static void Main()
        {
            using (new UWP.Island.App())
            {
                App app = new App();
                app.InitializeComponent();
                app.Run();
            }
        }
    }
}
