param(
    [switch]$Force = $false
)

$scriptArgs = ""
if ($Force)
{
    $scriptArgs = '-Force'
}

$currLocation = Get-Location
Set-Location $PSScriptRoot
Set-Location $(Get-ChildItem *LRReader* -Directory)
Invoke-Expression ".\Install.ps1 $scriptArgs"
CheckNetIsolation "loopbackexempt" "-a" "-n=Guerra24.LRReader_3fr0p4qst6948"
Set-Location $currLocation