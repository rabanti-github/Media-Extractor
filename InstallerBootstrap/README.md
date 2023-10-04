# Media-Extractor Installer Bootstrap Utility

This project is the link between the actual Media-Extractor application project and the setup project

## Usage

The utility resolves the application version of Media-Extractor and several ebedded properties, like the project website, the author or path to the appliucation files.

The application is usually automatically executed when building Media-Extractor. Then, the above mentioned properties are extracted and written as installer file for **Inno Setup**

The (console) application takes four arguments:

| Order | Argument | Description |
| --- | --- | --- |
| 1 | Template path | The absolute Path to the Inno Setup template file |
| 2 | Installer file path | The absolut path to the Inno Setup installer file (*.iss), used to buld the intsaller |
| 3 | Assemply path | The absolute path to the Media-Extractor assebly and all necessary files  |
| 4 | Assembly file | The file name of the executable (e.g. MediaExtractor.exe) within the assembly path |

## Template Variables

All template variables (e.g. *TPL_MY_APP_VERSION*) are hardcoded in the class **TemplateProcessor** and must match in the bootstrap utilit and the setup template file.
Therefore, any change mut be performed in the bootstrap utility and the template file (*.TPL) in the Inno Setup installer project.

## License

MIT License
