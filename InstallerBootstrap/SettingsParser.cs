/*
 * Media Extractor is an application to preview and extract packed media in Microsoft Office files (e.g. Word, PowerPoint or Excel documents)
 * Copyright Raphael Stoeckli © 2023
 * This program is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using InstallerBootstrap.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace InstallerBootstrap
{
    public static class SettingsParser
    {
        public static List<Section> ParseConfiguration(string configFile)
        {
            try
            {
                XDocument doc = XDocument.Load(configFile);
                List<Section> sections = new List<Section>();

                // Parse applicationSettings
                IEnumerable<XElement> appSettings = doc.Descendants("applicationSettings").Descendants();
                foreach (XElement settingElement in appSettings)
                {
                    Section section = new Section
                    {
                        Name = settingElement.Name.LocalName,
                        Settings = settingElement.Elements("setting")
                            .Select(ParseSetting)
                            .OrderBy(setting => setting.Name)
                            .ToList()
                    };
                    if (section.Settings.Count > 0 && !sections.Contains(section))
                    {
                        sections.Add(section);
                    }
                }

                // Parse userSettings
                IEnumerable<XElement> userSettings = doc.Descendants("userSettings").Descendants();
                foreach (XElement settingElement in userSettings)
                {
                    Section section = new Section
                    {
                        Name = settingElement.Name.LocalName,
                        Settings = settingElement.Elements("setting")
                            .Select(ParseSetting)
                            .OrderBy(setting => setting.Name)
                            .ToList()
                    };
                    section.IsUserSection = true;
                    if (section.Settings.Count > 0 && !sections.Contains(section))
                    {
                        sections.Add(section);
                    }
                }

                return sections;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing configuration file: " + ex.Message);
                return null;
            }
        }

        private static Setting ParseSetting(XElement settingElement)
        {
            Setting setting = new Setting
            {
                Name = settingElement.Attribute("name")?.Value,
                SerializeAs = settingElement.Attribute("serializeAs")?.Value,
                Value = settingElement.Element("value")?.Value
            };
            return setting;
        }
    }
}
