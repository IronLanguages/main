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

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Dynamic;
using System.Text;
using IronRuby;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;

internal class InitGenerator : Generator {
    private Assembly/*!*/ _assembly;
    private string _outFile;
    // namespace -> library
    private readonly Dictionary<string, LibraryDef>/*!*/ _libraries = new Dictionary<string, LibraryDef>();

    private InitGenerator() {
    }

    public static InitGenerator Create(string[]/*!*/ args) {
        var gen = new InitGenerator();

        for (int i = 0; i < args.Length; i++) {
            KeyValuePair<string, string> arg = ToNameValue(args[i]);

            switch (arg.Key) {
                case "out":
                    gen._outFile = arg.Value;
                    break;

                case "libraries":
                    foreach (string libararyNamespace in arg.Value.Split(';', ',')) {
                        gen._libraries[libararyNamespace] = new LibraryDef(libararyNamespace);
                    }
                    break;

                case "":
                    try {
                        gen._assembly = Assembly.LoadFrom(arg.Value);
                    } catch (Exception e) {
                        Console.Error.WriteLine(e.Message);
                        return null;
                    }
                    break;

                default:
                    Console.Error.WriteLine("Unknown option: {0}", arg.Key);
                    return null;
            }
        }

        if (gen._outFile == null) {
            Console.Error.WriteLine("Output file not specified");
            return null;
        }

        return gen;
    }

    public void Generate() {
        Type[] allTypes = _assembly.GetTypes();
        bool anyErrors = false;
        foreach (LibraryDef library in _libraries.Values) {
            library.ReflectTypes(allTypes);
            anyErrors |= library.AnyErrors;
        }

        if (anyErrors) {
            Console.Error.WriteLine("Failed.");
            return;
        }

        using (TextWriter writer = new StreamWriter(File.Open(_outFile, FileMode.Create, FileAccess.Write))) {
            IndentedTextWriter output = new IndentedTextWriter(writer, "    ");
            output.NewLine = "\r\n";

            WriteLicenseStatement(writer);
            output.WriteLine("#pragma warning disable 169 // mcs: unused private method");

            foreach (LibraryDef library in _libraries.Values) {
                output.WriteLine("[assembly: {2}(typeof({0}.{1}))]", library._namespace, library._initializerName, LibraryDef.TypeRubyLibraryAttribute);
            }

            output.WriteLine();

            foreach (LibraryDef library in _libraries.Values) {
                Console.WriteLine("Library {0}", library._namespace);
                library.GenerateCode(output);
            }
        }
    }
}
