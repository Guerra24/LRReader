$xmlFileName = "LRReader.UWP\Package.appxmanifest"      
[xml]$xmlDoc = Get-Content $xmlFileName

$count = & "C:\Program Files\Git\mingw64\bin\git.exe" rev-list --count HEAD
$version = $xmlDoc.Package.Identity.Version;
$substr = "."
$xmlDoc.Package.Identity.Version = $version.Remove(($lastIndex = $version.LastIndexOf($substr) + 1)).Insert($lastIndex,$count)

echo "Version:" $xmlDoc.Package.Identity.Version

$xmlDoc.Save("$(Get-Location)/$xmlFileName")