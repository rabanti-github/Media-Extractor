param (
    [string]$targetDir,
    [string]$solutionDir
)
Write-Host "Raben"

$ErrorActionPreference = "Stop"

# Normalize and ensure trailing backslash is removed
$targetDir   = $targetDir.TrimEnd('\','.')
$solutionDir = $solutionDir.TrimEnd('\','.')

function Copy-And-Rename {
    param (
        [string]$source,
        [string]$destinationName
    )
    $destinationPath = Join-Path $targetDir $destinationName
    Write-Host "Copying '$source' to '$destinationPath'"
    Copy-Item -Path $source -Destination $destinationPath -Force
}

# Copy and rename license
Copy-And-Rename -source (Join-Path $solutionDir "LICENSE") -destinationName "license.txt"
# Copy and rename changelog
Copy-And-Rename -source (Join-Path $solutionDir "Changelog.md") -destinationName "changelog.txt"
# Copy and rename icon
Copy-And-Rename -source (Join-Path $solutionDir "MediaExtractor\media\media_extractor.ico") -destinationName "MediaExtractor.ico"

Write-Host "Information files copied successfully."
