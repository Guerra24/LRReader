using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
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
			DataContext = this;
		}

		public void SetData(string title = "", string primarybutton = "", string secondarybutton = "", string closebutton = "", object? content = null)
		{
			Title = title;
			DialogTitleTextBlock.Text = title;
			PrimaryButton.Content = primarybutton;
			CloseButton.Content = closebutton;
			ContentPresenter.Content = content;
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

		public string DialogTitle
		{
			get => DialogTitleTextBlock.Text!;
			set => DialogTitleTextBlock.Text = value;
		}

		public string PrimaryButtonText
		{
			get => PrimaryButton.Content!.ToString()!;
			set => PrimaryButton.Content = value;
		}

		public string CloseButtonText
		{
			get => CloseButton.Content!.ToString()!;
			set => CloseButton.Content = value;
		}

		public object DialogContent
		{
			get => ContentPresenter.Content!;
			set => ContentPresenter.Content = value;
		}

		public bool IsPrimaryButtonEnabled
		{
			get => PrimaryButton.IsEnabled;
			set => PrimaryButton.IsEnabled = value;
		}

		//public static readonly DirectProperty<GenericDialog, bool> IsPrimaryButtonEnabledProperty = AvaloniaProperty.RegisterDirect<GenericDialog, bool>("IsPrimaryButtonEnabled", o => o.IsPrimaryButtonEnabled, (o, v) => o.IsPrimaryButtonEnabled = v);
		//public static readonly DirectProperty<GenericDialog, object> DialogContentProperty = AvaloniaProperty.RegisterDirect<GenericDialog, object>("DialogContent", o => o.ContentPresenter.Content!, (o, v) => o.ContentPresenter.Content = v);

	}
}
