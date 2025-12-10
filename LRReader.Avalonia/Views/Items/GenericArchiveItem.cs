using Avalonia;
using Avalonia.Controls.Primitives;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LRReader.Avalonia.Views.Items;

public class GenericArchiveItem : TemplatedControl
{
	public ArchiveItemViewModel ViewModel { get; }

	public GenericArchiveItem()
	{
		ViewModel = Service.Services.GetRequiredService<ArchiveItemViewModel>();
		DataContextChanged += Control_DataContextChanged;
	}

	protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
	{
		base.OnApplyTemplate(e);
	}

	private async void Control_DataContextChanged(object? sender, EventArgs e)
	{
		if (DataContext == null)
			return;
		if (DataContext is Archive archive)
			await ViewModel.Load(archive);
	}

	public int DecodePixelWidth
	{
		get => GetValue(DecodePixelWidthProperty);
		set => SetValue(DecodePixelWidthProperty, value);
	}
	public int DecodePixelHeight
	{
		get => GetValue(DecodePixelHeightProperty);
		set => SetValue(DecodePixelHeightProperty, value);
	}

	public static readonly StyledProperty<int> DecodePixelWidthProperty = AvaloniaProperty.Register<GenericArchiveItem, int>("DecodePixelWidth");
	public static readonly StyledProperty<int> DecodePixelHeightProperty = AvaloniaProperty.Register<GenericArchiveItem, int>("DecodePixelHeight");
}