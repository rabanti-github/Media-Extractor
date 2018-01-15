/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2018
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

    }
}
