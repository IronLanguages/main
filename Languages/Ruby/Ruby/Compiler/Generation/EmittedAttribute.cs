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
using System.Diagnostics;

namespace IronRuby.Compiler.Generation {

    /// <summary>
    /// Marks methods and properties that are emitted into the generated code.
    /// </summary>
    [Conditional("DEBUG")]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    public sealed class EmittedAttribute : Attribute {
        public bool UseReflection { get; set; }
    }

    /// <summary>
    /// Marks types whose [Emitted] members should be stored in Reflection Cache.
    /// </summary>
    [Conditional("DEBUG")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class ReflectionCachedAttribute : Attribute {
    }
}
