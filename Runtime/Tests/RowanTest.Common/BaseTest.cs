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
    /// The base class for test classes expected to be run automatically by 
    /// RowanTest.Common.Runner:
    /// * a test class must be derived from BaseTest in another assembly 
    ///   before it will be auto instantiated by the the Runner class
    /// * only methods following the "Test*()" naming convention will be invoked
    ///   by Runner
    /// * there are quite a few assertion helper methods available for use
    ///   in BaseTest
    /// </summary>
    public class BaseTest {

        /// <summary>
        /// Delegate definition to be used in conjunction with AssertError method.
        /// </summary>
        public delegate void Function();

        /// <summary>
        /// Throws an exception with a given message.
        /// </summary>
        /// <param name="excMsg">A message to be embedded within the Exception
        /// thrown by this method.</param>
        public static void Fail(string excMsg) {
            throw new System.Exception(excMsg);
        }

        /// <summary>
        /// Ensures a boolean value is True.  If not, Fail is called.
        /// </summary>
        /// <param name="val">Value to check</param>
        /// <param name="msg">An error message to display if val==False</param>
        public static void Assert(bool val, string msg) {
            if (!val) Fail(msg);
        }

        /// <summary>
        /// Ensure a boolean value is True.  If not, Fail is called.
        /// </summary>
        /// <param name="val">Value to check</param>
        public static void Assert(bool val) {
            Assert(val, "Failed assertion!");
        }

        /// <summary>
        /// Ensure two objects are equivalent.  If not, Fail is called.
        /// </summary>
        /// <param name="compare1">First object to compare</param>
        /// <param name="compare2">Second object to compare</param>
        /// <param name="msg">An error message to display if compare1!=compare2</param>
        public static void AreEqual(Object compare1, Object compare2, string msg) {
            if (compare1 == null && compare2 == null) return;
            
            Assert(compare1!=null && compare1.Equals(compare2), msg);
        }
        
        /// <summary>
        /// Ensure two objects are equivalent.  If not, Fail is called.
        /// </summary>
        /// <param name="compare1">First object to compare</param>
        /// <param name="compare2">Second object to compare</param>
        public static void AreEqual(Object compare1, Object compare2) {
            AreEqual(compare1, compare2, compare1 + " does not equal " + compare2 + "!");
        }

        /// <summary>
        /// Used to ensure a delegate throws an exception of type T.
        /// </summary>
        /// <typeparam name="T">Type of the expected exception</typeparam>
        /// <param name="f">A delegate which should throw a T exception</param>
        /// <param name="msg">An error message to display if the exception is not thrown</param>
        public static void AssertError<T>(Function f, string msg) where T : Exception {
            try {
                f();
                Fail(msg);
            } catch (T) {
                return;
            }
        }

        /// <summary>
        /// Used to ensure a delegate throws an exception of type T.
        /// </summary>
        /// <typeparam name="T">Type of the expected exception</typeparam>
        /// <param name="f">A delegate which should throw a T exception</param>
        public static void AssertError<T>(Function f) where T : Exception {
            AssertError<T>(f, "Expected exception '" + typeof(T) + "'.");
        }
    }

}
