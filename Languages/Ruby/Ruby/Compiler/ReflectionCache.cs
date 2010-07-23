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
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler {
    using MSAst = Microsoft.Scripting.Ast;
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    using AstExpressions = ReadOnlyCollectionBuilder<MSA.Expression>;
    using Microsoft.Scripting.Interpreter;

    public static partial class Fields {
        private static FieldInfo _StrongBox_Value;
        
        public static FieldInfo StrongBox_Value { get { return _StrongBox_Value ?? (_StrongBox_Value = GetField(typeof(StrongBox<object>), "Value")); } }
        
        internal static FieldInfo/*!*/ GetField(Type/*!*/ type, string/*!*/ name) {
            var field = type.GetField(name);
            Debug.Assert(field != null, type.Name + "::" + name);
            return field;
        }
    }
    
    public static partial class Methods {
        private static ConstructorInfo _RubyCallSignatureCtor;
        private static MethodInfo _Stopwatch_GetTimestamp, _WeakReference_get_Target, _IList_get_Item;

        public static ConstructorInfo RubyCallSignatureCtor { get { return _RubyCallSignatureCtor ?? (_RubyCallSignatureCtor = GetConstructor(typeof(RubyCallSignature), typeof(uint))); } }
        
        public static MethodInfo Stopwatch_GetTimestamp { get { return _Stopwatch_GetTimestamp ?? (_Stopwatch_GetTimestamp = GetMethod(typeof(Stopwatch), "GetTimestamp")); } }
        public static MethodInfo IList_get_Item { get { return _IList_get_Item ?? (_IList_get_Item = GetMethod(typeof(IList), "get_Item")); } }
        public static MethodInfo WeakReference_get_Target { get { return _WeakReference_get_Target ?? (_WeakReference_get_Target = GetMethod(typeof(WeakReference), "get_Target", BindingFlags.Instance, Type.EmptyTypes)); } }

        internal static ConstructorInfo/*!*/ GetConstructor(Type/*!*/ type, params Type/*!*/[]/*!*/ signature) {
            var ctor = type.GetConstructor(signature);
            Debug.Assert(ctor != null, type.Name + "::.ctor");
            return ctor;
        }

        internal static MethodInfo/*!*/ GetMethod(Type/*!*/ type, string/*!*/ name) {
            var method = type.GetMethod(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            Debug.Assert(method != null, type.Name + "::" + name);
            return method;
        }

        internal static MethodInfo/*!*/ GetMethod(Type/*!*/ type, string/*!*/ name, params Type/*!*/[]/*!*/ signature) {
            return GetMethod(type, name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly, signature);
        }

        internal static MethodInfo/*!*/ GetMethod(Type/*!*/ type, string/*!*/ name, BindingFlags flags, params Type/*!*/[]/*!*/ signature) {
            var method = type.GetMethod(name, flags | BindingFlags.Public | BindingFlags.DeclaredOnly, null, signature, null);

            Debug.Assert(method != null, type.Name + "::" + name);
            return method;
        }

        public static MSA.Expression/*!*/ MakeArrayOpCall(IList<MSA.Expression>/*!*/ args) {
            Assert.NotNull(args);

            switch (args.Count) {
                case 0: return Methods.MakeArray0.OpCall();
                case 1: return Methods.MakeArray1.OpCall(AstUtils.Box(args[0]));
                case 2: return Methods.MakeArray2.OpCall(AstUtils.Box(args[0]), AstUtils.Box(args[1]));
                case 3: return Methods.MakeArray3.OpCall(AstUtils.Box(args[0]), AstUtils.Box(args[1]), AstUtils.Box(args[2]));
                case 4: return Methods.MakeArray4.OpCall(AstUtils.Box(args[0]), AstUtils.Box(args[1]), AstUtils.Box(args[2]), AstUtils.Box(args[3]));
                case 5: 
                    return Methods.MakeArray5.OpCall(new AstExpressions {
                        AstUtils.Box(args[0]), AstUtils.Box(args[1]), AstUtils.Box(args[2]), AstUtils.Box(args[3]), AstUtils.Box(args[4])
                    });

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

    public static class MethodInfoExtensions {
        public static MSA.Expression/*!*/ OpCall(this MethodInfo/*!*/ method) {
            return MSA.Expression.Call(null, method);
        }

        public static MSA.Expression/*!*/ OpCall(this MethodInfo/*!*/ method, MSA.Expression/*!*/ arg0) {
            return MSA.Expression.Call(method, arg0);
        }

        public static MSA.Expression/*!*/ OpCall(this MethodInfo/*!*/ method, MSA.Expression/*!*/ arg0, MSA.Expression/*!*/ arg1) {
            return MSA.Expression.Call(method, arg0, arg1);
        }

        public static MSA.Expression/*!*/ OpCall(this MethodInfo/*!*/ method, MSA.Expression/*!*/ arg0, MSA.Expression/*!*/ arg1, MSA.Expression/*!*/ arg2) {
            return MSA.Expression.Call(method, arg0, arg1, arg2);
        }

        public static MSA.Expression/*!*/ OpCall(this MethodInfo/*!*/ method, MSA.Expression/*!*/ arg0, MSA.Expression/*!*/ arg1, MSA.Expression/*!*/ arg2, MSA.Expression/*!*/ arg3) {
            return MSA.Expression.Call(method, arg0, arg1, arg2, arg3);
        }

        public static MSA.Expression/*!*/ OpCall(this MethodInfo/*!*/ method, AstExpressions/*!*/ args) {
            return MSA.Expression.Call(method, args);
        }

        public static MSA.Expression/*!*/ OpCall(this MethodInfo/*!*/ method, MSAst.ExpressionCollectionBuilder/*!*/ args) {
            return args.ToMethodCall(null, method);
        }
    }
}
