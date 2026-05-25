[CmdletBinding()]
Param(
  [Parameter(Mandatory=$true, Position=0)]
  [string]$git
)

$csproj = "LRReader.Avalonia.Android\LRReader.Avalonia.Android.csproj"
[xml]$xmlDoc = Get-Content $csproj
$version = [System.Version]::Parse($xmlDoc.Project.PropertyGroup.Version);

$tag = "v$version"

$count = [int]$(& "$git" rev-list --count "$tag..HEAD")

$count += 1000

$Field = $version.GetType().GetField('_Revision', 'static,nonpublic,instance')
$Field.SetValue($version, $count -as [int])

$env:LRR_VERSION = $version.ToString();
$env:LRR_BUILD = $(& "$git" rev-list --count "HEAD")

echo "Version:" $env:LRR_VERSION
echo "Build:" $env:LRR_BUILD
