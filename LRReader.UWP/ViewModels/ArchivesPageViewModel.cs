using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using LRReader.Shared.Models.Api;
using LRReader.Shared.Models.Main;
using LRReader.UWP.Views.Tabs;
using RestSharp;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using static LRReader.Internal.Global;
using Microsoft.Toolkit.Collections;
using System.Threading;
using Microsoft.Toolkit.Uwp;
using LRReader.Shared.Internal;

namespace LRReader.UWP.ViewModels
{
	public class ArchivesPageViewModel : SearchResultsViewModel
	{
		public async Task Refresh()
		{
			if (_internalLoadingArchives)
				return;
			_internalLoadingArchives = true;
			RefreshOnErrorButton = false;
			ArchiveList.Clear();
			LoadingArchives = true;
			foreach (var b in SharedGlobal.SettingsManager.Profile.Bookmarks)
			{
				var archive = SharedGlobal.ArchivesManager.Archives.FirstOrDefault(a => a.arcid == b.archiveID);
				if (archive != null)
					EventManager.CloseTabWithHeader(archive.title);
			}
			await SharedGlobal.ArchivesManager.ReloadArchives();
			LoadBookmarks();
			Page = 0;
			LoadingArchives = false;
			_internalLoadingArchives = false;
		}

		public void LoadBookmarks()
		{
			if (SharedGlobal.SettingsManager.OpenBookmarksStart)
				if (SharedGlobal.ArchivesManager.Archives.Count > 0)
					foreach (var b in SharedGlobal.SettingsManager.Profile.Bookmarks)
					{
						var archive = SharedGlobal.ArchivesManager.Archives.FirstOrDefault(a => a.arcid == b.archiveID);
						if (archive != null)
							EventManager.AddTab(new ArchiveTab(archive), false);
						else
							EventManager.ShowError("Bookmarked Archive with ID[" + b.archiveID + "] not found.", "");
					}
		}

	}
}
