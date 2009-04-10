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

using System;
using System.Collections.Generic;

namespace Microsoft.Scripting.Actions.Calls {
    /// <summary>
    /// Represents information about a failure to convert an argument from one
    /// type to another.
    /// </summary>
    public sealed class ConversionResult {
        private readonly Type _fromType;
        private readonly Type _toType;
        private readonly bool _failed;

        public ConversionResult(Type fromType, Type toType, bool failed) {
            _fromType = fromType;
            _toType = toType;
            _failed = failed;
        }

        public Type From {
            get {
                return _fromType;
            }
        }

        public Type To {
            get {
                return _toType;
            }
        }

        public bool Failed {
            get {
                return _failed;
            }
        }

        internal static void ReplaceLastFailure(IList<ConversionResult> failures, bool isFailure) {
            ConversionResult failure = failures[failures.Count - 1];
            failures.RemoveAt(failures.Count - 1);
            failures.Add(new ConversionResult(failure.From, failure.To, isFailure));
        }
    }
}
