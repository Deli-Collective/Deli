# Unpack the arguments
$dir = $args[0]
$targetName = $args[1]

# Set our directory to the build directory
Set-Location $dir

# Setup the compression arguments
$compress = @{
    Path = "manifest.json", "$($targetName).dll"
    CompressionLevel = "Optimal"
    DestinationPath = "$($targetName).zip"
}

# Compress and overwrite the previous build
Compress-Archive -Force @compress