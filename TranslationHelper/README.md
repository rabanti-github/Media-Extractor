# Media-Extractor - TranslationHelper

TranslationHelper is a console application and mainly to export the existing translation strings to an Excel file. Furthermore, an Excel file can be read to generate a markdown file (for Wiki maintenance)

## Usage

Example to write all translation strings to an Excel file:

   ```shell
   TranslationHelper -l=de-De -x=C:\temp\germanStrings.xlsx
   ```

Example to read a translation from an Excel file and to write it back to a markdown file:

   ```shell
   TranslationHelper -i=C:\temp\germanStrings.xlsx -m=C:\temp\markdown.md
   ```

Example to write a default markdown file:

   ```shell
   TranslationHelper -d=C:\temp\markdown.md
   ```

Example to write a default markdown file with externally loaded comments:

   ```shell
    TranslationHelper -c=C:\temp\comments.xlsx -d=C:\temp\markdown.md
   ```

## Options

| Short Option | Long Option | Description |
| --- | --- | --- |
| `-h` | `--help` | Shows the help |
| `-l=VALUE` | `--locale=VALUE` | Defines the locale to be written (e.g. 'en', 'de-DE'). If not valid or defined, the application default (en) will be used |
| `-x=VALUE` | `--xlsx=VALUE`  | Writes an Excel file with all translation strings in the defined locale at the defined path |
| `-i=VALUE` | `--input=VALUE` | Reads an Excel file with all translation strings from the defined path. The structure must be the same as generated with 'x' |
| `-m=VALUE` | `--markdown=VALUE` | Writes a markdown text based on a loaded Excel file at the defined path. The 'i' flag must be defined as well |
| `-d=VALUE` | `--default=VALUE`  | Writes a markdown text only with the default terms to the specified path
| `-c=VALUE` | `--comments=VALUE` | Reads an Excel file with term comments, where column A is the translation key and B the comment (starting with row 2). These comments will overrode existing ones |

## License

MIT License
