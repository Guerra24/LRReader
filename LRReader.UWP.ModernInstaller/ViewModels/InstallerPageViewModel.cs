using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LRReader.UWP.Installer.Services;
using System;
using System.Threading.Tasks;

namespace LRReader.UWP.Installer.ViewModels;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT", Justification = "<Pending>")]
public partial class InstallerPageViewModel : ObservableObject
{
	private readonly InstallerService Installer;

	[ObservableProperty]
	private InstallState _installState;

	[ObservableProperty]
	private double _installProgress;

	[ObservableProperty]
	private string _error = string.Empty;

	[ObservableProperty]
	private bool _showButtons;

	[ObservableProperty]
	private bool _showProgress;

	public InstallerPageViewModel(InstallerService installer)
	{
		Installer = installer;
	}

	public async Task Load()
	{
		InstallState = await Installer.CheckAppState();
		ShowButtons = true;
	}

	[RelayCommand]
	private async Task Install()
	{
		InstallProgress = -1;
		ShowButtons = false;
		ShowProgress = true;
		var result = await Installer.Install(new Progress<uint>(percentage => InstallProgress = percentage));
		if (result.IsRegistered)
		{
			await Installer.Launch();
		}
		else
		{
			Error = result.ErrorText;
		}
		ShowProgress = false;
		InstallState = await Installer.CheckAppState();
		ShowButtons = true;
	}

	[RelayCommand]
	private async Task Uninstall()
	{
		await Installer.Uninstall();
		InstallState = await Installer.CheckAppState();
	}
}
