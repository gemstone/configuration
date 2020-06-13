//******************************************************************************************************
//  SQLiteConfigurationOptions.cs - Gbtc
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

using Microsoft.Data.Sqlite;

namespace Gemstone.Configuration.SQLite
{
    /// <summary>
    /// Defines parameters for the <see cref="SQLiteConfigurationSource"/>.
    /// </summary>
    public class SQLiteConfigurationOptions
    {
        private SqliteConnectionStringBuilder ConnectionStringBuilder { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="SQLiteConfigurationOptions"/> class.
        /// </summary>
        public SQLiteConfigurationOptions()
        {
            ConnectionStringBuilder = new SqliteConnectionStringBuilder();
            ConnectionStringBuilder.Mode = SqliteOpenMode.ReadWriteCreate;
            TableName = "Setting";
        }

        /// <summary>
        /// Gets or sets the name of the table
        /// that stores configuration parameters.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the path to the database file.
        /// </summary>
        public string DataSource
        {
            get => ConnectionStringBuilder.DataSource;
            set => ConnectionStringBuilder.DataSource = value;
        }

        /// <summary>
        /// Gets or sets the encryption key.
        /// </summary>
        public string Password
        {
            get => ConnectionStringBuilder.Password;
            set => ConnectionStringBuilder.Password = value;
        }

        /// <summary>
        /// Gets or sets the flag that determines whether
        /// to enable foreign key constraints.
        /// </summary>
        public bool? ForeignKeys
        {
            get => ConnectionStringBuilder.ForeignKeys;
            set => ConnectionStringBuilder.ForeignKeys = value;
        }

        /// <summary>
        /// Gets or sets the cahcing mode used
        /// by connections to the database.
        /// </summary>
        public SqliteCacheMode Cache
        {
            get => ConnectionStringBuilder.Cache;
            set => ConnectionStringBuilder.Cache = value;
        }

        /// <summary>
        /// Gets or sets the flag to indicate whether to enable recursive triggers.
        /// </summary>
        public bool RecursiveTriggers
        {
            get => ConnectionStringBuilder.RecursiveTriggers;
            set => ConnectionStringBuilder.RecursiveTriggers = value;
        }

        /// <summary>
        /// Gets the connection string as configured
        /// by the other properties in this class.
        /// </summary>
        public string ConnectionString =>
            ConnectionStringBuilder.ConnectionString;
    }
}
