Set-Location bin\Debug\net35\
$compress = @{
    Path = "manifest.json", "Deli.Core.dll"
    CompressionLevel = "Optimal"
    DestinationPath = "Deli.Core.zip"
}
Compress-Archive @compress