# Custom Translations for WXL

There are some customized terms in the WXL files for the installer localization.
To translate the, a LLM based translation can be used.

## Prompt

Replace *TARGET* with the target language you want to translate to, e.g. "French", "German", etc.


Copy from here ->

You are a translation expert for computer programs. Translate the following terms from 'English' into '*TARGET*' as target language. The terms are to localize a Windows installer. Use the values of the inner XML text as source. The XML attribute "Id" describes the purpose. Use the commonly used computer terms of the target languages (e.g. ""télécharger"" in French for ""download"", or ""Abbrechen"" in German for ""cancel""). Do not change, remove or add any attribute. Try to keep the length of the translated text roughly at the size as the original 'English' texts (translations can be shorter). Create a XML tags with the same structure as the provided input.
Ensure that all placeholders (e.g. [1]) are present in the translations. Use new lines (\n) in translations in the same way as in the 'English' original texts. Ensure that the output XML structure is not become broken.

Here is the input XML:
```xml
<!-- CUSTOM SECTION -->
	<String Id="LanguageSelection_Title" Overridable="yes">Select language</String>
	<String Id="LegacyCheck_Title" Overridable="yes">Checking existing installations</String>
	<String Id="LegacyCheck_TitleDescription" Overridable="yes">The installer is searching for legacy installations.</String>
	<String Id="previousInstallationWasDetected" Overridable="yes">
A previous version has been detected.
This version must be removed first, since it was installed in a non-compatible way.

The old version will uninstalled automatically if you click on Next.</String>
	<String Id="ProgressDlgTextChecking" Overridable="yes">Please wait while the Setup Wizard check the installation.</String>
    <String Id="InstallationFeatureProgramFiles" Overridable="yes">Program files</String>
    <String Id="InstallationFeatureProgramFilesDescription" Overridable="yes">Storing the program files (mandatory)</String>
    <String Id="InstallationFeatureDesktopIcon" Overridable="yes">Desktop icon</String>
    <String Id="InstallationFeatureDesktopIconDescription" Overridable="yes">Adding an icon to the desktop</String>
    <String Id="InstallationFeatureStartIcon" Overridable="yes">Start icon</String>
    <String Id="InstallationFeatureStartIconDescription" Overridable="yes">Adding an icon to the start menu</String>
    <String Id="InstallationFeatureRegisterExplorer" Overridable="yes">Register to Explorer</String>
    <String Id="InstallationFeatureRegisterExplorerDescription" Overridable="yes">Register the application in the Explorer as a default application to open files</String>
```

<- copy until here