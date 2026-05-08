//******************************************************************************************************
//  Tests.cs - Gbtc
//
//  Copyright © 2019, Grid Protection Alliance.  All Rights Reserved.
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
//  11/04/2019 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gemstone.Configuration.UnitTests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void FunctionalityTests()
        {
            Assert.IsTrue(true);
        }
    }

    #region [ IIgnorableParameter Hierarchy Validation Tests ]

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    internal sealed class TestParameterAttribute : Attribute, ConnectionStringParser<TestParameterAttribute>.IIgnorableParameter
    {
        public bool IgnoreWhenParsing { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    internal sealed class PlainParameterAttribute : Attribute
    {
    }

    internal class BaseSettings
    {
        [TestParameter(IgnoreWhenParsing = true)]
        public virtual string ManuallyHandled { get; set; } = string.Empty;

        [TestParameter]
        public virtual string AutoParsed { get; set; } = string.Empty;
    }

    // BUG case: override re-applies attribute but forgets IgnoreWhenParsing = true.
    internal class BuggyDerivedSettings : BaseSettings
    {
        [TestParameter]
        public override string ManuallyHandled { get; set; } = string.Empty;
    }

    // GOOD case: override re-applies attribute and keeps IgnoreWhenParsing = true.
    internal class GoodDerivedSettings : BaseSettings
    {
        [TestParameter(IgnoreWhenParsing = true)]
        public override string ManuallyHandled { get; set; } = string.Empty;
    }

    // GOOD case: override does not re-apply attribute; inherited attribute is used.
    internal class InheritingDerivedSettings : BaseSettings
    {
        public override string ManuallyHandled { get; set; } = string.Empty;
    }

    // Attribute without IIgnorableParameter — validation should be a no-op.
    internal class PlainBaseSettings
    {
        [PlainParameter]
        public virtual string Value { get; set; } = string.Empty;
    }

    internal class PlainDerivedSettings : PlainBaseSettings
    {
        [PlainParameter]
        public override string Value { get; set; } = string.Empty;
    }

    [TestClass]
    public class IgnorableParameterHierarchyTests
    {
        [TestMethod]
        public void DerivedThatForgetsIgnoreFlag_Throws()
        {
            ConnectionStringParser<TestParameterAttribute> parser = new();

            InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
                () => parser.ParseConnectionString("ManuallyHandled=x;AutoParsed=y", new BuggyDerivedSettings()));

            StringAssert.Contains(ex.Message, nameof(BuggyDerivedSettings));
            StringAssert.Contains(ex.Message, nameof(BaseSettings));
            StringAssert.Contains(ex.Message, nameof(TestParameterAttribute.IgnoreWhenParsing));
        }

        [TestMethod]
        public void DerivedThatKeepsIgnoreFlag_Succeeds()
        {
            ConnectionStringParser<TestParameterAttribute> parser = new();
            GoodDerivedSettings settings = new();

            parser.ParseConnectionString("AutoParsed=y", settings);

            Assert.AreEqual("y", settings.AutoParsed);
            Assert.AreEqual(string.Empty, settings.ManuallyHandled);
        }

        [TestMethod]
        public void DerivedThatInheritsAttribute_Succeeds()
        {
            ConnectionStringParser<TestParameterAttribute> parser = new();
            InheritingDerivedSettings settings = new();

            parser.ParseConnectionString("AutoParsed=y", settings);

            Assert.AreEqual("y", settings.AutoParsed);
            Assert.AreEqual(string.Empty, settings.ManuallyHandled);
        }

        [TestMethod]
        public void AttributeWithoutIgnorableInterface_NoValidation()
        {
            ConnectionStringParser<PlainParameterAttribute> parser = new();
            PlainDerivedSettings settings = new();

            parser.ParseConnectionString("Value=z", settings);

            Assert.AreEqual("z", settings.Value);
        }
    }

    #endregion
}
