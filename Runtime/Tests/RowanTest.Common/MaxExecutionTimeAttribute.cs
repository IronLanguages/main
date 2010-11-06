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
    /// Defines the maximum amount of time in milliseconds a "Test*()" method is 
    /// allowed to execute before failing.  This attribute does not actually prevent
    /// the method from running longer than the maximum timeout; instead it fails after
    /// the test method has returned control.  It is intended to be used in the 
    /// following manner:
    /// 
    /// public class SomeTestClass : BaseTest {
    ///     [MaxExecutionTimeAttribute(5)]
    ///     public void TestThisWillFail {
    ///         System.Threading.Thread.Sleep(6);
    ///     }
    ///     [MaxExecutionTimeAttribute(5)]
    ///     public void TestThisWillPass {
    ///         System.Threading.Thread.Sleep(4);
    ///     }
    /// }
    /// </summary>
    public class MaxExecutionTimeAttribute : System.Attribute {

        private long _maxExecutionTime;

        /// <summary>
        /// Standard constructor.
        /// </summary>
        /// <param name="max">A milliseconds duration</param>
        public MaxExecutionTimeAttribute(long max) {
            _maxExecutionTime = max;
        }

        /// <summary>
        /// Maximum execution time for an individual method in milliseconds.
        /// </summary>
        public long Max {
            get { return _maxExecutionTime; }
        }
    }
}