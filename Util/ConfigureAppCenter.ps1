$secrets = "$(Get-Location)/LRReader.UWP/Secrets.cs"

$content = [System.IO.File]::ReadAllText($secrets).Replace("{APPCENTER_APP_ID}", $env:APPCENTER_APP_ID)
[System.IO.File]::WriteAllText($secrets, $content)
