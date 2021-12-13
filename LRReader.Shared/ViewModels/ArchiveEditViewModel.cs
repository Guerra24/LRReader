using LRReader.Shared.Messages;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels
{
	public partial class ArchiveEditViewModel : ObservableObject
	{

		public AsyncRelayCommand SaveCommand { get; }
		public AsyncRelayCommand UsePluginCommand { get; }
		public AsyncRelayCommand ReloadCommand { get; }

		private RelayCommand<EditableTag> TagCommand { get; }
		public RelayCommand AddAllTags { get; }

		[AllowNull]
		public Archive Archive;

		[ObservableProperty]
		private string _title = "";
		[ObservableProperty]
		private string _tags = "";

		private bool _saving;
		public bool Saving
		{
			get => _saving;
			set
			{
				SetProperty(ref _saving, value);
				TagCommand.NotifyCanExecuteChanged();
				SaveCommand.NotifyCanExecuteChanged();
				UsePluginCommand.NotifyCanExecuteChanged();
				ReloadCommand.NotifyCanExecuteChanged();
				AddAllTags.NotifyCanExecuteChanged();
			}
		}

		public ObservableCollection<Plugin> Plugins = new ObservableCollection<Plugin>();
		public ObservableCollection<EditableTag> TagsList = new ObservableCollection<EditableTag>();

		public ObservableCollection<EditableTag> PluginTagsList = new ObservableCollection<EditableTag>();

		[ObservableProperty]
		private Plugin? _currentPlugin;
		[ObservableProperty]
		private bool _useTextTags;

		public string Arg = "";

		public ArchiveEditViewModel(SettingsService settings)
		{
			UseTextTags = !settings.UseVisualTags;

			SaveCommand = new AsyncRelayCommand(SaveArchive, () => !Saving);
			UsePluginCommand = new AsyncRelayCommand(UsePlugin, () => !Saving && Plugins.Count > 0);
			ReloadCommand = new AsyncRelayCommand(ReloadArchive, () => !Saving);

			TagCommand = new RelayCommand<EditableTag>(HandleTagCommand, (_) => !Saving);
			AddAllTags = new RelayCommand(AddPluginTags, () => !Saving && Plugins.Count > 0 && PluginTagsList.Count > 0);
		}


		public async Task LoadArchive(Archive archive)
		{
			Archive = archive;
			Title = archive.title;
			ReloadTagsList(archive.tags);

			OnPropertyChanged("Title");
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
					ReloadTagsList(result.tags);
					OnPropertyChanged("Title");
				}
				await ReloadPlugins();
			}
			catch (Exception e)
			{
				Crashes.TrackError(e.Demystify());
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
				var result = await ArchivesProvider.UpdateArchive(Archive.arcid, Title, tags);
				if (result)
				{
					Archive.title = Title;
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
				Crashes.TrackError(e.Demystify());
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
						WeakReferenceMessenger.Default.Send(new ShowNotification("Error while fetching tags", result.error, 0));
					}
				}
				Saving = false;
			}
			catch (Exception e)
			{
				Crashes.TrackError(e.Demystify());
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
			var result = "";
			foreach (var t in TagsList)
				if (!(t is AddTag))
					result += t.Tag + ", ";
			return result.Trim().TrimEnd(',');
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
			foreach (var t in tags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).OrderByDescending(t => t.Contains(":")).ThenBy(t => t.Trim()))
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
		[AllowNull]
		public string Tag { get; set; }
		public string? Color { get; set; }
		[AllowNull]
		public RelayCommand<EditableTag> Command { get; internal set; }

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
