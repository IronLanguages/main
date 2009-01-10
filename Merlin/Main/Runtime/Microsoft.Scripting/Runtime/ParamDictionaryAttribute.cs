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

namespace Microsoft.Scripting {
    /// <summary>
    /// This attribute is the dictionary equivalent of the System.ParamArrayAttribute.
    /// It is used to mark a parameter that can accept an arbitrary dictionary of
    /// name/value pairs for a method called with named arguments.  This parameter
    /// must be applied to a type that implements IDictionary(string, object) or
    /// IDictionary(SymbolId, object).
    /// 
    /// For eg. in this Python method,
    ///     def foo(**paramDict): print paramDict
    ///     foo(a=1, b=2)
    /// paramDict will be {"a":1, "b":2}
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class ParamDictionaryAttribute : Attribute {
        public ParamDictionaryAttribute() {
        }
    }
}
