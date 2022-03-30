using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using LRReader.Shared.Models;

namespace LRReader.Avalonia.Views.Dialogs
{
	public partial class GenericDialog : Window, IDialog
	{
		public GenericDialog()
		{
			InitializeComponent();
#if DEBUG
			this.AttachDevTools();
#endif
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public void SetData(string title = "", string primarybutton = "", string secondarybutton = "", string closebutton = "", object? content = null)
		{
			Title = title;
			this.FindControl<TextBlock>("DialogTitle").Text = title;
			this.FindControl<Button>("PrimaryButton").Content = primarybutton;
			this.FindControl<Button>("CloseButton").Content = closebutton;
			this.FindControl<ContentControl>("ContentControl").Content = content;
		}

		private void PrimaryButton_Click(object sender, RoutedEventArgs e)
		{
			Close(IDialogResult.Primary);
		}
		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Close(IDialogResult.None);
		}

		public async Task<IDialogResult> ShowAsync()
		{
			return await ShowDialog<IDialogResult>((Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow);
		}

	}
}
