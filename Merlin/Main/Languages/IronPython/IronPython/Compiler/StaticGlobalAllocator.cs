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
using System.Dynamic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;

using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;
    
    /// <summary>
    /// Implements globals which are backed by a static type.  The global variables are stored in static fields 
    /// on a type for fast access.  The type also includes fields for constants and call sites so they can be accessed much fasetr.
    /// 
    /// We don't generate any code into the type though - DynamicMethod's are much faster for code gen then normal ref emit.
    /// </summary>
    class StaticGlobalAllocator : GlobalAllocator {
        private readonly TypeGen/*!*/ _typeGen;
        private readonly MSAst.Expression/*!*/ _codeContext;
        private readonly FieldBuilder/*!*/ _codeContextField;
        private readonly Dictionary<object/*!*/, ConstantInfo/*!*/>/*!*/ _constants = new Dictionary<object/*!*/, ConstantInfo/*!*/>();
        private readonly Dictionary<string/*!*/, ConstantInfo/*!*/>/*!*/ _globals = new Dictionary<string/*!*/, ConstantInfo/*!*/>();
        private readonly Dictionary<SymbolId, FieldBuilder/*!*/> _indirectSymbolIds/*!*/ = new Dictionary<SymbolId, FieldBuilder/*!*/>();
        private readonly List<SiteInfo/*!*/>/*!*/ _sites = new List<SiteInfo/*!*/>();
        private readonly Dictionary<SymbolId, PythonGlobal> _globalVals = new Dictionary<SymbolId, PythonGlobal>();
        private readonly CodeContext _context;
        private readonly Scope _scope;
        private int _constantsCreated, _sitesCreated;
#if SILVERLIGHT
        private StrongBox<Type> _finalType = new StrongBox<Type>();        
#endif

        private static readonly Type[] _DelegateCtorSignature = new Type[] { typeof(object), typeof(IntPtr) };
        private const MethodAttributes CtorAttributes = MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public;
        private const MethodImplAttributes ImplAttributes = MethodImplAttributes.Runtime | MethodImplAttributes.Managed;
        private const MethodAttributes InvokeAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;

        public StaticGlobalAllocator(LanguageContext/*!*/ context, string name) {
            _typeGen = Snippets.Shared.DefineType(name, typeof(object), false, false);

            _codeContextField = _typeGen.AddStaticField(typeof(CodeContext), "__global_context");
            _codeContext = CreateFieldBuilderExpression(_codeContextField);

            _scope = new Scope(new PythonDictionary(new GlobalDictionaryStorage(_globalVals)));
            _context = new CodeContext(_scope, context);
        }

        public override ScriptCode/*!*/ MakeScriptCode(MSAst.Expression/*!*/ lambda, CompilerContext/*!*/ compilerContext, PythonAst/*!*/ ast) {
            PythonContext context = (PythonContext)compilerContext.SourceUnit.LanguageContext;

            Type t = _typeGen.FinishType();
#if SILVERLIGHT
            _finalType.Value = t;
#endif

            // create the CodeContext for this optimized module
            InitOptimizedCodeContext(t);
            t.GetField("__global_context").SetValue(null, _context);

            // publish the cached constants
            foreach (var ci in _constants) {
                FieldInfo fi = t.GetField(ci.Value.Field.Name);
                fi.SetValue(null, ci.Key);
            }

            // publish all of the call site instances
            foreach (SiteInfo si in _sites) {
                FieldInfo fi = t.GetField(si.Field.Name);

                fi.SetValue(null, CallSite.Create(si.DelegateType, si.Binder));
            }

            // initialize all of the cached symbol IDs.
            ScriptingRuntimeHelpers.InitializeSymbols(t);

            string name = ((PythonCompilerOptions)compilerContext.Options).ModuleName ?? "<unnamed>";
            var func = Ast.Lambda<Func<object>>(lambda, name, new MSAst.ParameterExpression[0]);
            return new RuntimeScriptCode(compilerContext, func, ast, _context);
        }

        private void InitOptimizedCodeContext(Type/*!*/ t) {
            // create the CodeContext

            // now fill in the dictionary creating the globals which depend on the context
            foreach (var global in _globals) {
                SymbolId globalName = SymbolTable.StringToId(global.Key);

                FieldInfo fi = t.GetField(global.Value.Field.Name);
                fi.SetValue(null, _globalVals[globalName]);
            }
        }

        public override MSAst.Expression/*!*/ GlobalContext {
            get {
                return _codeContext;
            }
        }

        #region Cached site support

        public override MSAst.Expression/*!*/ Dynamic(DynamicMetaObjectBinder/*!*/ binder, Type/*!*/ retType, params System.Linq.Expressions.Expression/*!*/[]/*!*/ args) {
            Assert.NotNull(binder, retType, args);
            Assert.NotNullItems(args);

            // TODO: Optimize me
            Type delegateType = GetDelegateType(_typeGen, retType, args);
            Type siteType = typeof(CallSite<>).MakeGenericType(delegateType);

            FieldBuilder fi = _typeGen.AddStaticField(siteType, "site " + binder.ToString() + " #" + _sitesCreated++);
            MSAst.Expression siteField = CreateFieldBuilderExpression(fi);
            _sites.Add(new SiteInfo(binder, delegateType, fi, siteField));

            return new ReducableDynamicExpression(
                Ast.Call(
                    Ast.Field(
                        siteField,
                        siteType.GetField("Target")
                    ),
                    delegateType.GetMethod("Invoke"),
                    ArrayUtils.Insert(
                        siteField,
                        args
                    )
                ),
                binder,
                args
            );
        }

        internal static Type/*!*/ GetDelegateType(TypeGen/*!*/ typeGen, Type/*!*/ retType, System.Linq.Expressions.Expression/*!*/[]/*!*/ args) {
            Type delegateType;
            if (retType != typeof(void)) {
                Type[] types = new Type[args.Length + 2];
                types[0] = typeof(CallSite);

                for (int i = 0; i < args.Length; i++) {
                    types[i + 1] = args[i].Type;
                }

                types[types.Length - 1] = retType;
                delegateType = GetFuncType(types) ?? MakeNewCustomDelegate(typeGen, types);
            } else {
                Type[] types = new Type[args.Length + 2];
                types[0] = typeof(CallSite);

                for (int i = 0; i < args.Length; i++) {
                    types[i + 1] = args[i].Type;
                }
                delegateType = GetActionType(types) ?? MakeNewCustomDelegate(typeGen, ArrayUtils.Append(types, typeof(void)));
            }
            return delegateType;
        }

        private static Type GetFuncType(Type/*!*/[]/*!*/ types) {
            switch (types.Length) {
                #region Generated Delegate Func Types

                // *** BEGIN GENERATED CODE ***
                // generated by function: gen_delegate_func from: generate_dynsites.py

                case 1: return typeof(Func<>).MakeGenericType(types);
                case 2: return typeof(Func<,>).MakeGenericType(types);
                case 3: return typeof(Func<,,>).MakeGenericType(types);
                case 4: return typeof(Func<,,,>).MakeGenericType(types);
                case 5: return typeof(Func<,,,,>).MakeGenericType(types);
                case 6: return typeof(Func<,,,,,>).MakeGenericType(types);
                case 7: return typeof(Func<,,,,,,>).MakeGenericType(types);
                case 8: return typeof(Func<,,,,,,,>).MakeGenericType(types);
                case 9: return typeof(Func<,,,,,,,,>).MakeGenericType(types);
                case 10: return typeof(Func<,,,,,,,,,>).MakeGenericType(types);
                case 11: return typeof(Func<,,,,,,,,,,>).MakeGenericType(types);
                case 12: return typeof(Func<,,,,,,,,,,,>).MakeGenericType(types);
                case 13: return typeof(Func<,,,,,,,,,,,,>).MakeGenericType(types);
                case 14: return typeof(Func<,,,,,,,,,,,,,>).MakeGenericType(types);
                case 15: return typeof(Func<,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 16: return typeof(Func<,,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 17: return typeof(Func<,,,,,,,,,,,,,,,,>).MakeGenericType(types);

                // *** END GENERATED CODE ***

                #endregion

                default: return null;
            }
        }

        private static Type/*!*/ GetActionType(Type/*!*/[]/*!*/ types) {
            switch (types.Length) {
                case 0: return typeof(Action);
                #region Generated Delegate Action Types

                // *** BEGIN GENERATED CODE ***
                // generated by function: gen_delegate_action from: generate_dynsites.py

                case 1: return typeof(Action<>).MakeGenericType(types);
                case 2: return typeof(Action<,>).MakeGenericType(types);
                case 3: return typeof(Action<,,>).MakeGenericType(types);
                case 4: return typeof(Action<,,,>).MakeGenericType(types);
                case 5: return typeof(Action<,,,,>).MakeGenericType(types);
                case 6: return typeof(Action<,,,,,>).MakeGenericType(types);
                case 7: return typeof(Action<,,,,,,>).MakeGenericType(types);
                case 8: return typeof(Action<,,,,,,,>).MakeGenericType(types);
                case 9: return typeof(Action<,,,,,,,,>).MakeGenericType(types);
                case 10: return typeof(Action<,,,,,,,,,>).MakeGenericType(types);
                case 11: return typeof(Action<,,,,,,,,,,>).MakeGenericType(types);
                case 12: return typeof(Action<,,,,,,,,,,,>).MakeGenericType(types);
                case 13: return typeof(Action<,,,,,,,,,,,,>).MakeGenericType(types);
                case 14: return typeof(Action<,,,,,,,,,,,,,>).MakeGenericType(types);
                case 15: return typeof(Action<,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 16: return typeof(Action<,,,,,,,,,,,,,,,>).MakeGenericType(types);

                // *** END GENERATED CODE ***

                #endregion

                default: return null;
            }
        }

        private static Type/*!*/ MakeNewCustomDelegate(TypeGen/*!*/ typeGen, Type/*!*/[]/*!*/ types) {
            Type returnType = types[types.Length - 1];
            Type[] parameters = ArrayUtils.RemoveLast(types);

            TypeBuilder builder = Snippets.Shared.DefineDelegateType("Delegate" + types.Length);
            builder.DefineConstructor(CtorAttributes, CallingConventions.Standard, _DelegateCtorSignature).SetImplementationFlags(ImplAttributes);
            builder.DefineMethod("Invoke", InvokeAttributes, returnType, parameters).SetImplementationFlags(ImplAttributes);
            return builder.CreateType();
        }

        #endregion

        #region Global Implementation

        protected override MSAst.Expression/*!*/ GetGlobal(string/*!*/ name, AstGenerator/*!*/ ag, bool isLocal) {
            Assert.NotNull(name);

            PythonGlobal global = _globalVals[SymbolTable.StringToId(name)] = new PythonGlobal(_context, SymbolTable.StringToId(name));
            return new PythonGlobalVariableExpression(GetGlobalInfo(name).Expression, global);
        }

        private ConstantInfo/*!*/ GetGlobalInfo(string name) {
            ConstantInfo res;
            if (!_globals.TryGetValue(name, out res)) {
                FieldBuilder field = _typeGen.AddStaticField(typeof(PythonGlobal), FieldAttributes.Public, GetGlobalFieldName(name));

                _globals[name] = res = new ConstantInfo(field, CreateFieldBuilderExpression(field));
            }

            return res;
        }

        private static string/*!*/ GetGlobalFieldName(string name) {
            return "Global " + name;
        }

        #endregion

        #region Cached Constant Support

        public override MSAst.Expression/*!*/ GetConstant(object value) {
            // if we can emit the value and we won't be continiously boxing/unboxing
            // then don't bother caching the value in a static field.
            
            // TODO: Sometimes we don't want to pre-box the values, such as if it's an int
            // going to a call site which can be strongly typed.  We need to coordinate 
            // more with whoever consumes the values.
            if (CompilerHelpers.CanEmitConstant(value, CompilerHelpers.GetType(value)) &&
                !CompilerHelpers.GetType(value).IsValueType) {
                return Ast.Constant(value);
            }

            ConstantInfo ci;
            if (!_constants.TryGetValue(value, out ci)) {
                string name = "Constant " + (_constantsCreated++) + CompilerHelpers.GetType(value).Name;
                FieldBuilder field = _typeGen.AddStaticField(typeof(object), FieldAttributes.Public, name);

                _constants[value] = ci = new ConstantInfo(field, CreateFieldBuilderExpression(field));
            }

            return ci.Expression;
        }

        public FieldBuilderExpression/*!*/ CreateFieldBuilderExpression(FieldBuilder/*!*/ builder) {
#if SILVERLIGHT
            return new FieldBuilderExpression(builder, _finalType);
#else
            return new FieldBuilderExpression(builder);
#endif
        }

        class ConstantInfo {
            public readonly MSAst.Expression/*!*/ Expression;
            public readonly FieldInfo/*!*/ Field;

            public ConstantInfo(FieldInfo/*!*/ field, MSAst.Expression/*!*/ expr) {
                Assert.NotNull(field, expr);

                Field = field;
                Expression = expr;
            }
        }

        class SiteInfo : ConstantInfo {
            public readonly DynamicMetaObjectBinder/*!*/ Binder;
            public readonly Type/*!*/ DelegateType;

            public SiteInfo(DynamicMetaObjectBinder/*!*/ binder, Type/*!*/ delegateType, FieldInfo/*!*/ field, MSAst.Expression/*!*/ expr)
                : base(field, expr) {
                Assert.NotNull(binder, delegateType);

                Binder = binder;
                DelegateType = delegateType;
            }
        }

        #endregion
    }
}
