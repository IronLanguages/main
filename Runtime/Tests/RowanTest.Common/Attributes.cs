/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace RowanTest.Common {

    /// <summary>
    /// Different reasons for a test being disabled.
    /// </summary>
    [Flags]
    public enum DisabledReason {
        /// <summary>
        /// General failures that affect all OSes, CLR releases, etc
        /// </summary>
        GeneralFailure = 0x0001,

        /// <summary>
        /// Test case is not working because it needs to be completed
        /// </summary>
        TODO = 0x0002,

        //... = 0x0004,

        /// <summary>
        /// Fails under 64-bit CLR only
        /// </summary>
        SixtyFourBitCLR = 0x0008,

        /// <summary>
        /// Fails under Silverlight only
        /// </summary>
        Silverlight = 0x0010,

        /// <summary>
        /// Fails under .NET 3.5 only
        /// </summary>
        Orcas = 0x0020,

        /// <summary>
        /// Test passes, but it runs too slowly
        /// </summary>
        SlowPerf = 0x0040,
    }

    /// <summary>
    /// States that a test should not be executed for various reasons.
    /// It is intended to be used in the following manner:
    /// 
    /// public class SomeTestClass : BaseTest {
    ///     [DisabledAttribute(DisabledReason.GeneralFailure, "CodePlex 1234")]
    ///     public void TestNotExecuted {
    ///     }
    /// 
    ///     [DisabledAttribute(DisabledReason.SixtyFourBitCLR, "CodePlex 5678")]
    ///     public void TestMightBeExecuted {
    ///     }
    /// 
    /// }
    /// </summary>
    public class DisabledAttribute : Attribute {

        private DisabledReason _disabledReason;
        private string _description;

        /// <summary>
        /// Standard constructor.
        /// </summary>
        /// <param name="dr">Reason the test is being disabled</param>
        /// <param name="description">Additional info</param>
        public DisabledAttribute(DisabledReason dr, string description) {
            _disabledReason = dr;
            _description = description;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dr">Reason the test is being disabled</param>
        public DisabledAttribute(DisabledReason dr)
            : this(dr, null) {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public DisabledAttribute()
            : this(DisabledReason.GeneralFailure) {
        }

        /// <summary>
        /// Reason the test is disabled.
        /// </summary>
        public DisabledReason Reason {
            get { return _disabledReason; }
        }

        /// <summary>
        /// Additional info about the test being disabled. Typically this should
        /// be a bug report number.
        /// </summary>
        public string Description {
            get {
                if (_description == null) {
                    return "Unknown";
                } else {
                    return _description;
                }
            }
        }
    }

    /// <summary>
    /// USe this attribute on a Test* method to run only that method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RunAttribute : Attribute {
    }
}