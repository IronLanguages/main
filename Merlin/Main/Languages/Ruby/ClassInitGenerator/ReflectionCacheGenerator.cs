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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using IronRuby.Runtime;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using IronRuby.Compiler.Generation;
using Microsoft.Scripting.Utils;
using IronRuby.Compiler;
using IronRuby.Runtime.Calls;

internal sealed class ReflectionCacheGenerator : Generator {
    private static readonly Type MethodCacheType = typeof(IronRuby.Compiler.Methods);

    private string _outFile;
    private IndentedTextWriter _output;
    private bool _anyError;

    private ReflectionCacheGenerator() {
    }

    public static ReflectionCacheGenerator Create(string[]/*!*/ args) {
        var gen = new ReflectionCacheGenerator();

        for (int i = 0; i < args.Length; i++) {
            KeyValuePair<string, string> arg = ToNameValue(args[i]);

            switch (arg.Key) {
                case "refcache":
                    // skip
                    break;

                case "out":
                    gen._outFile = arg.Value;
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
        _anyError = false;

        var unsortedMethods = ReflectMethods(typeof(RubyOps)).Values;
        List<MethodInfo> methods = new List<MethodInfo>();
        methods.AddRange(unsortedMethods);
        methods.Sort((m1, m2) => m1.Name.CompareTo(m2.Name));

        if (_anyError) {
            Environment.ExitCode = 1;
            return;
        }

        using (TextWriter writer = new StreamWriter(File.Open(_outFile, FileMode.Create, FileAccess.Write))) {
            _output = new IndentedTextWriter(writer, "    ");
            _output.NewLine = "\r\n";

            WriteLicenseStatement(writer);

            _output.WriteLine("using System.Reflection;");
            _output.WriteLine("using System.Diagnostics;");
            _output.WriteLine("using IronRuby.Runtime;");
            _output.WriteLine("using Microsoft.Scripting.Utils;");

            _output.WriteLine();
            _output.WriteLine("namespace {0} {{", MethodCacheType.Namespace);
            _output.Indent++;

            _output.WriteLine("internal static partial class {0} {{", MethodCacheType.Name);
            _output.Indent++;

            GenerateOps(methods);
            
            _output.WriteLine();

            GenerateStringFactoryOps("CreateRegex");
            GenerateStringFactoryOps("CreateMutableString");
            GenerateStringFactoryOps("CreateSymbol");

            GenerateOptimizedOps("Yield", BlockDispatcher.MaxBlockArity);
            GenerateOptimizedOps("YieldSplat", BlockDispatcher.MaxBlockArity);
            
            _output.Indent--;
            _output.WriteLine("}");

            _output.Indent--;
            _output.WriteLine("}");

            Debug.Assert(_output.Indent == 0);
        }
    }

    private Dictionary<string, MethodInfo>/*!*/ ReflectMethods(Type/*!*/ type) {
        var result = new Dictionary<string, MethodInfo>();
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        foreach (var method in methods) {
            if (method.IsDefined(typeof(EmittedAttribute), false)) {
                MethodInfo existingMethod;
                if (result.TryGetValue(method.Name, out existingMethod)) {
                    Console.WriteLine("ERROR: Emitted methods should not have overloads: \n\t{0}\n\t{1}",
                        ReflectionUtils.FormatSignature(new StringBuilder(), existingMethod),
                        ReflectionUtils.FormatSignature(new StringBuilder(), method));
                    _anyError = true;
                } else {
                    result.Add(method.Name, method);
                }
            }
        }
        return result;
    }

    private void GenerateOps(ICollection<MethodInfo>/*!*/ methods) {
        _output.Write("private static MethodInfo ");
        int count = 0;
        foreach (var method in methods) {
            if (count > 0) {
                _output.Write(", ");
            }

            count++;

            if (count % 20 == 0) {
                _output.WriteLine();
            }

            _output.Write("_");
            _output.Write(method.Name);
        }
        _output.WriteLine(";");

        _output.WriteLine();

        foreach (var method in methods) {
            _output.WriteLine("public static MethodInfo/*!*/ {0} {{ get {{ return _{0} ?? (_{0} = GetMethod(typeof(RubyOps), \"{0}\")); }} }}",
                method.Name);
        }
    }

    private void GenerateStringFactoryOps(string/*!*/ baseName) {
        _output.WriteLine("public static MethodInfo/*!*/ {0}(string/*!*/ suffix) {{", baseName);
        _output.Indent++;

        _output.WriteLine("Debug.Assert(suffix.Length <= RubyOps.MakeStringParamCount);");

        _output.WriteLine("switch (suffix) {");
        _output.Indent++;

        foreach (string suffix in new[] { "N", "B", "E", "U", "M", "BM", "UM", "EM", "MB", "MU", "ME", "MM" }) {
            _output.WriteLine("case \"{1}\": return {0}{1};", baseName, suffix);
        }

        _output.Indent--;
        _output.WriteLine("}");

        _output.WriteLine("throw Assert.Unreachable;");

        _output.Indent--;
        _output.WriteLine("}");

        _output.WriteLine();
    }

    private void GenerateOptimizedOps(string/*!*/ baseName, int maxParamCount) {
        _output.WriteLine("public static MethodInfo/*!*/ {0}(int parameterCount) {{", baseName);
        _output.Indent++;

        _output.WriteLine("switch (parameterCount) {");
        _output.Indent++;

        for (int i = 0; i <= maxParamCount; i++) {
            _output.WriteLine("case {1}: return {0}{1};", baseName, i);
        }

        _output.Indent--;
        _output.WriteLine("}");

        _output.WriteLine("return {0}N;", baseName);

        _output.Indent--;
        _output.WriteLine("}");

        _output.WriteLine();
    }
}
