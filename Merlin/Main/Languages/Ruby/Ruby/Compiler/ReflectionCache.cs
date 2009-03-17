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
using System.Collections;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime;
using IronRuby.Builtins;
using Microsoft.Scripting.Utils;

using AstUtils = Microsoft.Scripting.Ast.Utils;
using AstFactory = IronRuby.Compiler.Ast.AstFactory;
using MSA = System.Linq.Expressions;
using IronRuby.Runtime.Calls;
using System.Collections.ObjectModel;
using Microsoft.Scripting.Runtime;
using System.Runtime.CompilerServices;

namespace IronRuby.Compiler {
    internal static class Fields {
        private static FieldInfo _RubyOps_DefaultArgumentField, _RubyOps_MethodNotFound, _StrongBox_Of_Int_Value, _RubyClass_Version;
        public static FieldInfo RubyOps_DefaultArgumentField { get { return _RubyOps_DefaultArgumentField ?? (_RubyOps_DefaultArgumentField = GetField(typeof(RubyOps), "DefaultArgument")); } }
        public static FieldInfo RubyOps_MethodNotFound { get { return _RubyOps_MethodNotFound ?? (_RubyOps_MethodNotFound = GetField(typeof(RubyOps), "MethodNotFound")); } }
        public static FieldInfo StrongBox_Of_Int_Value { get { return _StrongBox_Of_Int_Value ?? (_StrongBox_Of_Int_Value = GetField(typeof(StrongBox<int>), "Value")); } }
        public static FieldInfo RubyClass_Version { get { return _RubyClass_Version ?? (_RubyClass_Version = GetField(typeof(RubyClass), "Version")); } }

        internal static FieldInfo/*!*/ GetField(Type/*!*/ type, string/*!*/ name) {
            var field = type.GetField(name);
            Debug.Assert(field != null, type.Name + "::" + name);
            return field;
        }
    }
    
    internal static partial class Methods {
        private static ConstructorInfo _RubyCallSignatureCtor;
        private static MethodInfo _Stopwatch_GetTimestamp, _IEnumerable_Of_Object_GetEnumerator, _IEnumerator_MoveNext,
            _IEnumerator_get_Current, _RubyStruct_GetValue, _RubyStruct_SetValue;

        public static ConstructorInfo RubyCallSignatureCtor { get { return _RubyCallSignatureCtor ?? (_RubyCallSignatureCtor = GetConstructor(typeof(RubyCallSignature), typeof(uint))); } }

        public static MethodInfo Stopwatch_GetTimestamp { get { return _Stopwatch_GetTimestamp ?? (_Stopwatch_GetTimestamp = GetMethod(typeof(Stopwatch), "GetTimestamp")); } }
        public static MethodInfo RubyStruct_GetValue { get { return _RubyStruct_GetValue ?? (_RubyStruct_GetValue = GetMethod(typeof(RubyStruct), "GetValue", BindingFlags.Instance, typeof(int))); } }
        public static MethodInfo RubyStruct_SetValue { get { return _RubyStruct_SetValue ?? (_RubyStruct_SetValue = GetMethod(typeof(RubyStruct), "SetValue", BindingFlags.Instance, typeof(int), typeof(object))); } }
        public static MethodInfo IEnumerable_Of_Object_GetEnumerator { get { return _IEnumerable_Of_Object_GetEnumerator ?? (_IEnumerable_Of_Object_GetEnumerator = GetMethod(typeof(IEnumerable<object>), "GetEnumerator", BindingFlags.Instance, Type.EmptyTypes)); } }
        public static MethodInfo IEnumerator_get_Current { get { return _IEnumerator_get_Current ?? (_IEnumerator_get_Current = GetMethod(typeof(IEnumerator), "get_Current", BindingFlags.Instance, Type.EmptyTypes)); } }
        public static MethodInfo IEnumerator_MoveNext { get { return _IEnumerator_MoveNext ?? (_IEnumerator_MoveNext = GetMethod(typeof(IEnumerator), "MoveNext", BindingFlags.Instance, Type.EmptyTypes)); } }

        internal static ConstructorInfo/*!*/ GetConstructor(Type/*!*/ type, params Type/*!*/[]/*!*/ signature) {
            var ctor = type.GetConstructor(signature);
            Debug.Assert(ctor != null, type.Name + "::.ctor");
            return ctor;
        }

        internal static MethodInfo/*!*/ GetMethod(Type/*!*/ type, string/*!*/ name) {
            var method = type.GetMethod(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            Debug.Assert(method != null, type.Name + "::" + name);
            return method;
        }

        internal static MethodInfo/*!*/ GetMethod(Type/*!*/ type, string/*!*/ name, params Type/*!*/[]/*!*/ signature) {
            return GetMethod(type, name, BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, signature);
        }

        internal static MethodInfo/*!*/ GetMethod(Type/*!*/ type, string/*!*/ name, BindingFlags flags, params Type/*!*/[]/*!*/ signature) {
            var method = type.GetMethod(name, flags | BindingFlags.Public | BindingFlags.DeclaredOnly, null, signature, null);

            Debug.Assert(method != null, type.Name + "::" + name);
            return method;
        }

        public static MSA.Expression/*!*/ MakeArrayOpCall(List<MSA.Expression>/*!*/ args) {
            Assert.NotNull(args);

            switch (args.Count) {
                case 0: return Methods.MakeArray0.OpCall();
                case 1: return Methods.MakeArray1.OpCall(AstFactory.Box(args[0]));
                case 2: return Methods.MakeArray2.OpCall(AstFactory.Box(args[0]), AstFactory.Box(args[1]));
                case 3: return Methods.MakeArray3.OpCall(AstFactory.Box(args[0]), AstFactory.Box(args[1]), AstFactory.Box(args[2]));
                case 4: return Methods.MakeArray4.OpCall(AstFactory.Box(args[0]), AstFactory.Box(args[1]), AstFactory.Box(args[2]), AstFactory.Box(args[3]));
                case 5: return Methods.MakeArray5.OpCall(AstFactory.Box(args[0]), AstFactory.Box(args[1]), AstFactory.Box(args[2]), AstFactory.Box(args[3]), AstFactory.Box(args[4]));

                default:
                    Debug.Assert(args.Count > Runtime.RubyOps.OptimizedOpCallParamCount);
                    return Methods.MakeArrayN.OpCall(AstUtils.NewArrayHelper(typeof(object), args));
            }
        }

        public static MethodInfo/*!*/ Yield(int argumentCount, bool hasSplattedArgument, bool hasRhsArgument, out bool hasArgumentArray) {
            if (hasRhsArgument) {
                if (hasSplattedArgument) {
                    hasArgumentArray = true;
                    return Methods.YieldSplatNRhs;
                } else {
                    argumentCount++;
                }
            }

            hasArgumentArray = argumentCount > BlockDispatcher.MaxBlockArity;
            return hasSplattedArgument ? Methods.YieldSplat(argumentCount) : Methods.Yield(argumentCount);
        }
    }

    internal static class MethodInfoExtensions {
        public static MSA.Expression/*!*/ OpCall(this MethodInfo/*!*/ method) {
            Assert.NotNull(method);
            return MSA.Expression.Call(null, method);
        }

        public static MSA.Expression/*!*/ OpCall(this MethodInfo/*!*/ method, MSA.Expression/*!*/ arg1) {
            Assert.NotNull(method);
            return MSA.Expression.Call(method, arg1);
        }

        public static MSA.Expression/*!*/ OpCall(this MethodInfo/*!*/ method, MSA.Expression/*!*/ arg1, MSA.Expression/*!*/ arg2) {
            Assert.NotNull(method);
            return MSA.Expression.Call(null, method, arg1, arg2);
        }

        public static MSA.Expression/*!*/ OpCall(this MethodInfo/*!*/ method, MSA.Expression/*!*/ arg1, MSA.Expression/*!*/ arg2, MSA.Expression/*!*/ arg3) {
            Assert.NotNull(method);
            return MSA.Expression.Call(null, method, arg1, arg2, arg3);
        }

        public static MSA.Expression/*!*/ OpCall(this MethodInfo/*!*/ method, params MSA.Expression[]/*!*/ args) {
            Assert.NotNull(method, args);
            return MSA.Expression.Call(null, method, new ReadOnlyCollection<MSA.Expression>(args));
        }

        public static MSA.Expression/*!*/ OpCall(this MethodInfo/*!*/ method, List<MSA.Expression>/*!*/ args) {
            Assert.NotNull(method, args);
            return MSA.Expression.Call(null, method, new ReadOnlyCollection<MSA.Expression>(args));
        }
    }
}
