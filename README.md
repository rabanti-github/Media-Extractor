# Media-Extractor
Media-Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents), as well as in common archive files (e.g. zip, 7z, tar). Media-Extractor was written in C# and uses the 7zip library as archive processor, as well as WPF as GUI framework.

## Features

* Supports most of the new Office formats (e.g. docx, xlsx, pptx)
* Supports a variety of archive formats (e.g. zip, tar, 7z)
* Creates previews of the most commonly used image formats used in Office (e.g. png, jpg, emf) which can be embedded
* Creates previews of embedded text and XML files
* Supports export of all embedded files at once or individually
* Supports export of other embedded data (e.g. xml files in xlsx or docx)


## Requirements / Dependencies

* .NET 4.5 or higher
* SevenZipExtractor (maintained by NuGet)
* WindowsAPICodePack-Core (maintained by NuGet)
* WindowsAPICodePack-Shell (maintained by NuGet)
