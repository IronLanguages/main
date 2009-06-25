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

        var reflectedMethods = new Dictionary<string, MethodInfo>();
        foreach (var type in typeof(RubyOps).Assembly.GetExportedTypes()) {
            if (type.IsDefined(typeof(ReflectionCachedAttribute), false)) {
                Console.WriteLine(type);
                ReflectMethods(reflectedMethods, type);
            }
        }

        var methods = reflectedMethods.Sort((m1, m2) => m1.Key.CompareTo(m2.Key));
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
            _output.WriteLine("using IronRuby.Builtins;");
            _output.WriteLine("using Microsoft.Scripting.Utils;");

            _output.WriteLine();
            _output.WriteLine("namespace {0} {{", MethodCacheType.Namespace);
            _output.Indent++;

            _output.WriteLine("public static partial class {0} {{", MethodCacheType.Name);
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

    private void ReflectMethods(Dictionary<string, MethodInfo>/*!*/ methods, Type/*!*/ type) {
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
            if (method.IsDefined(typeof(EmittedAttribute), false)) {
                ReflectMethod(methods, method);
            }
        }

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
            if (property.IsDefined(typeof(EmittedAttribute), false)) {
                foreach (var method in new[] { property.GetGetMethod(), property.GetSetMethod() }) {
                    if (method != null && !method.IsDefined(typeof(EmittedAttribute), false)) {
                        ReflectMethod(methods, method);
                    }
                }
            }
        }
    }

    private void ReflectMethod(Dictionary<string, MethodInfo>/*!*/ methods, MethodInfo/*!*/ method) {
        string name = method.Name;
        if (method.DeclaringType != typeof(RubyOps)) {
            name = method.DeclaringType.Name + "_" + name;
        }

        MethodInfo existingMethod;
        if (methods.TryGetValue(name, out existingMethod)) {
            Console.WriteLine("ERROR: Emitted methods should not have overloads: \n\t{0}\n\t{1}",
                ReflectionUtils.FormatSignature(new StringBuilder(), existingMethod),
                ReflectionUtils.FormatSignature(new StringBuilder(), method));
            _anyError = true;
        } else {
            methods.Add(name, method);
        }
    }

    private void GenerateOps(IList<KeyValuePair<string, MethodInfo>>/*!*/ methods) {
        foreach (var method in methods) {
            _output.WriteLine("public static MethodInfo/*!*/ {0} {{ get {{ return _{0} ?? (_{0} = GetMethod(typeof({1}), \"{2}\")); }} }}",
                method.Key, method.Value.DeclaringType.Name, method.Value.Name);
            _output.WriteLine("private static MethodInfo _{0};", method.Key);
        }
    }

    private void GenerateStringFactoryOps(string/*!*/ baseName) {
        _output.WriteLine("public static MethodInfo/*!*/ {0}(string/*!*/ suffix) {{", baseName);
        _output.Indent++;

        _output.WriteLine("Debug.Assert(suffix.Length <= RubyOps.MakeStringParamCount);");

        _output.WriteLine("switch (suffix) {");
        _output.Indent++;

        foreach (string suffix in new[] { "N", "L", "M", "LM", "ML", "MM" }) {
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
