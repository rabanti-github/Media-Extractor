# Scripts Directory

This folder contains scripts that are used for utility tasks, but are not included in Media-Extractor itself.

## Content

### generate-file-list.ps1

This script collects all files that are in the program directory and its subdirectories, and generates a text file with all relative paths of the collected files.
The output file is later used by the installer project to determine the relevant files in case of a portable installation.

The script is executed by default in the post-build event of the Media-Extractor project, so it is not necessary to run it manually.