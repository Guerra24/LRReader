using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LRReader.Shared.Messages;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using Sentry;

namespace LRReader.Shared.ViewModels
{
	public partial class ArchiveEditViewModel : ObservableObject
	{
		private readonly ImageProcessingService ImageProcessing;
		private readonly ImagesService Images;
		private readonly PlatformService Platform;

		public AsyncRelayCommand SaveCommand { get; }
		public AsyncRelayCommand UsePluginCommand { get; }
		public AsyncRelayCommand ReloadCommand { get; }
		public AsyncRelayCommand ChangeThumbnailCommand { get; }

		private RelayCommand<EditableTag> TagCommand { get; }
		public RelayCommand AddAllTags { get; }

		public Archive Archive = null!;

		[ObservableProperty]
		private string _title = "";
		[ObservableProperty]
		private string _summary = "";
		[ObservableProperty]
		private string _tags = "";
		[ObservableProperty]
		private object? _thumbnail;

		[ObservableProperty]
		[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
		[NotifyCanExecuteChangedFor(nameof(TagCommand))]
		[NotifyCanExecuteChangedFor(nameof(UsePluginCommand))]
		[NotifyCanExecuteChangedFor(nameof(ReloadCommand))]
		[NotifyCanExecuteChangedFor(nameof(AddAllTags))]
		private bool _saving;

		public ObservableCollection<Plugin> Plugins = new ObservableCollection<Plugin>();
		public ObservableCollection<EditableTag> TagsList = new ObservableCollection<EditableTag>();

		public ObservableCollection<EditableTag> PluginTagsList = new ObservableCollection<EditableTag>();

		[ObservableProperty]
		private Plugin? _currentPlugin;
		[ObservableProperty]
		private bool _useTextTags;

		public event AsyncAction<bool>? Show;
		public event AsyncAction<bool>? Hide;

		public string Arg = "";

		public ArchiveEditViewModel(SettingsService settings, ImageProcessingService imageProcessing, ImagesService images, PlatformService platformService)
		{
			ImageProcessing = imageProcessing;
			Images = images;
			Platform = platformService;

			UseTextTags = !settings.UseVisualTags;

			SaveCommand = new AsyncRelayCommand(SaveArchive, () => !Saving);
			UsePluginCommand = new AsyncRelayCommand(UsePlugin, () => !Saving && Plugins.Count > 0);
			ReloadCommand = new AsyncRelayCommand(ReloadArchive, () => !Saving);
			ChangeThumbnailCommand = new AsyncRelayCommand(ChangeThumbnail, () => !Saving);

			TagCommand = new RelayCommand<EditableTag>(HandleTagCommand, (_) => !Saving);
			AddAllTags = new RelayCommand(AddPluginTags, () => !Saving && Plugins.Count > 0 && PluginTagsList.Count > 0);
		}

		public async Task LoadArchive(Archive archive)
		{
			Archive = archive;
			Title = archive.title;
			Summary = archive.summary;
			ReloadTagsList(archive.tags);

			await Hide.InvokeAsync(false);
			Thumbnail = await ImageProcessing.ByteToBitmap(await Images.GetThumbnailCached(Archive.arcid), decodeHeight: 275);
			await Show.InvokeAsync(Platform.AnimationsEnabled);

			OnPropertyChanged("Archive");
			await ReloadPlugins();
		}

		private async Task ReloadArchive()
		{
			try
			{
				var result = await ArchivesProvider.GetArchive(Archive.arcid);
				if (result != null)
				{
					Title = result.title;
					Summary = result.summary;
					ReloadTagsList(result.tags);
					Thumbnail = await ImageProcessing.ByteToBitmap(await Images.GetThumbnailCached(Archive.arcid), decodeHeight: 275);
				}
				await ReloadPlugins();
			}
			catch (Exception e)
			{
				SentrySdk.CaptureException(e);
			}
		}

		private async Task ChangeThumbnail()
		{
			var dialog = Platform.CreateDialog<IThumbnailPickerDialog>(Dialog.ThumbnailPicker, Archive.arcid);
			await dialog.LoadThumbnails();
			if (await dialog.ShowAsync() == IDialogResult.Primary && await ArchivesProvider.ChangeThumbnail(Archive.arcid, dialog.Page))
			{
				await Hide.InvokeAsync(Platform.AnimationsEnabled);
				Thumbnail = await ImageProcessing.ByteToBitmap(await Images.GetThumbnailCached(Archive.arcid, forced: true), decodeHeight: 275);
				await Show.InvokeAsync(Platform.AnimationsEnabled);
			}
		}

		private async Task SaveArchive()
		{
			try
			{
				Saving = true;
				string tags;
				if (UseTextTags)
					tags = Tags;
				else
					tags = BuildTags();
				var result = await ArchivesProvider.UpdateArchive(Archive.arcid, Title, tags, Summary);
				if (result)
				{
					Archive.title = Title;
					Archive.summary = Summary;
					Archive.tags = tags;
					Archive.UpdateTags();
					if (UseTextTags)
						ReloadTagsList(tags);
					else
						Tags = BuildTags();
					PluginTagsList.Clear();
					OnPropertyChanged("PluginTagsList");
					AddAllTags.NotifyCanExecuteChanged();
					OnPropertyChanged("Archive");
				}
				Saving = false;
			}
			catch (Exception e)
			{
				SentrySdk.CaptureException(e);
			}
		}

		private async Task UsePlugin()
		{
			if (CurrentPlugin is null)
				return;
			try
			{
				await SaveArchive();
				Saving = true;
				PluginTagsList.Clear();
				OnPropertyChanged("PluginTagsList");
				var result = await ServerProvider.UsePlugin(CurrentPlugin.@namespace, Archive.arcid, Arg);
				if (result != null)
				{
					if (result.success)
					{
						if (!string.IsNullOrEmpty(result.data.new_tags))
						{
							if (UseTextTags)
							{
								if (!Tags.TrimEnd().EndsWith(","))
									Tags = Tags.TrimEnd() + ",";
								Tags += result.data.new_tags;
							}
							else
							{
								foreach (var t in result.data.new_tags.Split(','))
									PluginTagsList.Add(ColorTag(new PluginTag { Tag = t.Trim(), Command = TagCommand }));
								AddAllTags.NotifyCanExecuteChanged();
								OnPropertyChanged("PluginTagsList");
							}
						}
					}
					else
					{
						WeakReferenceMessenger.Default.Send(new ShowNotification("Error while fetching tags", result.error, 0, NotificationSeverity.Error));
					}
				}
				Saving = false;
			}
			catch (Exception e)
			{
				SentrySdk.CaptureException(e);
			}
		}

		private async Task ReloadPlugins()
		{
			var plugins = await ServerProvider.GetPlugins(PluginType.Metadata);
			if (plugins == null || plugins.Count == 0)
			{
				CurrentPlugin = null;
				return;
			}
			PluginTagsList.Clear();
			Plugins.Clear();
			plugins.ForEach(p => Plugins.Add(p));
			CurrentPlugin = Plugins.ElementAt(0);
			UsePluginCommand.NotifyCanExecuteChanged();
			AddAllTags.NotifyCanExecuteChanged();
			OnPropertyChanged("PluginTagsList");
		}

		private void AddPluginTags()
		{
			foreach (var tag in PluginTagsList)
				if (!TagsList.Contains(tag))
					TagsList.Insert(TagsList.Count - 1, ColorTag(new EditableTag { Tag = tag.Tag, Command = TagCommand }));
		}

		private string BuildTags()
		{
			var builder = new StringBuilder();
			foreach (var t in TagsList)
				if (t is not AddTag)
				{
					builder.Append(t.Tag);
					builder.Append(", ");
				}
			return builder.ToString().Trim([',', ' ']);
		}

		private void HandleTagCommand(EditableTag? tag)
		{
			if (tag is AddTag)
			{
				TagsList.Insert(TagsList.Count - 1, new EditableTag { Tag = "", Command = TagCommand });
			}
			else if (tag is PluginTag)
			{
				if (!TagsList.Contains(tag))
					TagsList.Insert(TagsList.Count - 1, ColorTag(new EditableTag { Tag = tag.Tag, Command = TagCommand }));
			}
			else if (tag != null)
			{
				TagsList.Remove(tag);
			}
		}

		private void ReloadTagsList(string tags)
		{
			TagsList.Clear();
			foreach (var t in tags.Split([','], StringSplitOptions.RemoveEmptyEntries).OrderByDescending(t => t.Contains(":")).ThenBy(t => t.Trim()))
				TagsList.Add(ColorTag(new EditableTag { Tag = t.Trim(), Command = TagCommand }));
			TagsList.Add(new AddTag { Command = TagCommand });
			Tags = BuildTags();
		}


		private T ColorTag<T>(T tag) where T : EditableTag
		{
			if (tag is AddTag)
				return tag;
			string? color = null;
			var text = tag.Tag.ToLower();
			if (text.Contains("artist:"))
				color = "#22a7f0";
			else if (text.Contains("series:") || text.Contains("parody:"))
				color = "#d2527f";
			else if (text.Contains("circle:") || text.Contains("group:"))
				color = "#36d7b7";
			tag.Color = color;
			return tag;
		}
	}

	public class EditableTag
	{
		public string Tag { get; set; } = null!;
		public string? Color { get; set; }
		public RelayCommand<EditableTag> Command { get; internal set; } = null!;

		public override bool Equals(object obj)
		{
			if (obj is AddTag || this is AddTag)
				return false;
			return obj is EditableTag tag &&
				   Tag.Equals(tag.Tag);
		}

		public override int GetHashCode()
		{
			return Tag.GetHashCode();
		}
	}

	public class AddTag : EditableTag
	{

	}
	public class PluginTag : EditableTag
	{

	}
}
