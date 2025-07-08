# Scripts

## Description

This folder contains scripts that are used for several purposes, e.g. to be executed in post-build events

## Content

### generate-portable-zip.ps1

This script generates a portable zip file of the application. It is used to create a zip file that can be distributed and run without installation.

#### Parameters (named)

- binaryFolder: Path to the application files (exe, dlls, additional files that are required to run the application). This is usually the `./bin/Release` folder
- releaseFolder: Path where the portable zip file should be placed
- assemblyInfoPath: Path to the Assembly Info class, usually `./Properties/AssemblyInfo.cs`
- fileNameSuffix : Usually something like `-portable.zip`

### clean-folder.ps1

Deletes all files in the specified folder **except** for variations of `readme.md` and `.gitignore` (case-insensitive).

#### Parameters

- Folder: Path to the folder that should be cleaned

### copy-info-files.ps1

Copies files like the readme, license, change log and the application icon into the output folder and gives them a meaningful name.

#### Parameters

- target dir: Path to the target directory where the files should be copied, usually the `./bin/Release` folder
- solution dir: Path to the solution, where the files are originally located