using CommunityToolkit.Mvvm.Messaging.Messages;
using LRReader.Shared.Models.Main;

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

		public NotificationSeverity Severity { get; }

		public Notification(string title, string? content, int duration, NotificationSeverity severity = NotificationSeverity.Informational)
		{
			Title = title;
			Content = content;
			Duration = duration;
			Severity = severity;
		}
	}

	public enum NotificationSeverity
	{
		Informational, Success, Warning, Error
	}

	public class ShowNotification : ValueChangedMessage<Notification>
	{
		public ShowNotification(string title, string? content, int duration = 5000, NotificationSeverity severity = NotificationSeverity.Informational) : base(new Notification(title, content, duration, severity)) { }
	}
}
