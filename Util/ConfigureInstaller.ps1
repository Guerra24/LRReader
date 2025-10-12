[CmdletBinding()]
Param(
  [Parameter(Mandatory=$true, Position=0)]
  [string]$file
)

$content = [System.IO.File]::ReadAllText($file)
if (-Not ($env:APP_INSTALLER_URL -eq $null)) {
    $content = $content.Replace("{APP_INSTALLER_URL}", $env:APP_INSTALLER_URL)
}
if (-Not ($env:APP_VERSION -eq $null)) {
    $content = $content.Replace("{APP_VERSION}", $env:APP_VERSION)
}
[System.IO.File]::WriteAllText($file, $content)
