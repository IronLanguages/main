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
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// An attribute that is applied to saved ScriptCode's to be used to re-create the ScriptCode
    /// from disk.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class DlrCachedCodeAttribute : Attribute {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class CachedOptimizedCodeAttribute : Attribute {
        private readonly string[] _names;

        // C# requires a constructor with CLS compliant types:
        public CachedOptimizedCodeAttribute() {
            _names = ArrayUtils.EmptyStrings;
        }

        public CachedOptimizedCodeAttribute(string[] names) {
            ContractUtils.RequiresNotNull(names, "names");
            _names = names;
        }

        /// <summary>
        /// Gets names stored in optimized scope. 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] Names {
            get {
                return _names;
            }
        }
    }
}
