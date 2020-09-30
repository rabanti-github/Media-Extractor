# Media-Extractor

![license](https://img.shields.io/github/license/rabanti-github/media-extractor.svg)
![GitHub Releases](https://img.shields.io/github/downloads/rabanti-github/media-extractor/latest/total.svg)
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Frabanti-github%2FMedia-Extractor.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2Frabanti-github%2FMedia-Extractor?ref=badge_shield)

Media-Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents), as well as in common archive files (e.g. zip, 7z, tar). Media-Extractor was written in C# and uses the 7zip library as archive processor, as well as WPF as GUI framework.

## Features

* Supports most of the new Office formats (e.g. docx, xlsx, pptx)
* Supports a variety of archive formats (e.g. zip, tar, 7z)
* Creates previews of the most commonly used image formats used in Office (e.g. png, jpg, emf) which can be embedded
* Creates previews of embedded text and XML files
* Supports export of all embedded files at once, selected ones or particular ones
* Supports export of other embedded data (e.g. xml files in xlsx or docx)
* Loading of files by menu, button, drag&drop or 'open with' (Windows Explorer)
* Dark mode
* Translations (see section 'Translations')

## Translations

Currently, Media-Extractor is translated to the following Languages:

| Language | Status    | Provided by        |
| -------- | --------- | ------------------ |
| **English**  | Completed | Application Author |
| **German**   | Completed | Application Author |

If you are interested in translating the application, please [open a new Issue](https://github.com/rabanti-github/Media-Extractor/issues/new) with the tag '**translation**'. The defined terms that are to be translated can be found in [this wiki Article](https://github.com/rabanti-github/Media-Extractor/wiki/Translation-Template).
For a translation, just a copy of the whole table (with name and translation column necessary) or the file [Resources.resx](https://github.com/rabanti-github/Media-Extractor/blob/master/MediaExtractor/Properties/Resources.resx). Please don't hesitate to ask, if the context of a term is not clear.

## System Requirements

* Microsoft Windows 7, 8.x, 10
* .NET 4.5 or higher

Media-Extractor does not need an installation. It can be executed by:

* Double-clicking on the exe file
* Dragging a supported media file into the exe file
* Using the option in Windows Explorer "Open with..."
* Dropping a file into the designated area (drag&drop)

## Development Dependencies

The following libraries / dependencies are necessary for the development of Media-Extractor. All of them are maintained by NuGet:

* SevenZipExtractor
* WindowsAPICodePack-Core and WindowsAPICodePack-Shell
* AdonisUI and AdonisUI.ClassicTheme

## License

[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Frabanti-github%2FMedia-Extractor.svg?type=large)](https://app.fossa.io/projects/git%2Bgithub.com%2Frabanti-github%2FMedia-Extractor?ref=badge_large)
