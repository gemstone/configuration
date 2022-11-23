//******************************************************************************************************
//  SQLiteConfigurationSource.cs - Gbtc
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
//  06/12/2020 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using Microsoft.Extensions.Configuration;

namespace Gemstone.Configuration.SQLite
{
    /// <summary>
    /// The source for configuration stored in a SQLite database.
    /// </summary>
    public class SQLiteConfigurationSource : IConfigurationSource
    {
        private Action<SQLiteConfigurationOptions> OptionsAction { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="SQLiteConfigurationSource"/> class.
        /// </summary>
        /// <param name="optionsAction">The action called to set parameters for the source.</param>
        public SQLiteConfigurationSource(Action<SQLiteConfigurationOptions> optionsAction) =>
            OptionsAction = optionsAction;

        /// <summary>
        /// Builds an <see cref="IConfigurationProvider"/> from the SQLite configuration source.
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        /// <returns>The configuration provider.</returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            SQLiteConfigurationOptions options = new();
            OptionsAction(options);
            return new SQLiteConfigurationProvider(options);
        }
    }
}
