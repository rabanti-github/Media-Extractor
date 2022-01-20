/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2022
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MediaExtractor
{
    /// <summary>
    /// Class to deal with monitors and screen resolution
    /// </summary>
    public class ScreenHandler
    {
        /// <summary>
        /// Border width of the window to compensate window dimensions in full screen mode
        /// </summary>
        private const double BORDER_WIDTH = 8.1d;

        private readonly string deviceName;
        private readonly bool isPrimary;

        /// <summary>
        /// X Position of the screen (Left)
        /// </summary>
        public double X { get; private set; }
        /// <summary>
        ///  Y Position of the screen (Top)
        /// </summary>
        public double Y { get; private set; }
        /// <summary>
        /// Width of the screen
        /// </summary>
        public double Width { get; private set; }
        /// <summary>
        /// Height of the screen
        /// </summary>
        public double Height { get; private set; }

        /// <summary>
        /// Constructor with screen object as parameter
        /// </summary>
        /// <param name="screen">Winforms Screen object</param>
        protected internal ScreenHandler(Screen screen)
        {
            X = screen.WorkingArea.Left;
            Y = screen.WorkingArea.Top;
            Width = screen.WorkingArea.Width;
            Height = screen.WorkingArea.Height;
            deviceName = screen.DeviceName;
            isPrimary = screen.Primary;
        }

        /// <summary>
        /// Method to get all system screens
        /// </summary>
        /// <returns>IEnumerable of Winforms screen objects</returns>
        private static IEnumerable<ScreenHandler> GetAllScreens()
        {
            foreach (Screen s in Screen.AllScreens) 
            {
                yield return new ScreenHandler(s);
            }
        }

        /// <summary>
        /// Gets the primary screen
        /// </summary>
        /// <returns>Screen that is determined as primary monitor</returns>
        public static ScreenHandler GetPrimaryScreen()
        {
            return GetAllScreens().First(s => s.isPrimary);
        }

        /// <summary>
        /// Gets the screen name by the window position
        /// </summary>
        /// <param name="left">Left (x) position of the window</param>
        /// <param name="top">Top (y) position of the window</param>
        /// <returns>Name of the screen</returns>
        public static string GetScreenNameByRect(double left, double top)
        {
            List<ScreenHandler> screens = new List<ScreenHandler>(GetAllScreens());
            foreach(ScreenHandler screen in screens)
            {
                if (left >= screen.X && left < (screen.X + screen.Width) && top >= screen.Y && top < (screen.Y + screen.Height))
                {
                    return screen.deviceName;
                }
            }
            return GetPrimaryScreen().deviceName;
        }

        /// <summary>
        /// Gets the screen by its name
        /// </summary>
        /// <param name="screenName">Name of the screen</param>
        /// <param name="screen">Screen as output parameter. Is null, if the screen with the defined name was not found</param>
        /// <returns>True if the screen was found, otherwise false</returns>
        public static bool GetScreenByName(string screenName, out ScreenHandler screen)
        {
           screen = GetAllScreens().FirstOrDefault(s => s.deviceName == screenName);
            if (screen == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates the window position against the given screens
        /// </summary>
        /// <param name="x">Left (x) position of the window</param>
        /// <param name="y">>Top (y) position of the window</param>
        /// <param name="width">Width of the window</param>
        /// <param name="height">Height of the window</param>
        /// <returns>True if the window is in a valid screen area, otherwise false (e.g. if a screen was disabled)</returns>
        public bool ValidateScreen(double x, double y, double width, double height)
        {
            if (x < this.X || y < this.Y || width > (this.Width + BORDER_WIDTH * 2) || height > (this.Height + BORDER_WIDTH * 2))
            {
                return false;
            }
            return true;
        }

    }
}
