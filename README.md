# ![Favicon](./resources/img/icon32.png) Media-Extractor

[![license: MIT](https://img.shields.io/github/license/rabanti-github/media-extractor.svg)](https://opensource.org/licenses/MIT)
[![GitHub Releases](https://img.shields.io/github/downloads/rabanti-github/media-extractor/latest/total.svg)](https://github.com/rabanti-github/Media-Extractor/releases/latest)
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Frabanti-github%2FMedia-Extractor.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2Frabanti-github%2FMedia-Extractor?ref=badge_shield)

Media-Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents), as well as in common archive files (e.g. zip, 7z, tar). Media-Extractor was written in C# and uses the 7zip library as archive processor, as well as WPF as GUI framework.

## Download

The most recent version of Media-Extractor can be downloaded on the [Page of the latest Release](https://github.com/rabanti-github/Media-Extractor/releases/latest).

- Usually, the **Installer** is suitable for the most users (e.g. **Media-Extractor-Setup_1_8.exe**).
- If you want to use Media-Extractor as a **portable app**, the release version is the right choice (e.g. '**Media-Extractor_Release_v1.8.zip**').
- The portable debug version (Debug.zip), as well as the source code (zip / tar.gz), is only necessary for development purpose.

Please see also the section [SmartScreen Warning in Windows 8.1, 10 and 11](#smartscreen-warning-in-windows-81-10-and-11)

## Feature Overview

- Supports most of the new Office formats (e.g. docx, xlsx, pptx)
- Supports a variety of archive formats (e.g. zip, tar, 7z)
- Creates previews of the most commonly used image formats used in Office (e.g. png, jpg, emf) which can be embedded
- Creates previews of embedded text and XML files
- Option to preview unknown, embedded files as plain text
- Supports export of all embedded files at once, selected ones or particular ones
- Supports export of other embedded data (e.g. xml files in xlsx or docx)
- Loading of files by menu, button, drag&drop or 'open with' (Windows Explorer)
- Dark mode
- Translations (see section [Translations](#Translations))

---

### Loading Documents

Documents (e.g. Excel worksheets, Word documents or zip archives) can be opened in several ways:

Use either the file menu or the 'Open Document' button to quickly load a document or archive.

![Open by Button](./resources/img/button_open.gif)

Drag a document or archive from the Windows Explorer into the drop area of the application (dashed box 'Drop Document here').

![Open by Drag into Area](./resources/img/drag_open.gif)

If Media-Extractor is not open yet, a document or archive can simply be dragged into the executable (MediaExtractor.exe).

![Open by Drag into App](./resources/img/drag_app_open.gif)

Last but not least, a document or archive can be opened over the Windows Explorer, using the context menu entry "Open with...". If performed once, Media-Extractor will be available as possible application to open a document or archive. This assignment must be done for each individual file extension (e.g. .docx, .xlsx, .zip).

![Open by Context Menu](./resources/img/context_open.gif)

### Appearance

Media-Extractor supports on the fly changing of the language (see section [Translations](#Translations)). However, the application will quickly reload. Previously loaded documents or archives must be opened again if the language is changed.

![Change the Language](./resources/img/change_locale.gif)

Media-Extractor supports Dark Mode. The mode can be changed on-the-fly, even if a document or archive is loaded. The mode, as well as other settings are remembered by Media-Extractor and restored if the application is restarted.

![Switch to Dark Mode](./resources/img/dark_mode.gif)

### Preview of embedded Files

By default, only previews of images are provided when loading a document or archive in Media-Extractor. However, all other files (e.g. XML, texts or config files) can be enabled for previews.

![Show all Files](./resources/img/show_all_files.gif)

There is also an option to display unknown file formats as texts. Nevertheless, the attempt to display binary files (e.g. exe, nested archives or movie clips) as text may still fail if there are non-printable characters.

![Preview unknown Formats](./resources/img/preview_unknown_formats.gif)

Furthermore, embedded files can be sorted based on their properties, like file name, size or extension.

Currently, four columns are available:

- Name of the embedded file
- Relative path of the embedded file in the document or archive
- File extension of the embedded file
- Size of the embedded file (extracted size)

![Preview unknown Formats](./resources/img/sort_files.gif)

### Saving of embedded Files

Saving of embedded files is independent of the previews. They can be saved, even if not possible to be displayed as preview. Embedded files can be saved in various ways:

If no file is selected in the list, all files are saved by default. The default of saving all or saving only selected files can be defined with the save button on the right side of the application.

![Save all Files](./resources/img/save_all_files.gif)

One or many files (using ctrl or shift key) can be selected in the list and extracted.

![Save selected Files](./resources/img/save_selected_files.gif)

---

## System Requirements

Although Media-Extractor provides an [installer](#Download), it does not need an installation and can be run as portable app. The [downloaded zip file](#Download) can be unzipped in a folder of your choice. The general system requirements are:

- Microsoft Windows 7, 8.1, 10, 11
- .NET 4.8 or higher installed

Please see the section about the [SmartScreen Warning](#smartscreen-warning-in-windows-81-10-and-11) if you have problem to run the application after downloading.

## Translations

Currently, Media-Extractor is translated to the following Languages:

| Language | Status    | Provided by        |
| -------- | --------- | ------------------ |
| **English**  | Completed | Application Author |
| **German**   | Completed | Application Author |
| **French**   | Completed | AI based translation |

If you are interested in translating the application, please [open a new Issue](https://github.com/rabanti-github/Media-Extractor/issues/new) with the tag '**translation**'. The defined terms that are to be translated can be found in [this wiki Article](https://github.com/rabanti-github/Media-Extractor/wiki/Translation-Template). Alternatively, the default translation terms can be downloaded as [Excel file](./resources/translation/DefaultTranslationStrings.xlsx). This is a direct copy of the default [Resources.resx file](https://github.com/rabanti-github/Media-Extractor/blob/master/MediaExtractor/Properties/Resources.resx).

Please do not hesitate to ask, if the context of a term is not clear.

## SmartScreen Warning in Windows 8.1, 10 and 11

Windows 8.1 introduced a mechanism to protect users from phishing websites and malware, yet not known to Antivirus programs. This mechanism is called [SmartScreen](https://support.microsoft.com/en-us/help/17443/microsoft-edge-smartscreen-faq) and either blocks the access to a malicious website, or the execution of unknown apps.
Unfortunately, it is possible that Media-Extractor is initially blocked by SmartScreen, since the mechanism do not know the app until 'some people' have downloaded and executed the app. The number of necessary downloads until the app is not seen as unknown (thus, not blocked by SmartScreen anymore) is not that clear.
The second unfortunately fact is that SmartScreen will be triggered by each new version of the app.
What you can do to execute Media-Extractor:

- Tell SmartScreen that you trust the app (must only declared once)
- Disable SmartScreen according to [this article](https://support.microsoft.com/en-us/help/17443/microsoft-edge-smartscreen-faq)
- Wait until enough people have downloaded and executed Media-Extractor

Note: Similar mechanisms may be also triggered by Browsers, like Edge, Chrome or Vivaldi

## Development Dependencies

The following libraries / dependencies are necessary for the development of Media-Extractor. All of them are maintained by NuGet:

- [SevenZipExtractor](https://github.com/adoconnection/SevenZipExtractor)
- [Ookii.Dialogs.Wpf](https://github.com/ookii-dialogs/ookii-dialogs-wpf)
- [AdonisUI](https://github.com/benruehl/adonis-ui/) and AdonisUI.ClassicTheme
- [NanoXLSX](https://github.com/rabanti-github/NanoXLSX) (for TranslationHelper project)
- [Mono.Options](https://github.com/xamarin/XamarinComponents/tree/main/XPlat/Mono.Options) (for TranslationHelper project)

## Additional Development Projects

- TranslationHelper (import/export utility, used for the translation of Media-Extractor)
- InstallerBootstrap (utility to post-build the installer script after building Media-Extractor)
- MediaExtractorInstaller ([Inno Setup](https://jrsoftware.org/isinfo.php) project to create the installer file)

## License

Media-Extractor is developed and distributed freely and without any costs, under the [MIT license](https://opensource.org/licenses/MIT).

[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Frabanti-github%2FMedia-Extractor.svg?type=large)](https://app.fossa.io/projects/git%2Bgithub.com%2Frabanti-github%2FMedia-Extractor?ref=badge_large)
