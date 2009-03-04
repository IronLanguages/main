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
using System.Text;

namespace IronPython.Runtime.Types {
    public class CachedNewTypeInfo {
        public readonly Type Type;
        public readonly Dictionary<string, string[]> SpecialNames;
        public readonly Type[] InterfaceTypes;

        public CachedNewTypeInfo(Type type, Dictionary<string, string[]> specialNames, Type[] interfaceTypes) {
            Type = type;
            SpecialNames = specialNames;
            InterfaceTypes = interfaceTypes ?? Type.EmptyTypes;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PythonCachedTypeInfoAttribute : Attribute {
    }
}
