/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2023
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using System.Collections.Generic;

namespace InstallerBootstrap.DTO
{
    public class Setting
    {
        public string Name { get; set; }
        public string SerializeAs { get; set; }
        public string Value { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Setting setting &&
                   Name == setting.Name &&
                   SerializeAs == setting.SerializeAs &&
                   Value == setting.Value;
        }

        public override int GetHashCode()
        {
            int hashCode = -2100274719;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SerializeAs);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            return hashCode;
        }
    }

}
