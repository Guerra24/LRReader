namespace LRReader.Shared.Models
{
	public enum PacketType
	{
		None, InstanceStart, InstanceStop, InstanceStatus, InstanceSetting, InstanceRepair
	}

	public enum SettingOperation
	{
		None, Load, Save
	}
	public enum SettingType
	{
		None, ContentFolder, ThumbnailFolder, StartServerAutomatically, StartWithWindows, NetworkPort,
		ForceDebugMode, UseWSL2
	}

}
