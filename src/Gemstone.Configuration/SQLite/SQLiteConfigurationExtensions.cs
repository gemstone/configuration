//******************************************************************************************************
//  SQLiteConfigurationExtensions.cs - Gbtc
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
//  06/12/2020 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Gemstone.Configuration.SQLite;

/// <summary>
/// Defines extensions for adding the <see cref="SQLiteConfigurationSource"/>
/// to an <see cref="IConfigurationBuilder"/> pipeline.
/// </summary>
public static class SQLiteConfigurationExtensions
{
    /// <summary>
    /// Adds the <see cref="SQLiteConfigurationSource"/> to the given <see cref="IConfigurationBuilder"/>.
    /// </summary>
    /// <param name="builder">The configuration builder.</param>
    /// <returns>The configuration builder.</returns>
    /// <remarks>
    /// <para>The SQLite configuration source will be configured with default parameters.</para>
    /// 
    /// <list type="bullet">
    ///   <item>DataSource=%PROGRAMDATA%\[AppName]\settings.db</item>
    ///   <item>TableName=Setting</item>
    ///   <item>ReadOnly=False</item>
    /// </list>
    /// </remarks>
    public static IConfigurationBuilder AddSQLite(this IConfigurationBuilder builder) =>
        builder.AddSQLite(ConfigureDefaults);

    /// <summary>
    /// Adds the <see cref="SQLiteConfigurationSource"/> to the given <see cref="IConfigurationBuilder"/>.
    /// </summary>
    /// <param name="builder">The configuration builder.</param>
    /// <param name="optionsAction">The action called to configure the <see cref="SQLiteConfigurationSource"/>.</param>
    /// <returns>The configuration builder.</returns>
    public static IConfigurationBuilder AddSQLite(this IConfigurationBuilder builder, Action<SQLiteConfigurationOptions> optionsAction)
    {
        IConfigurationSource configurationSource = new SQLiteConfigurationSource(optionsAction);
        builder.Add(configurationSource);
        return builder;
    }

    private static void ConfigureDefaults(SQLiteConfigurationOptions options)
    {
        Environment.SpecialFolder specialFolder = Environment.SpecialFolder.ApplicationData;
        string appDataPath = Environment.GetFolderPath(specialFolder);

        Assembly? entryAssembly = Assembly.GetEntryAssembly();
        string? name = entryAssembly?.GetName().Name
                       ?? Process.GetCurrentProcess().ProcessName;

        string appPath = Path.Combine(appDataPath, name);
        Directory.CreateDirectory(appPath);

        options.DataSource = Path.Combine(appPath, "settings.db");
    }
}
