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
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Metadata {
    public static class ReflectionExtensions {
#if !CCI
        public static MetadataTables GetMetadataTables(this Module module) {
            return module.ModuleHandle.GetMetadataTables();
        }
#endif

        public static IEnumerable<MethodInfo> GetVisibleExtensionMethods(this Module module) {
            var ea = typeof(ExtensionAttribute);
            if (module.Assembly.IsDefined(ea, false)) {
                foreach (Type type in module.GetTypes()) {
                    var tattrs = type.Attributes;
                    if (((tattrs & TypeAttributes.VisibilityMask) == TypeAttributes.Public ||
                        (tattrs & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPublic) &&
                        (tattrs & TypeAttributes.Abstract) != 0 &&
                        (tattrs & TypeAttributes.Sealed) != 0 &&
                        type.IsDefined(ea, false)) {

                        foreach (MethodInfo method in type.GetMethods()) {
                            if (method.IsPublic && method.IsStatic && method.IsDefined(ea, false)) {
                                yield return method;
                            }
                        }
                    }
                }
            }
        }
    }
}
