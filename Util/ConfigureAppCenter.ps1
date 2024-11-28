$secrets = "$(Get-Location)/LRReader.UWP/Secrets.cs"

$content = [System.IO.File]::ReadAllText($secrets).Replace("{SENTRY_DSN}", $env:SENTRY_DSN)
[System.IO.File]::WriteAllText($secrets, $content)
