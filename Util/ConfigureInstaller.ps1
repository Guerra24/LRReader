if ($env:APP_INSTALLER_URL -eq $null) {
    Return
}
$secrets = "$(Get-Location)/LRReader.UWP.Installer/Variables.cs"
$content = [System.IO.File]::ReadAllText($secrets).Replace("{APP_INSTALLER_URL}", $env:APP_INSTALLER_URL)
[System.IO.File]::WriteAllText($secrets, $content)
