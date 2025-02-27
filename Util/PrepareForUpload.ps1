Set-Location "./LRReader.UWP/AppPackages/"
Remove-Item -Path "./index.html"
Set-Location "./LRReader.UWP"
Get-ChildItem "*.cer" | foreach { Remove-Item -Path $_.FullName }
Get-ChildItem "*.msix" | foreach { Remove-Item -Path $_.FullName }
Set-Location "./Dependencies/"
Remove-Item -Path "./arm","./Win32","./x86" –recurse -ErrorAction SilentlyContinue
