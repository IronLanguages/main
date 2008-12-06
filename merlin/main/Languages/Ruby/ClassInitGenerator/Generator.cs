/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Dynamic;
using System.Dynamic.Utils;
using System.Text;
using IronRuby;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;

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
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
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
}
