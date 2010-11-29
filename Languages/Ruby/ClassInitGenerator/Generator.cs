/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Scripting.Utils;

internal class Generator {
    static void Main(string[]/*!*/ args) {
        var list = new List<string>(args);
        if (list.IndexOf("/refcache") >= 0 || list.IndexOf("-refcache") >= 0) {
            ReflectionCacheGenerator.Create(args).Generate();
        } else {
            var generator = InitGenerator.Create(args);
            if (generator == null) {
                Environment.ExitCode = -1;
                return;
            }

            generator.Generate();
        }
    }

    internal void WriteLicenseStatement(TextWriter/*!*/ writer) {
        writer.Write(
@"/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

");
    }

    internal static KeyValuePair<string/*!*/, string/*!*/> ToNameValue(string/*!*/ arg) {
        if (arg.StartsWith("/") || arg.StartsWith("-")) {
            int colon = arg.IndexOf(':');
            if (colon >= 0) {
                return new KeyValuePair<string, string>(arg.Substring(1, colon - 1), arg.Substring(colon + 1));
            } else {
                return new KeyValuePair<string, string>(arg.Substring(1), String.Empty);
            }
        } else {
            return new KeyValuePair<string, string>(String.Empty, arg);
        }
    }

    internal static string/*!*/ TypeNameDispenser(Type/*!*/ type) {
        return
            type == typeof(MSA.Expression) ||
            type.FullName.StartsWith(typeof(Action).Namespace + ".Action") ||
            type.FullName.StartsWith(typeof(Action<>).Namespace + ".Action`") ||
            type.FullName.StartsWith(typeof(Func<>).Namespace + ".Func`") ||
            type.FullName == "System.Runtime.InteropServices.DefaultParameterValueAttribute" ?
            type.Name : type.FullName;
    }
}
