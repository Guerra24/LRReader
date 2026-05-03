$csproj = "LRReader.Avalonia.Desktop\LRReader.Avalonia.Desktop.csproj"
[xml]$xmlDoc = Get-Content $csproj
$version = [System.Version]::Parse($xmlDoc.Project.PropertyGroup.Version);

$tag = "v$version"

$count = [int]$(& "C:\Program Files\Git\mingw64\bin\git.exe" rev-list --count "$tag..HEAD")

$count += 1000

$Field = $version.GetType().GetField('_Revision', 'static,nonpublic,instance')
$Field.SetValue($version, $count -as [int])

$env:LRR_VERSION = $version.ToString();

echo "Version:" $env:LRR_VERSION
