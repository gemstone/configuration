//******************************************************************************************************
//  EvalConverter.cs - Gbtc
//
//  Copyright © 2024, Grid Protection Alliance.  All Rights Reserved.
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
//  04/11/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Gemstone.TypeExtensions;

namespace Gemstone.Configuration;

/// <summary>
/// Defines a type converter for <see cref="Eval"/> expressions.
/// </summary>
public partial class EvalConverter : TypeConverter
{
    /// <inheritdoc />
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    /// <inheritdoc />
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is not string expression)
            return base.ConvertFrom(context, culture, value);

        expression = expression.Trim();

        Eval eval = new(expression);

        // Handle substituting braced environment variables with expanded values, for example:
        //   Source Expression: [eval]:{env:APPDATA}\MyApp
        //  Updated Expression: [eval]:Environment.GetEnvironmentVariable("APPDATA")\MyApp

        // Handle substituting braced setting expressions with expanded / typed settings, for example:
        //    Source Expression: [eval]:{Alarming.UserDataProtectionTimeout} * 10.0D
        //   Updated Expression: [eval]:(double)(settings["Alarming"]["UserDataProtectionTimeout"]) * 10.0D

        // Replace each matching substitution pattern with a proper expansion
        foreach (Match match in s_settingSubstitution.Matches(expression))
        {
            string env = match.Groups["env"].Value;

            if (string.IsNullOrWhiteSpace(env))
            {
                // Handle "{Setting:Key}" setting substitution
                string section = match.Groups["section"].Value;
                string key = match.Groups["key"].Value;
                object? typedValue = Settings.Instance[section][key];

                while (typedValue is Eval evalInstance)
                    typedValue = evalInstance.Value;

                expression = expression.Replace(match.Value, typedValue is not null && typedValue is not string ?
                    $"({typedValue.GetType().GetReflectedTypeName()})(settings[\"{section}\"][\"{key}\"])" :
                    $"settings[\"{section}\"][\"{key}\"]");
            }
            else
            {
                // Handle "{env:NAME}" environment variable substitution
                expression = expression.Replace(match.Value, $"Environment.GetEnvironmentVariable(\"{env}\")");
            }
        }

        eval.TranspiledExpression = expression;

        // Returning "Eval" instance - accessing "Value" property will evaluate expression as needed
        return eval;
    }

    private const string SettingSubstitutionPattern = @"\{((?<section>\w+)\.(?<key>\w+)|env\:(?<env>\w+))\}";
    private static readonly Regex s_settingSubstitution;

    static EvalConverter()
    {
#if NET
        s_settingSubstitution = GenerateSettingSubstitutionPatternRegex();
#else
        s_settingSubstitution = new Regex(SettingSubstitutionPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
#endif
    }

#if NET
    [GeneratedRegex(SettingSubstitutionPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex GenerateSettingSubstitutionPatternRegex();
#endif
}
