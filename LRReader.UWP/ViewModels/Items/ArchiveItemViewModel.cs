using LRReader.UWP.ViewModels.Base;

namespace LRReader.UWP.ViewModels.Items
{
	public class ArchiveItemViewModel : ArchiveBaseViewModel
	{
		private bool _missingImage = false;
		public bool MissingImage
		{
			get => _missingImage;
			set
			{
				_missingImage = value;
				OnPropertyChanged("MissingImage");
			}
		}
	}
}
