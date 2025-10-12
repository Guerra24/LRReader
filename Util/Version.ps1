$xmlFileName = "LRReader.UWP\Package.appxmanifest"
[xml]$xmlDoc = Get-Content $xmlFileName
$version = [System.Version]::Parse($xmlDoc.Package.Identity.Version);

$tag = "v$version"

$count = [int]$(& "C:\Program Files\Git\mingw64\bin\git.exe" rev-list --count "$tag..HEAD")

$count += 1000

$Field = $version.GetType().GetField('_Revision', 'static,nonpublic,instance')
$Field.SetValue($version, $count -as [int])

$xmlDoc.Package.Identity.Version = $version.ToString();

echo "Version:" $xmlDoc.Package.Identity.Version

$xmlDoc.Save("$(Get-Location)/$xmlFileName")
