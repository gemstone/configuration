//******************************************************************************************************
//  ConfigurationBuilderExtensions.cs - Gbtc
//
//  Copyright © 2023, Grid Protection Alliance.  All Rights Reserved.
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
using System.IO;
using Gemstone.Configuration.AppSettings;
using Gemstone.Configuration.INIConfigurationExtensions;
using Gemstone.Configuration.ReadOnly;
using Gemstone.Configuration.SQLite;
using Microsoft.Extensions.Configuration;
using static Gemstone.Configuration.INIConfigurationHelpers;

namespace Gemstone.Configuration;

/// <summary>
/// Defines extensions for setting up configuration defaults for Gemstone projects.
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Configures the builder using the defined settings from Gemstone project configuration sources.
    /// </summary>
    /// <param name="builder">The configuration builder.</param>
    /// <param name="settings">Settings for configuring default sources.</param>
    /// <returns>The configuration builder.</returns>
    /// <remarks>
    /// This extension function configures common configuration sources for a Gemstone project. The
    /// <see cref="Settings"/> instance controls the configuration sources that are available. Handling
    /// of settings are defined in a hierarchy where the settings are loaded are in the following
    /// priority order, from lowest to hightest:
    /// <list type="bullet">
    ///   <item>INI file (defaults.ini) - Machine Level</item>
    ///   <item>INI file (settings.ini) - Machine Level</item>
    ///   <item>SQLite database (settings.db) - User Level</item>
    ///   <item>Environment variables - Machine Level</item>
    ///   <item>Environment variables - User Level</item>
    /// </list>
    /// </remarks>
    public static IConfigurationBuilder ConfigureGemstoneDefaults(this IConfigurationBuilder builder, Settings settings)
    {
        return builder.ConfigureGemstoneDefaults(
            settings.ConfigureAppSettings,
            settings.INIFile != ConfigurationOperation.Disabled,
            settings.SQLite != ConfigurationOperation.Disabled,
            settings.EnvironmentalVariables != ConfigurationOperation.Disabled,
            settings.SplitDescriptionLines,
            settings.ConfiguredINIPath);
    }

    /// <summary>
    /// Configures the builder using the default configuration sources for Gemstone projects.
    /// </summary>
    /// <param name="builder">The configuration builder.</param>
    /// <param name="configureAppSettings">Action for configuring default app settings.</param>
    /// <param name="useINI">INI file can be produced in lieu of a UI for configuration.</param>
    /// <param name="useSQLite">Use SQLite for user configuration storage.</param>
    /// <param name="useEnvironmentalVariables">Use environmental variables for configuration.</param>
    /// <param name="splitDescriptionLines">Split long description lines into multiple lines.</param>
    /// <param name="configuredINIPath">Configured INI path. Set to <c>null</c> for default %ProgramData% path.</param>
    /// <returns>The configuration builder.</returns>
    /// <remarks>
    /// This extension function configures common configuration sources for a Gemstone project. The
    /// provided parameters control the configuration sources that are available. Handling of settings
    /// are defined in a hierarchy where the settings are loaded are in the following priority order,
    /// from lowest to hightest:
    /// <list type="bullet">
    ///   <item>INI file (defaults.ini) - Machine Level</item>
    ///   <item>INI file (settings.ini) - Machine Level</item>
    ///   <item>SQLite database (settings.db) - User Level</item>
    ///   <item>Environment variables - Machine Level</item>
    ///   <item>Environment variables - User Level</item>
    /// </list>
    /// </remarks>
    public static IConfigurationBuilder ConfigureGemstoneDefaults(
        this IConfigurationBuilder builder, 
        Action<IAppSettingsBuilder> configureAppSettings, 
        bool useINI = false, 
        bool useSQLite = true, 
        bool useEnvironmentalVariables = true, 
        bool splitDescriptionLines = false,
        string? configuredINIPath = null)
    {
        builder.AddAppSettings(configureAppSettings).AsReadOnly();

        if (useINI)
            builder.AddGemstoneINIFile(splitDescriptionLines, configuredINIPath).AsReadOnly();

        if (useSQLite)
            builder.AddSQLite();

        if (useEnvironmentalVariables)
            builder.AddEnvironmentVariables("GEMSTONE_").AsReadOnly();

        return builder;
    }

    private static IConfigurationBuilder AddGemstoneINIFile(this IConfigurationBuilder builder, bool splitDescriptionLines, string? configuredINIPath)
    {
        IConfiguration configuration = builder.Build();
        string iniFilePath = GetINIFilePath("settings.ini", configuredINIPath);

        builder.AddIniFile(iniFilePath, false, true);

        // Create an empty settings file if none exists
        if (!File.Exists(iniFilePath))
            GetINIFileWriter(iniFilePath).Dispose();

        // Create, or recreate, default settings file
        string defaultContents = configuration.GenerateINIFileContents(INIGenerationOption.UncommentedValue, splitDescriptionLines);
        string defaultsPath = GetINIFilePath("defaults.ini", configuredINIPath);
        using TextWriter writer = GetINIFileWriter(defaultsPath);

        writer.Write(defaultContents);

        return builder;
    }
}
