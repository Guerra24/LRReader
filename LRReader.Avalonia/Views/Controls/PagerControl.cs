using Avalonia.Automation;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Styling;
using FluentAvalonia.Core;
using System.Collections.ObjectModel;

namespace LRReader.Avalonia.Views.Controls
{
	public sealed partial class PagerControl : TemplatedControl
	{
		private int m_lastSelectedPageIndex = -1;
		private int m_lastNumberOfPagesCount = 0;
		private const int c_infiniteModeComboBoxItemsIncrement = 100;

		private ComboBox ComboBox = null!;

		public PagerControl()
		{
			Pages = new ObservableCollection<object>();
		}

		protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
		{
			base.OnApplyTemplate(e);

			// TODO: Lets not hardcode this
			PrefixText = "Page";
			SuffixText = "of";

			var firstPageButton = e.NameScope.Get<Button>("FirstPageButton");
			AutomationProperties.SetName(firstPageButton, "First page");
			firstPageButton.Click += FirstButtonClicked;

			var previousPageButton = e.NameScope.Get<Button>("PreviousPageButton");
			AutomationProperties.SetName(previousPageButton, "Previous page");
			previousPageButton.Click += PreviousButtonClicked;

			var nextPageButton = e.NameScope.Get<Button>("NextPageButton");
			AutomationProperties.SetName(nextPageButton, "Next page");
			nextPageButton.Click += NextButtonClicked;

			var lastPageButton = e.NameScope.Get<Button>("LastPageButton");
			AutomationProperties.SetName(lastPageButton, "Last page");
			lastPageButton.Click += LastButtonClicked;

			ComboBox = e.NameScope.Get<ComboBox>("ComboBoxDisplay");
			FillComboBoxCollectionToSize(NumberOfPages);
			ComboBox.SelectedIndex = SelectedPageIndex - 1;
			ComboBox.SelectionChanged += ComboBoxSelectionChanged;

			OnNumberOfPagesChanged(0);

			OnSelectedPageIndexChange(-1);
		}

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);

			if (Template != null)
			{
				var property = change.Property;

				if (property == NumberOfPagesProperty)
				{
					OnNumberOfPagesChanged((int)change.OldValue!);
				}
				else if (property == SelectedPageIndexProperty)
				{
					OnSelectedPageIndexChange((int)change.OldValue!);
				}
			}
		}

		private void OnNumberOfPagesChanged(int oldValue)
		{

			m_lastNumberOfPagesCount = oldValue;
			int numberOfPages = NumberOfPages;
			if (numberOfPages < SelectedPageIndex && numberOfPages > -1)
			{
				SelectedPageIndex = numberOfPages - 1;
			}
			UpdateTemplateSettingElementLists();

			//UpdateOnEdgeButtonVisualStates();
		}

		//private void OnRootGridKeyDown(object sender, KeyRoutedEventArgs args);

		private void OnSelectedPageIndexChange(int oldValue)
		{
			// If we don't have any pages, there is nothing we should do.
			// Ensure that SelectedPageIndex will end up in the valid range of values
			// Special case is NumberOfPages being 0, in that case, don't verify upperbound restrictions
			if (SelectedPageIndex > NumberOfPages - 1 && NumberOfPages > 0)
			{
				SelectedPageIndex = NumberOfPages - 1;
			}
			else if (SelectedPageIndex < 0)
			{
				SelectedPageIndex = 0;
			}
			// Now handle the value changes
			m_lastSelectedPageIndex = oldValue;

			if (SelectedPageIndex < Pages.Count)
			{
				ComboBox.SelectedIndex = SelectedPageIndex;
			}

			//UpdateOnEdgeButtonVisualStates();
			UpdateTemplateSettingElementLists();

			RaiseSelectedIndexChanged();
		}

		private void RaiseSelectedIndexChanged()
		{
			var args = new PagerControlSelectedIndexChangedEventArgs(m_lastSelectedPageIndex, SelectedPageIndex);
			SelectedIndexChanged?.Invoke(this, args);
		}

		private void UpdateTemplateSettingElementLists()
		{
			// Cache values for performance :)
			var numberOfPages = NumberOfPages;

			if (numberOfPages > -1)
			{
				FillComboBoxCollectionToSize(numberOfPages);
			}
			else
			{
				if (Pages.Count < c_infiniteModeComboBoxItemsIncrement)
				{
					FillComboBoxCollectionToSize(c_infiniteModeComboBoxItemsIncrement);
				}
			}
		}

		private void FillComboBoxCollectionToSize(int numberOfPages)
		{

			int currentComboBoxItemsCount = Pages.Count;
			if (currentComboBoxItemsCount <= numberOfPages)
			{
				// We are increasing the number of pages, so add the missing numbers.
				for (int i = currentComboBoxItemsCount; i < numberOfPages; i++)
				{
					Pages.Add(i + 1);
				}
			}

			else
			{
				// We are decreasing the number of pages, so remove numbers starting at the end.
				for (int i = currentComboBoxItemsCount; i > numberOfPages; i--)
				{
					Pages.RemoveAt(Pages.Count - 1);
				}
			}
		}

		private void ComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs args)
		{
			SelectedPageIndex = ComboBox.SelectedIndex;
		}

		private void FirstButtonClicked(object? sender, RoutedEventArgs args)
		{
			SelectedPageIndex = 0;
		}

		private void PreviousButtonClicked(object? sender, RoutedEventArgs args)
		{
			SelectedPageIndex--;
		}

		private void NextButtonClicked(object? sender, RoutedEventArgs args)
		{
			SelectedPageIndex++;
		}

		private void LastButtonClicked(object? sender, RoutedEventArgs args)
		{
			SelectedPageIndex = NumberOfPages - 1;
		}

		public ControlTheme FirstButtonStyle
		{
			get => GetValue(FirstButtonStyleProperty);
			set => SetValue(FirstButtonStyleProperty, value);
		}
		public ControlTheme PreviousButtonStyle
		{
			get => GetValue(PreviousButtonStyleProperty);
			set => SetValue(PreviousButtonStyleProperty, value);
		}
		public ControlTheme NextButtonStyle
		{
			get => GetValue(NextButtonStyleProperty);
			set => SetValue(NextButtonStyleProperty, value);
		}
		public ControlTheme LastButtonStyle
		{
			get => GetValue(LastButtonStyleProperty);
			set => SetValue(LastButtonStyleProperty, value);
		}
		public int NumberOfPages
		{
			get => GetValue(NumberOfPagesProperty);
			set => SetValue(NumberOfPagesProperty, value);
		}
		public string PrefixText
		{
			get => GetValue(PrefixTextProperty);
			set => SetValue(PrefixTextProperty, value);
		}
		public string SuffixText
		{
			get => GetValue(SuffixTextProperty);
			set => SetValue(SuffixTextProperty, value);
		}
		public IList<object> Pages
		{
			get => GetValue(PagesProperty);
			set => SetValue(PagesProperty, value);
		}
		public int SelectedPageIndex
		{
			get => GetValue(SelectedPageIndexProperty);
			set => SetValue(SelectedPageIndexProperty, value);
		}

		public event TypedEventHandler<PagerControl, PagerControlSelectedIndexChangedEventArgs>? SelectedIndexChanged;

		public static readonly StyledProperty<ControlTheme> FirstButtonStyleProperty = AvaloniaProperty.Register<PagerControl, ControlTheme>("FirstButtonStyle");
		public static readonly StyledProperty<ControlTheme> PreviousButtonStyleProperty = AvaloniaProperty.Register<PagerControl, ControlTheme>("PreviousButtonStyle");
		public static readonly StyledProperty<ControlTheme> NextButtonStyleProperty = AvaloniaProperty.Register<PagerControl, ControlTheme>("NextButtonStyle");
		public static readonly StyledProperty<ControlTheme> LastButtonStyleProperty = AvaloniaProperty.Register<PagerControl, ControlTheme>("LastButtonStyle");
		public static readonly StyledProperty<int> NumberOfPagesProperty = AvaloniaProperty.Register<PagerControl, int>("NumberOfPages");
		public static readonly StyledProperty<string> PrefixTextProperty = AvaloniaProperty.Register<PagerControl, string>("PrefixText");
		public static readonly StyledProperty<string> SuffixTextProperty = AvaloniaProperty.Register<PagerControl, string>("SuffixText");
		public static readonly StyledProperty<IList<object>> PagesProperty = AvaloniaProperty.Register<PagerControl, IList<object>>("Pages");
		public static readonly StyledProperty<int> SelectedPageIndexProperty = AvaloniaProperty.Register<PagerControl, int>("SelectedPageIndex");
	}

	public sealed class PagerControlSelectedIndexChangedEventArgs
	{
		public int NewPageIndex { get; }
		public int PreviousPageIndex { get; }

		public PagerControlSelectedIndexChangedEventArgs(int previousPageIndex, int newPageIndex)
		{
			NewPageIndex = newPageIndex;
			PreviousPageIndex = previousPageIndex;
		}
	}
}
