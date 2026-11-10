# Media-Extractor - Setup Project

Setup Project is the new installer project for Media-Extractor. It replaces the previous InnoSetup project.
The project is based on [WixSharp](https://github.com/oleg-shilo/wixsharp) that is based on [WiX Toolset](https://github.com/wixtoolset).
Due to license concerns, the installer is pinned to WiX 5.0.2. WiX 6 and later must not be used because they are governed by the Open Source Maintenance Fee.

## Dependencies

The following dependencies are required to build the Setup Project:

- WixSharp-wix4.WPF
- Wix.Toolset.Dtf.WindowsInstaller
- WixToolset.Mba.Core
- WixToolset.UI.wixext
- WiX Toolset CLI 5.0.2 (restored from the repository-local tool manifest)
- MediaExtractor (main application project)

Restore the pinned CLI from the `SetupProject` directory before building:

```powershell
dotnet tool restore
```

Do not replace this with an unversioned global tool installation. WixSharp also enforces version 5.0.2 when generating the installer.

WixSharp is intentionally kept on 2.6.2 for this experimental fix. The completion-dialog repair is implemented locally and does not require a framework upgrade.

Note that the following classes have static dependencies (references to Properties) to the MediaExtractor project. If the application project changes, these classes must be revises possibly:

- PropertiesReader.cs
- Constants.cs

## Building

The setup project is normally built automatically by the main solution. There are no special requirements to build the project, as long as all files are provided, that are normally created from the post-build action of the MediaExtractor project.
The build result (an MSI file) is placed in the folder "Install", which is located in the top hierarchy of the solution.

To compile the setup application without generating an MSI, set `SkipInstallerGeneration`:

```powershell
dotnet build -p:SkipInstallerGeneration=true
```

## License

MIT License
