$xmlFileName = "./LRReader.UWP/AppPackages/LRReader.UWP.appinstaller"      
[xml]$xmlDoc = Get-Content $xmlFileName

$node = $xmlDoc.AppInstaller.UpdateSettings
$xmlDoc.AppInstaller.RemoveChild($node) | Out-Null

$xwSettings = new-object System.Xml.XmlWriterSettings
$xwSettings.Indent = $true
$xwSettings.IndentChars = "`t"
$xwSettings.NewLineOnAttributes = $true

$xmlWriter = [Xml.XmlWriter]::Create("$(Get-Location)/$xmlFileName", $xwSettings)

$xmlDoc.Save($xmlWriter)

$xmlWriter.Flush()
$xmlWriter.Close()