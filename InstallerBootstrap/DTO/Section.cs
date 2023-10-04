/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2023
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using System.Collections.Generic;

namespace InstallerBootstrap.DTO
{
    public class Section
    {
        public string Name { get; set; }
        public List<Setting> Settings { get; set; }
        public bool IsUserSection { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Section otherSection = (Section)obj;

            // Compare sections by name and whether they are application settings
            return Name == otherSection.Name && IsUserSection == otherSection.IsUserSection;
        }

        public override int GetHashCode()
        {
            return (Name + IsUserSection.ToString()).GetHashCode();
        }
    }
}
