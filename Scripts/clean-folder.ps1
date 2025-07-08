param (
    [Parameter(Mandatory)]
    [string]$folder
)

# Normalize the folder path
$folder = $folder.TrimEnd('\','.')

Write-Host "Cleaning folder: $folder"

if (-not (Test-Path $folder -PathType Container)) {
    throw "Folder does not exist: $folder"
}

# Get all files except README.md (any case) and .gitignore
$filesToDelete = Get-ChildItem -Path $folder -File | Where-Object {
    $_.Name -notmatch '^readme(\.md)?$' -and $_.Name -ne '.gitignore'
}

foreach ($file in $filesToDelete) {
    Write-Host "Deleting: $($file.FullName)"
    Remove-Item -Path $file.FullName -Force
}

Write-Host "✅ Cleanup complete."
