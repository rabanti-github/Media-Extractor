# Media-Extractor
Media-Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents), as well as in common archive files (e.g. zip, 7z, tar). Media-Extractor was written in C# and uses the 7zip library as archive processor, as well as WPF as GUI framework.

## Features

* Supports most of the new Office formats (e.g. docx, xlsx, pptx)
* Supports a variety of archive formats (e.g. zip, tar, 7z)
* Creates previews of the most commonly used image formats used in Office (e.g. png, jpg, emf) which can be embedded
* Creates previews of embedded text and XML files
* Supports export of all embedded files at once or individually
* Supports export of other embedded data (e.g. xml files in xlsx or docx)

## Requirements

* Microsoft Windows 7, 8.x, 10
* .NET 4.5 or higher

Media-Extractor does not need an installation. It can be executed by:

* Double-clicking on the exe file
* Dragging a supported media file into the exe file
* Using the option in Windows Explorer "Open with..."

## Development Dependencies

The following libraries / dependencies are necessary for the development of Media-Extractor. All of them are maintained by NuGet:

* SevenZipExtractor
* WindowsAPICodePack-Core
* WindowsAPICodePack-Shell
