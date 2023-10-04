/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2023
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using SevenZipExtractor;
using System;
using System.Collections.Generic;
using System.IO;

namespace MediaExtractor
{
    /// <summary>
    /// Class to resolve archives either by the file name or the stream (guessed)
    /// </summary>
    public static class ArchiveResolver
    {

        private static Dictionary<string, SevenZipFormat> archiveFormats = null;

        /// <summary>
        /// Dictionary of supported archive formats
        /// </summary>
        public static Dictionary<string, SevenZipFormat> ArchiveFormats
        {
            get
            {
                if (archiveFormats == null)
                {
                    GetFormats();
                }
                return archiveFormats;
            }
        }

        /// <summary>
        /// Singleton initializer to get the supported archive formats
        /// </summary>
        private static void GetFormats()
        {
            SevenZipFormat[] formats = Enum.GetValues(typeof(SevenZipFormat)) as SevenZipFormat[];
            archiveFormats = new Dictionary<string, SevenZipFormat>(formats.Length);
            foreach (SevenZipFormat format in formats)
            {
                archiveFormats.Add(Enum.GetName(typeof(SevenZipFormat), format).ToLower(), format);
            }
        }

        /// <summary>
        /// Method to open an archive, initially by its file extension
        /// </summary>
        /// <param name="stream">Memory stream of the archive</param>
        /// <param name="extension">File extension of the archive</param>
        /// <returns></returns>
        public static ArchiveFile Open(MemoryStream stream, string extension)
        {
            ArchiveFile archive;
            string error;
            if (ArchiveFormats.ContainsKey(extension.ToLower()))
            {
                if (OpenArchive(ref stream, ArchiveFormats[extension.ToLower()], out archive, out error))
                {
                    return archive;
                }
            }
            if (TryOpen(ref stream, out archive, out error))
            {
                return archive;
            }
            else
            {
                throw new IOException(error);
            }
        }

        /// <summary>
        /// Method to try opening an archive without defined format
        /// </summary>
        /// <param name="stream">Memory stream of the archive</param>
        /// <param name="archive">Opened archive as out parameter. May be null in case of an error</param>
        /// <param name="error">Error message. May be empty, if no error occurred</param>
        /// <returns>True if the extraction was successful, otherwise false</returns>
        private static bool TryOpen(ref MemoryStream stream, out ArchiveFile archive, out string error)
        {
            ArchiveFile arch;
            string err;
            if (OpenArchive(ref stream, SevenZipFormat.Zip, out arch, out err))
            {
                archive = arch;
                error = string.Empty;
                return true;
            }
            foreach (KeyValuePair<string, SevenZipFormat> format in ArchiveFormats)
            {
                if (format.Value == SevenZipFormat.Zip)
                {
                    continue; // Already checked
                }
                if (OpenArchive(ref stream, format.Value, out arch, out err))
                {
                    archive = arch;
                    error = string.Empty;
                    return true;
                }
            }
            error = err;
            archive = null;
            return false;
        }

        /// <summary>
        /// Method to try opening an archive, based on an archive type
        /// </summary>
        /// <param name="stream">Memory stream of the archive</param>
        /// <param name="format">Predefined archive format</param>
        /// <param name="archive">Opened archive as out parameter. May be null in case of an error</param>
        /// <param name="error">Error message. May be empty, if no error occurred</param>
        /// <returns>True if the extraction was successful, otherwise false</returns>
        private static bool OpenArchive(ref MemoryStream stream, SevenZipFormat format, out ArchiveFile archive, out string error)
        {
            try
            {
                archive = new ArchiveFile(stream, format);
                if (archive.Entries.Count != 0)
                {
                    error = string.Empty;
                    return true;
                }
                else // will be triggered when the archive is valid but empty
                {
                    error = "Empty Archive";
                    return false;
                }
                
            }
            catch (Exception ex)
            {
                archive = null;
                stream.Position = 0;
                error = ex.Message;
                return false;
            }
        }

    }
}
