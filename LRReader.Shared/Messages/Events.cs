using LRReader.Shared.Models.Main;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LRReader.Shared.Messages
{

	public class DeleteArchiveMessage : ValueChangedMessage<Archive>
	{
		public DeleteArchiveMessage(Archive id) : base(id) { }
	}

	public class DeleteCategoryMessage : ValueChangedMessage<Category>
	{
		public DeleteCategoryMessage(Category id) : base(id) { }
	}

	public struct Notification
	{
		public string Title { get; }
		public string? Content { get; }
		public int Duration { get; }

		public Notification(string title, string? content, int duration)
		{
			Title = title;
			Content = content;
			Duration = duration;
		}
	}

	public class ShowNotification : ValueChangedMessage<Notification>
	{
		public ShowNotification(string title, string? content, int duration = 5000) : base(new Notification(title, content, duration)) { }
	}
}
