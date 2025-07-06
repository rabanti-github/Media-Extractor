param (
    [string]$outputFolder,
    [string]$targetFile
)

# Remove any trailing "\." or "\" from the folder path
$outputFolder = $outputFolder.TrimEnd('\','.')
Write-Host "Scanning folder: $outputFolder"
Write-Host "Writing manifest to: $targetFile"

if (-not (Test-Path $outputFolder)) {
    throw "Output folder does not exist: $outputFolder"
}

# Collect files
$files = Get-ChildItem -Path $outputFolder -Recurse -File | ForEach-Object {
    $relativePath = $_.FullName.Substring($outputFolder.Length).TrimStart('\')
    $relativePath -replace '\\','/'
}

# Prepend comment and write the file
"# This file was generated for the installer and can safely be deleted" | Out-File -Encoding UTF8 -FilePath $targetFile
# Write the manifest
$files | Add-Content -Encoding UTF8 $targetFile
Write-Host "File list written ($($files.Count) entries)."
