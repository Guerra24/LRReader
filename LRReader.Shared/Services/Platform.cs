using LRReader.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LRReader.Shared.Services
{

	public enum Symbol
	{
		Favorite, Pictures
	}

	public enum Dialog
	{
		Generic, CategoryArchive, CreateCategory, Markdown, ProgressConflict, ServerProfile, ValidateApi, ThumbnailPicker
	}

	public enum Pages
	{
		FirstRun, HostTab, Loading
	}

	public enum PagesTransition
	{
		None, DrillIn
	}

	public abstract class PlatformService
	{
		protected SemaphoreSlim DialogSemaphore = new SemaphoreSlim(1);

		public abstract Version Version { get; }
		public abstract bool AnimationsEnabled { get; }
		public abstract uint HoverTime { get; }

		public bool Active { get; protected set; }

		public abstract void Init();
		public abstract void ChangeTheme(AppTheme theme);
		public abstract string GetLocalizedString(string key);
		public abstract Task<bool> OpenInBrowser(Uri uri);
		public abstract void CopyToClipboard(string text);

		private readonly Dictionary<Dialog, Type> Dialogs = new Dictionary<Dialog, Type>();

		private readonly Dictionary<Symbol, object> Symbols = new Dictionary<Symbol, object>();

		private readonly Dictionary<Pages, Type> Pages = new Dictionary<Pages, Type>();

		private readonly Dictionary<PagesTransition, Type> Transitions = new Dictionary<PagesTransition, Type>();

		public void MapSymbolToSymbol(Symbol symbol, object backing) => Symbols.Add(symbol, backing);

		public object GetSymbol(Symbol symbol) => Symbols[symbol];

		public void MapDialogToType(Dialog tab, Type type) => Dialogs.Add(tab, type);

		public Task<IDialogResult> OpenDialog(Dialog dialog, params object?[] args) => OpenDialog<IDialog>(dialog, args);

		public async Task<IDialogResult> OpenDialog<D>(Dialog dialog, params object?[] args) where D : IDialog
		{
			await DialogSemaphore.WaitAsync();
			try
			{
				var newDialog = CreateDialog<D>(dialog, args);
				if (newDialog == null)
					return IDialogResult.None;
				return await newDialog.ShowAsync();
			}
			finally
			{
				DialogSemaphore.Release();
			}
		}

		public D CreateDialog<D>(Dialog dialog, params object?[] args) where D : IDialog => (D)Activator.CreateInstance(Dialogs[dialog], args);

		public abstract Task<IDialogResult> OpenGenericDialog(string title = "", string primarybutton = "", string secondarybutton = "", string closebutton = "", object? content = null);

		public void MapPageToType(Pages page, Type type) => Pages.Add(page, type);

		public abstract void GoToPage(Pages page, PagesTransition transition, object? parameter = null);

		public Type GetPage(Pages page) => Pages[page];

		public void MapTransitionToType(PagesTransition transition, Type type) => Transitions.Add(transition, type);

		public T CreateTransition<T>(PagesTransition transition) => (T)Activator.CreateInstance(Transitions[transition]);

		public abstract Task<bool> CheckAppInstalled(string package);
	}

	public class StubPlatformService : PlatformService
	{

		public override void Init() { }

		public override Version Version => new Version(0, 0, 0, 0);

		public override bool AnimationsEnabled => false;

		public override uint HoverTime => 300;

		public override Task<bool> OpenInBrowser(Uri uri)
		{
			return Task.Run(() => false);
		}

		public override string GetLocalizedString(string key) => key;

		public override void ChangeTheme(AppTheme theme) { }

		public override Task<IDialogResult> OpenGenericDialog(string title, string primarybutton, string secondarybutton, string closebutton, object? content) { return Task.Run(() => IDialogResult.None); }

		public override void CopyToClipboard(string text) { }

		public override void GoToPage(Pages page, PagesTransition transition, object? parameter = null) { }

		public override Task<bool> CheckAppInstalled(string package) => Task.FromResult(false);
	}
}
