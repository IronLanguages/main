/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
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
using System.Dynamic;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions.Calls {
    /// <summary>
    /// Represents information about a failure to convert an argument from one
    /// type to another.
    /// </summary>
    public sealed class ConversionResult {
        private readonly object _arg;
        private readonly Type _argType;
        private readonly Type _toType;
        private readonly bool _failed;

        internal ConversionResult(object arg, Type argType, Type toType, bool failed) {
            _arg = arg;
            _argType = argType;
            _toType = toType;
            _failed = failed;
        }

        /// <summary>
        /// Value of the argument or null if it is not available.
        /// </summary>
        public object Arg {
            get { return _arg; }
        }

        /// <summary>
        /// Argument actual type or its limit type if the value not known.
        /// DynamicNull if the argument value is null.
        /// </summary>
        public Type ArgType {
            get { return _argType; }
        }

        public Type To {
            get { return _toType; }
        }

        public bool Failed {
            get { return _failed; }
        }

        internal static void ReplaceLastFailure(IList<ConversionResult> failures, bool isFailure) {
            ConversionResult failure = failures[failures.Count - 1];
            failures.RemoveAt(failures.Count - 1);
            failures.Add(new ConversionResult(failure.Arg, failure.ArgType, failure.To, isFailure));
        }

        public string GetArgumentTypeName(ActionBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return (_arg != null) ? binder.GetObjectTypeName(_arg) : binder.GetTypeName(_argType);
        }
    }
}
