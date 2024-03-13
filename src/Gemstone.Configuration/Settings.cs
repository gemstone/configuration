//******************************************************************************************************
//  Settings.cs - Gbtc
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
//  10/10/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Gemstone.Configuration;

/// <summary>
/// Defines system settings for an application.
/// </summary>
public abstract class Settings
{
    /// <summary>
    /// Defines the configuration section name for system settings.
    /// </summary>
    public const string SystemSettings = nameof(SystemSettings);

    /// <summary>
    /// Gets the <see cref="IConfiguration"/> instance used to populate <see cref="Settings"/>.
    /// </summary>
    public IConfiguration Configuration { get; private set; } = default!;

    /// <summary>
    /// Creates a new <see cref="Settings"/> instance.
    /// </summary>
    protected Settings()
    {
        Instance = this;
    }

    /// <summary>
    /// Initializes new <see cref="Settings"/> instance.
    /// </summary>
    /// <param name="configuration">Configuration used to populate settings.</param>
    public virtual void Initialize(IConfiguration configuration)
    {
        Configuration = configuration;

        // Ensure system settings section exists
        Configuration.GetSection(SystemSettings);

    }

    /// <summary>
    /// Gets the command line switch mappings for <see cref="Settings"/>.
    /// </summary>
    public Dictionary<string, string> SwitchMappings => new();

    /// <summary>
    /// Gets the default instance of <see cref="Settings"/>.
    /// </summary>
    public static Settings? Instance { get; private set; }
}
