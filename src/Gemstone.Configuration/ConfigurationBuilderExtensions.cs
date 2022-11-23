//******************************************************************************************************
//  ConfigurationBuilderExtensions.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/14/2020 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Gemstone.Configuration.AppSettings;
using Gemstone.Configuration.ReadOnly;
using Gemstone.Configuration.SQLite;
using Microsoft.Extensions.Configuration;

namespace Gemstone.Configuration
{
    /// <summary>
    /// Defines extensions for setting up configuration defaults for Gemstone projects.
    /// </summary>
    public static class ConfigurationBuilderExtensions
    {
        /// <summary>
        /// Configures the builder using the default configuration sources for Gemstone projects.
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        /// <param name="configureAppSettings">Action for configuring default app settings.</param>
        /// <param name="useINI">INI file can be produced in lieu of a UI for configuration.</param>
        /// <returns>The configuration builder.</returns>
        public static IConfigurationBuilder ConfigureGemstoneDefaults(this IConfigurationBuilder builder, Action<IAppSettingsBuilder> configureAppSettings, bool useINI = false)
        {
            builder.AddAppSettings(configureAppSettings).AsReadOnly();

            if (useINI)
                builder.AddGemstoneINIFile().AsReadOnly();

            builder.AddSQLite();
            builder.AddEnvironmentVariables("GEMSTONE_").AsReadOnly();
            return builder;
        }

        private static IConfigurationBuilder AddGemstoneINIFile(this IConfigurationBuilder builder)
        {
            IConfiguration configuration = builder.Build();
            string defaultContents = GenerateDefaultINIFileContents(configuration);

            string iniPath = GetINIFilePath("settings.ini");
            builder.AddIniFile(iniPath, false, true);

            if (File.Exists(iniPath))
            {
                string contents = File.ReadAllText(iniPath);

                if (contents == defaultContents)
                    return builder;

                iniPath = GetINIFilePath("defaults.ini");

                if (File.Exists(iniPath))
                {
                    contents = File.ReadAllText(iniPath);

                    if (contents == defaultContents)
                        return builder;
                }
            }

            using (TextWriter writer = GetINIFileWriter(iniPath))
                writer.Write(defaultContents);

            return builder;
        }

        private static string GetINIFilePath(string fileName)
        {
            Environment.SpecialFolder specialFolder = Environment.SpecialFolder.CommonApplicationData;
            string appDataPath = Environment.GetFolderPath(specialFolder);

            Assembly? entryAssembly = Assembly.GetEntryAssembly();
            string? appName = entryAssembly?.GetName().Name
                ?? Process.GetCurrentProcess().ProcessName;

            return Path.Combine(appDataPath, appName, fileName);
        }

        private static string GenerateDefaultINIFileContents(this IConfiguration configuration)
        {
            static IEnumerable<string> Split(string str, int maxLineLength)
            {
                string[] lines = str.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                foreach (string line in lines)
                {
                    string leftover = line.TrimStart();

                    // Lines in the original text that contain
                    // only whitespace will be returned as-is
                    if (leftover.Length == 0)
                        yield return line;

                    while (leftover.Length > 0)
                    {
                        char[] chars = leftover
                            .Take(maxLineLength + 1)
                            .Reverse()
                            .SkipWhile(c => !char.IsWhiteSpace(c))
                            .SkipWhile(char.IsWhiteSpace)
                            .Reverse()
                            .ToArray();

                        if (!chars.Any())
                        {
                            // Tokens that are longer than the maximum length will
                            // be returned (in their entirety) on their own line;
                            // maxLineLength is just a suggestion
                            chars = leftover
                                .TakeWhile(c => !char.IsWhiteSpace(c))
                                .ToArray();
                        }

                        string splitLine = new(chars);
                        leftover = leftover.Substring(splitLine.Length).TrimStart();
                        yield return splitLine;
                    }
                }
            }

            static bool HasAppSetting(IConfiguration section) =>
                section.GetChildren().Any(HasAppSettingDescription);

            static bool HasAppSettingDescription(IConfigurationSection setting) =>
                setting.GetAppSettingDescription() != null;

            static string ConvertSettingToINI(IConfigurationSection setting)
            {
                string key = setting.Key;
                string initialValue = setting.GetAppSettingInitialValue() ?? "";
                string description = setting.GetAppSettingDescription() ?? "";

                // Break up long descriptions to be more readable in the INI file
                IEnumerable<string> descriptionLines = Split(description, 78)
                    .Select(line => $"; {line}");

                string multilineDescription = string.Join(Environment.NewLine, descriptionLines);

                string[] lines = new[]
                {
                    $"{multilineDescription}",
                    $";{key}={initialValue}"
                };

                return string.Join(Environment.NewLine, lines);
            }

            static string ConvertConfigToINI(IConfiguration config)
            {
                IEnumerable<string> settings = config.GetChildren()
                    .Where(HasAppSettingDescription)
                    .OrderBy(setting => setting.Key)
                    .Select(setting => ConvertSettingToINI(setting));

                string settingSeparator = string.Format("{0}{0}", Environment.NewLine);
                string settingsText = string.Join(settingSeparator, settings);

                // The root section has no heading
                if (!(config is ConfigurationSection section))
                    return settingsText;

                return string.Join(Environment.NewLine, $"[{section.Key}]", settingsText);
            }

            // Root MUST go before all other sections, so the order is important:
            //     1. Sort by section key
            //     2. Prepend root
            //     3. Filter out sections without any app settings
            IEnumerable<string> appSettingsSections = configuration.AsEnumerable()
                .Select(kvp => configuration.GetSection(kvp.Key))
                .OrderBy(section => section.Key)
                .Prepend(configuration)
                .Where(HasAppSetting)
                .Select(ConvertConfigToINI);

            string sectionSeparator = string.Format("{0}{0}", Environment.NewLine);
            return string.Join(sectionSeparator, appSettingsSections);
        }

        private static TextWriter GetINIFileWriter(string path)
        {
            if (!File.Exists(path))
            {
                string directoryPath = Path.GetDirectoryName(path) ?? string.Empty;
                Directory.CreateDirectory(directoryPath);
            }

            return File.CreateText(path);
        }
    }
}
