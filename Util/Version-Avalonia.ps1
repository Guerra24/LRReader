[CmdletBinding()]
Param(
  [Parameter(Mandatory=$true, Position=0)]
  [string]$git
)

$csproj = "LRReader.Avalonia.Desktop\LRReader.Avalonia.Desktop.csproj"
[xml]$xmlDoc = Get-Content $csproj
$version = [System.Version]::Parse($xmlDoc.Project.PropertyGroup.Version);

$tag = "v$version"

$count = [int]$(& "$git" rev-list --count "$tag..HEAD")

$count += 1000

$Field = $version.GetType().GetField('_Revision', 'static,nonpublic,instance')
$Field.SetValue($version, $count -as [int])

$env:LRR_VERSION = $version.ToString();

echo "Version:" $env:LRR_VERSION
