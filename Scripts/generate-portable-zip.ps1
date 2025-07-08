# positional args:
#   $args[0] = binaryFolder
#   $args[1] = releaseFolder
#   $args[2] = assemblyInfoPath
#   $args[3] = fileNameSuffix

[string]$binaryFolder     = $args[0].TrimEnd('\','.')
[string]$releaseFolder    = $args[1].TrimEnd('\','.')
[string]$assemblyInfoPath = $args[2]
[string]$fileNameSuffix   = $args[3]

Write-Host "Binary Folder     : $binaryFolder"
Write-Host "Release Folder    : $releaseFolder"
Write-Host "Assembly Info Path: $assemblyInfoPath"
Write-Host "File Name Suffix  : $fileNameSuffix"

# Validate paths
if (-not (Test-Path -Path $binaryFolder -PathType Container)) {
    throw "Binary folder does not exist: $binaryFolder"
}
if (-not (Test-Path -Path $releaseFolder -PathType Container)) {
    throw "Release folder does not exist: $releaseFolder"
}
if (-not (Test-Path -Path $assemblyInfoPath -PathType Leaf)) {
    throw "AssemblyInfo.cs not found: $assemblyInfoPath"
}

# Extract version and product name from AssemblyInfo.cs
$assemblyLines = Get-Content $assemblyInfoPath

$appName = ($assemblyLines | Where-Object { $_ -match '^\s*\[assembly:\s*AssemblyTitle\("(.+?)"\)\s*\]' }) -replace '^\s*\[assembly:\s*AssemblyTitle\("(.+?)"\)\s*\]', '$1'
$versionRaw = ($assemblyLines | Where-Object { $_ -match '^\s*\[assembly:\s*AssemblyVersion\("(.+?)"\)\s*\]' }) -replace '^\s*\[assembly:\s*AssemblyVersion\("(.+?)"\)\s*\]', '$1'

if ([string]::IsNullOrWhiteSpace($appName) -or [string]::IsNullOrWhiteSpace($versionRaw)) {
    throw "Could not parse AssemblyTitle or AssemblyVersion from: $assemblyInfoPath"
}

# Trim trailing ".0" segments from the version
$versionParts = $versionRaw.Split('.')
while ($versionParts.Count -gt 2 -and $versionParts[-1] -eq '0') {
    $versionParts = $versionParts[0..($versionParts.Count - 2)]
}
$version = $versionParts -join '.'

# Compose final file name
$zipFileName = "${appName}-${version}${fileNameSuffix}"
$zipFilePath = Join-Path $releaseFolder $zipFileName

# Remove existing zip if present
if (Test-Path -Path $zipFilePath) {
    Write-Host "Removing existing ZIP: $zipFilePath"
    Remove-Item -Path $zipFilePath -Force
}

# Create the ZIP file
Write-Host "Creating ZIP: $zipFilePath"
Compress-Archive -Path (Join-Path $binaryFolder '*') -DestinationPath $zipFilePath -Force

# Confirmation
if (Test-Path -Path $zipFilePath) {
    $size = (Get-Item $zipFilePath).Length / 1KB
    Write-Host "✅ Portable ZIP created: $zipFilePath ($([Math]::Round($size,2)) KB)"
} else {
    throw "❌ Failed to create portable ZIP"
}
