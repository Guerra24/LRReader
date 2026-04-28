using FluentAvalonia.UI.Controls;
using LRReader.Shared.Models;

namespace LRReader.Avalonia.Views.Dialogs;

public partial class MarkdownDialog : FAContentDialog, IDialog
{
	protected override Type StyleKeyOverride => typeof(FAContentDialog);

	public string Text { get; } = "";

	public MarkdownDialog()
	{
		InitializeComponent();
	}

	public MarkdownDialog(string title, string text)
	{
		Title = title;
		Text = text;
		InitializeComponent();
	}

	public async Task<IDialogResult> ShowAsync(object root) => (IDialogResult)(int)await base.ShowAsync((TopLevel)root);
}