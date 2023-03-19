using System.Threading.Tasks;

namespace LRReader.Shared.Models
{
	public enum IDialogResult
	{
		None = 0,
		Primary = 1,
		Secondary = 2
	}

	public interface IDialog
	{
		Task<IDialogResult> ShowAsync();
	}

	public interface ICreateCategoryDialog : IDialog
	{
		string Name { get; set; }
		string Query { get; set; }
		bool Pin { get; set; }
	}

	public interface ICreateProfileDialog : IDialog
	{
		string Name { get; set; }
		string Address { get; set; }
		string ApiKey { get; set; }
		bool Integration { get; set; }
	}

	public enum ConflictMode
	{
		Local, Remote
	}

	public interface IProgressConflictDialog : IDialog
	{
		ConflictMode Mode { get; set; }
	}

	public interface IThumbnailPickerDialog : IDialog
	{
		int Page { get; set; }

		Task LoadThumbnails();
	}

}
