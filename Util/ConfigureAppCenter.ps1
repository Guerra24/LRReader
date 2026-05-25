[CmdletBinding()]
Param(
  [Parameter(Mandatory=$true, Position=0)]
  [string]$secrets
)

$content = [System.IO.File]::ReadAllText($secrets).Replace("{SENTRY_DSN}", $env:SENTRY_DSN)
[System.IO.File]::WriteAllText($secrets, $content)
