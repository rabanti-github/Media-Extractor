# Media-Extractor - Setup Project

Setup Project is the new installer project for Media-Extractor. It replaces the previous InnoSetup project.
The project is based on [WixSharp](https://github.com/oleg-shilo/wixsharp) that is based on [WiX Toolset](https://github.com/wixtoolset).
Due to license concerns, the installer is currently based on WiX 4.x, although newer versions are available.

## Dependencies

The following dependencies are required to build the Setup Project:

- WixSharp-wix4.WPF
- Wix.Toolset.Dtf.WindowsInstaller
- WixToolset.Mba.Core
- WixToolset.UI.wixext
- MediaExtractor (main application project)

Note that the following classes have static dependencies (references to Properties) to the MediaExtractor project. If the application project changes, these classes must be revises possibly:

- PropertiesReader.cs
- Constants.cs

## Building

The setup project is normally built automatically by the main solution. There are no special requirements to build the project, as long as all files are provided, that are normally created from the post-build action of the MediaExtractor project.
The build result (an MSI file) is placed in the folder "Install", which is located in the top hierarchy of the solution.

## License

MIT License
