using LRReader.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LRReader.Shared.Services
{

	public enum Symbols
	{
		Favorite, Pictures
	}

	public enum Dialog
	{
		Generic, CategoryArchive, CreateCategory, Markdown, ProgressConflict, ServerProfile
	}

	public abstract class PlatformService
	{
		public abstract Version Version { get; }
		public abstract bool AnimationsEnabled { get; }
		public abstract uint HoverTime { get; }

		public abstract void Init();
		public abstract void ChangeTheme(AppTheme theme);
		public abstract string GetLocalizedString(string key);
		public abstract object? GetSymbol(Symbols symbol);
		public abstract Task<bool> OpenInBrowser(Uri uri);

		private Dictionary<Dialog, Type> Dialogs = new Dictionary<Dialog, Type>();

		public void MapDialogToType(Dialog tab, Type type) => Dialogs.Add(tab, type);

		public Task<IDialogResult> OpenDialog(Dialog dialog, params object?[] args) => OpenDialog<IDialog>(dialog, args);

		public async Task<IDialogResult> OpenDialog<D>(Dialog dialog, params object?[] args) where D : IDialog
		{
			var newDialog = CreateDialog<D>(dialog, args);
			if (newDialog == null)
				return IDialogResult.None;
			return await newDialog.ShowAsync();
		}

		public D CreateDialog<D>(Dialog dialog, params object?[] args) where D : IDialog => (D)Activator.CreateInstance(Dialogs[dialog], args);

		public abstract Task<IDialogResult> OpenGenericDialog(string title = "", string primarybutton = "", string secondarybutton = "", string closebutton = "", object? content = null);
	}

	public class StubPlatformService : PlatformService
	{

		public override void Init()
		{
		}

		public override Version Version => new Version(0, 0, 0, 0);

		public override bool AnimationsEnabled => false;

		public override uint HoverTime => 300;

		public override Task<bool> OpenInBrowser(Uri uri)
		{
			return Task.Run(() => false);
		}

		public override object? GetSymbol(Symbols symbol) => null;

		public override string GetLocalizedString(string key) => key;

		public override void ChangeTheme(AppTheme theme) { }

		public override Task<IDialogResult> OpenGenericDialog(string title, string primarybutton, string secondarybutton, string closebutton, object content) { return Task.Run(() => IDialogResult.None); }
	}
}
