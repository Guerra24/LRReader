[xml]$xmlDoc = Get-Content "./LRReader.UWP/Package.appxmanifest"

Set-Location "./LRReader.UWP/AppPackages/"
# Rename-Item $(Get-ChildItem *LRReader* -Directory) "$(Get-Location)/LRReader.UWP"

$installer = "$(Get-Location)/LRReader.UWP.appinstaller"
$html = "$(Get-Location)/index.html"
$text = "_$($xmlDoc.Package.Identity.Version)_Test"

$content = [System.IO.File]::ReadAllText($installer).Replace($text, "")
[System.IO.File]::WriteAllText($installer, $content)

Set-Location "./LRReader.UWP"
Remove-Item -Path $html
# Remove-Item -Path "./Install.ps1","./Add-AppDevPackage.ps1"
# Remove-Item -Path "./Add-AppDevPackage.resources" –recurse
Set-Location "./Dependencies/"
Remove-Item -Path "./arm","./Win32","./x86" –recurse -ErrorAction SilentlyContinue
