$secrets = "$(Get-Location)/LRReader.UWP.Installer/Variables.cs"
$content = [System.IO.File]::ReadAllText($secrets)
if (-Not ($env:APP_INSTALLER_URL -eq $null)) {
    $content = $content.Replace("{APP_INSTALLER_URL}", $env:APP_INSTALLER_URL)
}
if (-Not ($env:APP_VERSION -eq $null)) {
    $content = $content.Replace("{APP_VERSION}", $env:APP_VERSION)
}
[System.IO.File]::WriteAllText($secrets, $content)
