/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace System.Runtime.CompilerServices {
    /// <summary>
    /// Class responsible for binding dynamic operations on the dynamic site.
    /// </summary>
    public abstract class CallSiteBinder {
        protected CallSiteBinder() {
        }

        /// <summary>
        /// Key used for the DLR caching
        /// </summary>
        public abstract object CacheIdentity { get; }

        /// <summary>
        /// The bind call to produce the binding.
        /// </summary>
        /// <param name="args">Array of arguments to the call</param>
        /// <param name="parameters">Array of ParameterExpressions that represent to parameters of the call site</param>
        /// <param name="returnLabel">LabelTarget used to return the result of the call site</param>
        /// <returns>
        /// An Expression that performs tests on the arguments, and
        /// returns a result if the test is valid. If the tests fail, Bind
        /// will be called again to produce a new Expression for the new
        /// argument types
        /// </returns>
        public abstract Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel);
    }
}
