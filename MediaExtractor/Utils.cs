/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2018
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MediaExtractor
{
    /// <summary>
    /// Utils Class
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Opens Windows Explorer at the passed location
        /// </summary>
        /// <param name="location">Location to open</param>
        /// <returns>True, if the location could be opened in Explorer, otherwise false</returns>
        public static bool ShowInExplorer(string location)
        {
            try
            {
                Process.Start(location);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
            
        }

        /// <summary>
        /// Returns a formatted file size (Windows format)
        /// </summary>
        /// <param name="size">File size in bytes</param>
        /// <returns>Formatted size as string</returns>
        public static string ConvertFileSize(long size)
        {
            int index = 0;
            while (size >= 1024 && index < 4)
            {
                index++;
                size = size / 1024;
            }
            return String.Format("{0:0.##} {1}", size, new string[] {"B", "KB", "MB", "GB", "TB"}[index]);
        }

        public static string GetNextFileName(string fullPath)
        {
            FileInfo fi;
            string path = fullPath;
            string name, numberString;
            string d = Path.DirectorySeparatorChar.ToString();
            int number = 1;
            Regex rx = new Regex(@"^(.+\()(\d+)(\))$");
            Match match;
            try
            {
                while (true)
                {
                    if (File.Exists(path) == false)
                    {
                        return path;
                    }
                    fi = new FileInfo(path);
                    name = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);
                    match = rx.Match(name);
                    if (match.Groups.Count > 3)
                    {
                        numberString = match.Groups[2].Value;
                        int.TryParse(numberString, out number);
                        number++;
                        path = fi.DirectoryName + d + match.Groups[1].Value + number + match.Groups[3].Value + fi.Extension;
                    }
                    else
                    {
                        path = fi.DirectoryName + d + name + "(" + number + ")" + fi.Extension;
                    }
                }
            }
            catch (Exception e)
            {
                return fullPath + "(error).tmp";
            }
        }


#region CRC

        private const int bufferSize = 4096;
        private const uint polynome = 0xEDB88320;
        private static bool inizialized = false;
        private static uint startValue = 0xffffffff;
        private static uint[] crcTable;


        public static uint GetCrc(string fileName)
        {
            uint number;
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open))
                {
                    number = GetCrc(fs);
                }
            }
            catch (Exception ex)
            {
                number = 0;
            }
            return number;
        }

        public static uint GetCrc(Stream stream)
        {
            if (inizialized == false) // InitializeCrc
            { InitializeCrc(); }
            byte[] streambuffer = new byte[bufferSize];
            byte[] hash = new byte[4];
            int i;
            ulong pointer;
            long bytecounter = 0;
            uint tempCrc = startValue;
            long lStreamBuffer = streambuffer.Length;
            long lStream = stream.Length;
            while (stream.Read(streambuffer, 0, bufferSize) > 0)
            {
                for (i = 0; i < lStreamBuffer; i++)
                {
                    if (bytecounter >= lStream) { break; }
                    pointer = (tempCrc & 0xFF) ^ streambuffer[i];
                    tempCrc >>= 8;
                    tempCrc ^= crcTable[pointer];
                    bytecounter++;
                }
            }
            pointer = tempCrc ^ startValue;
            hash[0] = (byte)((pointer >> 0) & 0xFF);
            hash[1] = (byte)((pointer >> 8) & 0xFF);
            hash[2] = (byte)((pointer >> 16) & 0xFF);
            hash[3] = (byte)((pointer >> 24) & 0xFF);
            return BitConverter.ToUInt32(hash, 0);
        }

        private static void InitializeCrc()
        {
            crcTable = new uint[256];
            uint temp;
            for (uint i = 0; i < 256; i++)
            {
                temp = i;
                for (int j = 8; j > 0; j--)
                {
                    if ((temp & 1) == 1)
                    { temp = (temp >> 1) ^ polynome; }
                    else
                    { temp >>= 1; }
                }
                crcTable[i] = temp;
            }
            inizialized = true;
        }
#endregion

    }
}
