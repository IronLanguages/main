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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Utils;

internal sealed class ReflectionCacheGenerator : Generator {
    private static readonly Type MethodCacheType = typeof(IronRuby.Compiler.Methods);
    private static readonly Type FieldCacheType = typeof(IronRuby.Compiler.Fields);

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
        var reflectedFields = new Dictionary<string, FieldInfo>();
        foreach (var type in typeof(RubyOps).Assembly.GetExportedTypes()) {
            if (type.IsDefined(typeof(ReflectionCachedAttribute), false)) {
                Console.WriteLine(type);
                ReflectMembers(reflectedMethods, reflectedFields, type);
            }
        }

        var methods = reflectedMethods.ToSortedList((m1, m2) => m1.Key.CompareTo(m2.Key));
        var fields = reflectedFields.ToSortedList((f1, f2) => f1.Key.CompareTo(f2.Key));
        if (_anyError) {
            Environment.ExitCode = 1;
            return;
        }

        using (TextWriter writer = new StreamWriter(File.Open(_outFile, FileMode.Create, FileAccess.Write))) {
            _output = new IndentedTextWriter(writer, "    ");
            _output.NewLine = "\r\n";

            WriteLicenseStatement(writer);

            _output.WriteLine("#if !CLR2");
            _output.WriteLine("using System.Linq.Expressions;");
            _output.WriteLine("#else");
            _output.WriteLine("using Microsoft.Scripting.Ast;");
            _output.WriteLine("#endif");
            _output.WriteLine();

            _output.WriteLine("using System.Reflection;");
            _output.WriteLine("using System.Diagnostics;");
            _output.WriteLine("using IronRuby.Builtins;");
            _output.WriteLine("using IronRuby.Runtime;");
            _output.WriteLine("using IronRuby.Runtime.Calls;");
            _output.WriteLine("using Microsoft.Scripting.Utils;");
            _output.WriteLine("using Microsoft.Scripting.Interpreter;");

            _output.WriteLine();
            _output.WriteLine("#pragma warning disable 618 // obsolete attribute");

            _output.WriteLine();
            _output.WriteLine("namespace {0} {{", MethodCacheType.Namespace);
            _output.Indent++;

            _output.WriteLine("using System;");
            _output.WriteLine("using Microsoft.Scripting.Utils;");
            _output.WriteLine();

            _output.WriteLine("public static partial class {0} {{", MethodCacheType.Name);
            _output.Indent++;

            GenerateMethods(methods);
            
            _output.WriteLine();

            GenerateStringFactoryOps("CreateRegex");
            GenerateStringFactoryOps("CreateMutableString");
            GenerateStringFactoryOps("CreateSymbol");

            GenerateOptimizedOps("Yield", BlockDispatcher.MaxBlockArity);
            GenerateOptimizedOps("YieldSplat", BlockDispatcher.MaxBlockArity);
            
            _output.Indent--;
            _output.WriteLine("}");

            _output.WriteLine("public static partial class {0} {{", FieldCacheType.Name);
            _output.Indent++;

            GenerateMembers(fields, "FieldInfo", "GetField");

            _output.WriteLine();

            _output.Indent--;
            _output.WriteLine("}");

            _output.Indent--;
            _output.WriteLine("}");

            Debug.Assert(_output.Indent == 0);
        }
    }

    private void ReflectMembers(Dictionary<string, MethodInfo>/*!*/ methods, Dictionary<string, FieldInfo>/*!*/ fields, Type/*!*/ type) {
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
            if (method.IsDefined(typeof(EmittedAttribute), false)) {
                ReflectMember(methods, method);
            }
        }

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
            if (property.IsDefined(typeof(EmittedAttribute), false)) {
                foreach (var method in new[] { property.GetGetMethod(), property.GetSetMethod() }) {
                    if (method != null && !method.IsDefined(typeof(EmittedAttribute), false)) {
                        ReflectMember(methods, method);
                    }
                }
            }
        }

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
            if (field.IsDefined(typeof(EmittedAttribute), false)) {
                ReflectMember(fields, field);
            }
        }
    }

    private void ReflectMember<TMemberInfo>(Dictionary<string, TMemberInfo>/*!*/ members, TMemberInfo/*!*/ member) 
        where TMemberInfo : MemberInfo {
        string name = member.Name;
        if (member.DeclaringType != typeof(RubyOps)) {
            name = member.DeclaringType.Name + "_" + name;
        }

        TMemberInfo existing;
        if (members.TryGetValue(name, out existing)) {
            switch (existing.MemberType) {
                case MemberTypes.Method:
                    Console.WriteLine("ERROR: Emitted methods should not have overloads: \n\t{0}\n\t{1}",
                         ReflectionUtils.FormatSignature(new StringBuilder(), (MethodInfo)(object)existing),
                         ReflectionUtils.FormatSignature(new StringBuilder(), (MethodInfo)(object)member));
                    break;

                case MemberTypes.Field:
                    Console.WriteLine("ERROR: Multiple fields of name {0}", name);
                    break;
            }
            _anyError = true;
        } else {
            members.Add(name, member);
        }
    }

    private void GenerateMethods(IList<KeyValuePair<string, MethodInfo>>/*!*/ methods) {
        foreach (var entry in methods) {
            var name = entry.Key;
            var method = entry.Value;
            var ps = method.GetParameters();

            if (!method.IsGenericMethod && method.IsStatic && CollectionUtils.TrueForAll(ps, (p) => !p.ParameterType.IsByRef) &&
                !((EmittedAttribute)method.GetCustomAttributes(typeof(EmittedAttribute), false)[0]).UseReflection) {

                Type[] types = ReflectionUtils.GetParameterTypes(ps);
                string delegateType;
                if (method.ReturnType == typeof(void)) {
                    delegateType = "Action";
                } else {
                    delegateType = "Func";
                    types = ArrayUtils.Append(types, method.ReturnType);
                }

                _output.WriteLine("public static MethodInfo/*!*/ {0} {{ get {{ return _{0} ?? (_{0} = CallInstruction.Cache{3}{4}({1}.{2})); }} }}",
                    name,
                    method.DeclaringType.Name,
                    method.Name,
                    delegateType,
                    ReflectionUtils.FormatTypeArgs(new StringBuilder(), types, Generator.TypeNameDispenser)
                 );

                _output.WriteLine("private static MethodInfo _{0};", name);
            } else {
                GenerateMember(name, method, "MethodInfo", "GetMethod");
            }
        }
    }

    private void GenerateMembers<TMemberInfo>(IList<KeyValuePair<string, TMemberInfo>>/*!*/ members, string/*!*/ info, string/*!*/ getter)
        where TMemberInfo : MemberInfo {

        foreach (var member in members) {
            GenerateMember(member.Key, member.Value, info, getter);
        }
    }

    private void GenerateMember<TMemberInfo>(string/*!*/ name, TMemberInfo/*!*/ member, string/*!*/ info, string/*!*/ getter) 
        where TMemberInfo : MemberInfo {

        _output.WriteLine("public static {3}/*!*/ {0} {{ get {{ return _{0} ?? (_{0} = {4}(typeof({1}), \"{2}\")); }} }}",
            name, member.DeclaringType.Name, member.Name, info, getter
        );
        _output.WriteLine("private static {1} _{0};", name, info);
    }

    private void GenerateStringFactoryOps(string/*!*/ baseName) {
        _output.WriteLine("public static MethodInfo/*!*/ {0}(string/*!*/ suffix) {{", baseName);
        _output.Indent++;

        _output.WriteLine("Debug.Assert(suffix.Length <= RubyOps.MakeStringParamCount);");

        _output.WriteLine("switch (suffix) {");
        _output.Indent++;

        foreach (string suffix in new[] { "N", "M", "LM", "ML", "MM" }) {
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
