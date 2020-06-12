//******************************************************************************************************
//  SQLiteConfigurationProvider.cs - Gbtc
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
using Microsoft.Extensions.Configuration;

namespace Gemstone.Configuration.SQLite
{
    /// <summary>
    /// The provider of configuration from a <see cref="SQLiteConfigurationSource"/>.
    /// </summary>
    public class SQLiteConfigurationProvider : ConfigurationProvider
    {
        private string ConnectionString { get; }
        private string TableName { get; }
        private bool IgnoresModifications { get; }
        private bool IsTableCreated { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="SQLiteConfigurationProvider"/> class.
        /// </summary>
        /// <param name="options">The parameters for the configuration source.</param>
        public SQLiteConfigurationProvider(SQLiteConfigurationOptions options)
        {
            ConnectionString = options.ConnectionString;
            TableName = options.TableName;
            IgnoresModifications = options.IgnoreModifications;
        }

        /// <summary>
        /// Loads (or reloads) the data for this provider.
        /// </summary>
        public override void Load()
        {
            using SqliteConnection connection = new SqliteConnection(ConnectionString);
            connection.Open();
            CreateTable(connection);

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = $"SELECT Key, Value FROM {TableName}";

            using SqliteDataReader reader = command.ExecuteReader();
            Data.Clear();

            while (reader.Read())
            {
                string key = reader.GetString(0);
                string value = reader.GetString(1);
                Data[key] = value;
            }
        }

        /// <summary>
        /// Sets a value for a given key.
        /// </summary>
        /// <param name="key">The configuration key to set.</param>
        /// <param name="value">The value to set.</param>
        public override void Set(string key, string value)
        {
            if (IgnoresModifications)
                return;

            base.Set(key, value);

            using SqliteConnection connection = new SqliteConnection(ConnectionString);
            connection.Open();
            CreateTable(connection);

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = $"INSERT INTO {TableName} VALUES(@key, @value) ON CONFLICT(Key) DO UPDATE SET Value = @value";
            command.Parameters.AddWithValue("@key", key);
            command.Parameters.AddWithValue("@value", value);
            command.ExecuteNonQuery();
        }

        private void CreateTable(SqliteConnection connection)
        {
            if (IsTableCreated)
                return;

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = $"CREATE TABLE IF NOT EXISTS {TableName}(Key TEXT PRIMARY KEY, VALUE TEXT)";
            command.ExecuteNonQuery();
            IsTableCreated = true;
        }
    }
}
