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
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
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
    /// Implements globals which are backed by a static type, followed by an array if the static types' slots become full.  The global
    /// variables are stored in static fields on a type for fast access.  The type also includes fields for constants and call sites
    /// so they can be accessed much fasetr.
    /// 
    /// We don't generate any code into the type though - DynamicMethod's are much faster for code gen then normal ref emit.
    /// </summary>
    partial class SharedGlobalAllocator : GlobalAllocator {
        private static readonly Dictionary<object/*!*/, ConstantInfo/*!*/>/*!*/ _allConstants = new Dictionary<object/*!*/, ConstantInfo/*!*/>();
        private static readonly Dictionary<SymbolId, ConstantInfo/*!*/>/*!*/ _allSymbols = new Dictionary<SymbolId, ConstantInfo/*!*/>();
        private static readonly Dictionary<Type/*!*/, DelegateCache/*!*/>/*!*/ _delegateCache = new Dictionary<Type/*!*/, DelegateCache/*!*/>();

        private readonly MSAst.Expression/*!*/ _codeContext;
        private readonly CodeContext/*!*/ _context;
        private readonly ConstantInfo/*!*/ _codeContextInfo;
        private readonly Dictionary<object/*!*/, ConstantInfo/*!*/>/*!*/ _constants = new Dictionary<object/*!*/, ConstantInfo/*!*/>();
        private readonly Dictionary<SymbolId, ConstantInfo/*!*/>/*!*/ _symbols = new Dictionary<SymbolId, ConstantInfo/*!*/>();
        private readonly Dictionary<string/*!*/, ConstantInfo/*!*/>/*!*/ _globals = new Dictionary<string/*!*/, ConstantInfo/*!*/>();
        private readonly Dictionary<SymbolId, PythonGlobal/*!*/>/*!*/ _globalVals = new Dictionary<SymbolId, PythonGlobal>();
        private readonly List<SiteInfo/*!*/>/*!*/ _sites = new List<SiteInfo/*!*/>();
        private readonly Scope _scope;

        public SharedGlobalAllocator(PythonContext/*!*/ context) {
            _codeContextInfo = NextContext();
            _codeContext = _codeContextInfo.Expression;

            _scope = new Scope(new PythonDictionary(new GlobalDictionaryStorage(_globalVals)));
            _context = new CodeContext(_scope, context);
        }

        public override ScriptCode/*!*/ MakeScriptCode(MSAst.Expression/*!*/ lambda, CompilerContext/*!*/ compilerContext, PythonAst/*!*/ ast, Dictionary<int, bool> handlerLocations, Dictionary<int, Dictionary<int, bool>> loopAndFinallyLocations) {
            PythonContext context = (PythonContext)compilerContext.SourceUnit.LanguageContext;

            // create the CodeContext for this optimized module
            PublishGlobals(_globals, _globalVals);
            PublishContext(_context, _codeContextInfo);

            // publish the cached constants
            PublishConstants(_constants);
            PublishSymbols(_symbols);

            // publish all of the call site instances
            PublishSites(_sites);

            /*
            Console.WriteLine(
                "{0} constants, {1} contexts, {2} globals, {3} symbols (total); {4} sites (this module)",
                StorageData.ConstantCount, StorageData.ContextCount, StorageData.GlobalCount, StorageData.SymbolCount,
                _sites.Count
            );
            */

            string name = ((PythonCompilerOptions)compilerContext.Options).ModuleName ?? "<unnamed>";
            var func = Ast.Lambda<Func<FunctionCode, object>>(Utils.Convert(lambda, typeof(object)), name, new [] { AstGenerator._functionCode });
            return new RuntimeScriptCode(compilerContext, func, ast, _context);
        }

        public override MSAst.Expression/*!*/ GlobalContext {
            get {
                return _codeContext;
            }
        }

        #region Cached Constant/Symbol/Global Support

        public override MSAst.Expression/*!*/ GetConstant(object value) {
            // if we can emit the value and we won't be continiously boxing/unboxing
            // then don't bother caching the value in a static field.
            
            // TODO: Sometimes we don't want to pre-box the values, such as if it's an int
            // going to a call site which can be strongly typed.  We need to coordinate 
            // more with whoever consumes the values.
            if (CompilerHelpers.CanEmitConstant(value, CompilerHelpers.GetType(value)) &&
                !CompilerHelpers.GetType(value).IsValueType) {
                return Utils.Constant(value);
            }

            ConstantInfo ci;
            if (!_allConstants.TryGetValue(value, out ci)) {
                _allConstants[value] = _constants[value] = ci = NextConstant(_constants.Count, CompilerHelpers.GetType(value));
            }

            return ci.Expression;
        }

        public override MSAst.Expression/*!*/ GetSymbol(SymbolId name) {
            ConstantInfo ci;
            if (!_allSymbols.TryGetValue(name, out ci)) {
                _allSymbols[name] = _symbols[name] = ci = NextSymbol(_symbols.Count);
            }

            return ci.Expression;
        }

        protected override MSAst.Expression/*!*/ GetGlobal(string/*!*/ name, AstGenerator/*!*/ ag, bool isLocal) {
            Assert.NotNull(name);

            PythonGlobal global = _globalVals[SymbolTable.StringToId(name)] = new PythonGlobal(_context, SymbolTable.StringToId(name));
            return new PythonGlobalVariableExpression(GetGlobalInfo(name).Expression, global);
        }

        private ConstantInfo/*!*/ GetGlobalInfo(string name) {
            ConstantInfo res;
            if (!_globals.TryGetValue(name, out res)) {
                _globals[name] = res = NextGlobal(_globals.Count);
            }

            return res;
        }

        #endregion

        #region Field Allocation and Publishing

        private static ConstantInfo/*!*/ NextContext() {
            lock (StorageData.Contexts) {
                int index = StorageData.ContextCount++;
                int arrIndex = index - StorageData.ContextTypes * StorageData.StaticFields;
                Type storageType = StorageData.ContextStorageType(index);

                MSAst.Expression expr;
                FieldInfo fieldInfo;
                if (arrIndex < 0) {
                    fieldInfo = storageType.GetField(string.Format("Context{0:000}", index % StorageData.StaticFields));
                    expr = Ast.Field(null, fieldInfo);
                } else {
                    fieldInfo = typeof(StorageData).GetField("Contexts");
                    expr = Ast.ArrayIndex(
                        Ast.Field(null, fieldInfo),
                        Ast.Constant(arrIndex, typeof(int))
                    );
                }

                return new ConstantInfo(expr, fieldInfo, index);
            }
        }

        private static ConstantInfo/*!*/ NextConstant(int offset, Type/*!*/ returnType) {
            return new ConstantInfo(new ConstantExpression(offset, returnType), null, offset);
        }

        private static ConstantInfo/*!*/ NextSymbol(int offset) {
            return new ConstantInfo(new SymbolExpression(offset), null, offset);
        }

        private static ConstantInfo/*!*/ NextGlobal(int offset) {
            return new ConstantInfo(new GlobalExpression(offset), null, offset);
        }

        // public for accessibility via GetMethod("NextSite")
        public static SiteInfo/*!*/ NextSite<T>(DynamicMetaObjectBinder/*!*/ binder) where T : class {
            lock (StorageData.SiteLockObj) {
                int index = SiteStorage<T>.SiteCount++;
                int arrIndex = index - StorageData.SiteTypes * StorageData.StaticFields;
                Type storageType = SiteStorage<T>.SiteStorageType(index);

                MSAst.Expression expr;
                FieldInfo fieldInfo;
                if (arrIndex < 0) {
                    fieldInfo = storageType.GetField(string.Format("Site{0:000}", index % StorageData.StaticFields));
                    expr = Ast.Field(null, fieldInfo);
                } else {
                    fieldInfo = typeof(SiteStorage<T>).GetField("Sites");
                    expr = Ast.ArrayIndex(
                        Ast.Field(null, fieldInfo),
                        Ast.Constant(arrIndex, typeof(int))
                    );
                }

                return new SiteInfo<T>(binder, expr, fieldInfo, index);
            }
        }

        // Note: This should stay non-public to avoid name conflicts when accessing the
        //       generic overload via GetMethod("NextSite")
        private static SiteInfo/*!*/ NextSite(DynamicMetaObjectBinder/*!*/ binder, Type/*!*/ delegateType) {
            Type siteType = typeof(SiteStorage<>).MakeGenericType(delegateType);

            lock (StorageData.SiteLockObj) {
                int index = (int)siteType.GetField("SiteCount").GetValue(null);
                siteType.GetField("SiteCount").SetValue(null, index + 1);
                int arrIndex = index - StorageData.SiteTypes * StorageData.StaticFields;
                Type storageType = (Type)siteType.GetMethod("SiteStorageType").Invoke(null, new object[] { index });

                MSAst.Expression expr;
                FieldInfo fieldInfo;
                if (arrIndex < 0) {
                    fieldInfo = storageType.GetField(string.Format("Site{0:000}", index % StorageData.StaticFields));
                    expr = Ast.Field(null, fieldInfo);
                } else {
                    fieldInfo = siteType.GetField("Sites");
                    expr = Ast.ArrayIndex(
                        Ast.Field(null, fieldInfo),
                        Ast.Constant(arrIndex, typeof(int))
                    );
                }

                return new SiteInfoLarge(binder, expr, fieldInfo, index, delegateType);
            }
        }

        private static void PublishContext(CodeContext/*!*/ context, ConstantInfo/*!*/ codeContextInfo) {
            int arrIndex = codeContextInfo.Offset - StorageData.ContextTypes * StorageData.StaticFields;

            if (arrIndex < 0) {
                codeContextInfo.Field.SetValue(null, context);
            } else {
                lock (StorageData.Contexts) {
                    StorageData.Contexts[arrIndex] = context;
                }
            }
        }

        private static void PublishConstants(Dictionary<object/*!*/, ConstantInfo/*!*/>/*!*/ constants) {
            if (constants.Count > 0) {
                lock (StorageData.Constants) {
                    int start = StorageData.ConstantCount;
                    StorageData.ConstantCount += constants.Count;
                    StorageData.ConstantStorageType(StorageData.ConstantCount - 1); // resize array once

                    foreach (var constant in constants) {
                        PublishWorker(start, StorageData.ConstantTypes, constant.Value, constant.Key, StorageData.Constants);
                    }
                }
            }
        }

        private static void PublishSymbols(Dictionary<SymbolId, ConstantInfo/*!*/>/*!*/ symbols) {
            if (symbols.Count > 0) {
                lock (StorageData.Symbols) {
                    int start = StorageData.SymbolCount;
                    StorageData.SymbolCount += symbols.Count;
                    StorageData.SymbolStorageType(StorageData.SymbolCount - 1); // resize array once

                    foreach (var symbol in symbols) {
                        PublishWorker(start, StorageData.SymbolTypes, symbol.Value, symbol.Key, StorageData.Symbols);
                    }
                }
            }
        }

        private static void PublishGlobals(Dictionary<string/*!*/, ConstantInfo/*!*/>/*!*/ globals, Dictionary<SymbolId, PythonGlobal/*!*/>/*!*/ globalVals) {
            Assert.Equals(globals.Count, globalVals.Count);

            if (globals.Count > 0) {
                lock (StorageData.Globals) {
                    int start = StorageData.GlobalCount;
                    StorageData.GlobalCount += globals.Count;
                    StorageData.GlobalStorageType(StorageData.GlobalCount - 1); // resize array once

                    foreach (var global in globals) {
                        PublishWorker(start, StorageData.GlobalTypes, global.Value, globalVals[SymbolTable.StringToId(global.Key)], StorageData.Globals);
                    }
                }
            }
        }

        private static void PublishSites(List<SiteInfo/*!*/>/*!*/ sites) {
            foreach (SiteInfo si in sites) {
                int arrIndex = si.Offset - StorageData.SiteTypes * StorageData.StaticFields;
                CallSite site = si.MakeSite();

                if (arrIndex < 0) {
                    si.Field.SetValue(null, site);
                } else {
                    lock (StorageData.SiteLockObj) {
                        ((CallSite[])si.Field.GetValue(null))[arrIndex] = site;
                    }
                }
            }
        }

        private static void PublishWorker<T>(int start, int nTypes, ConstantInfo info, T value, T[] fallbackArray) {
            int arrIndex = start + info.Offset - nTypes * StorageData.StaticFields;
            ((ReducibleExpression)info.Expression).Start = start;

            if (arrIndex < 0) {
                ((ReducibleExpression)info.Expression).FieldInfo.SetValue(null, value);
            } else {
                fallbackArray[arrIndex] = value;
            }
        }

        #endregion

        #region ConstantInfo

        public class ConstantInfo {
            public readonly MSAst.Expression/*!*/ Expression;
            public readonly FieldInfo Field;
            public readonly int Offset;

            public ConstantInfo(MSAst.Expression/*!*/ expr, FieldInfo field, int offset) {
                Assert.NotNull(expr);

                Expression = expr;
                Field = field;
                Offset = offset;
            }
        }

        public abstract class SiteInfo : ConstantInfo {
            public readonly DynamicMetaObjectBinder/*!*/ Binder;
            public readonly Type/*!*/ DelegateType;

            protected Type/*!*/ _siteType;
            public Type/*!*/ SiteType {
                get {
                    if (_siteType != null) {
                        _siteType = typeof(CallSite<>).MakeGenericType(DelegateType);
                    }
                    return _siteType;
                }
            }

            public SiteInfo(DynamicMetaObjectBinder/*!*/ binder, MSAst.Expression/*!*/ expr, FieldInfo/*!*/ field, int index, Type/*!*/ delegateType)
                : base(expr, field, index) {
                Assert.NotNull(binder);

                Binder = binder;
                DelegateType = delegateType;
            }

            public SiteInfo(DynamicMetaObjectBinder/*!*/ binder, MSAst.Expression/*!*/ expr, FieldInfo/*!*/ field, int index, Type/*!*/ delegateType, Type/*!*/ siteType)
                : this(binder, expr, field, index, delegateType) {
                _siteType = siteType;
            }

            public abstract CallSite/*!*/ MakeSite();
        }

        public class SiteInfoLarge : SiteInfo {
            public SiteInfoLarge(DynamicMetaObjectBinder/*!*/ binder, MSAst.Expression/*!*/ expr, FieldInfo/*!*/ field, int index, Type/*!*/ delegateType)
                : base (binder, expr, field, index, delegateType) { }

            public override CallSite MakeSite() {
                return CallSite.Create(DelegateType, Binder);
            }
        }

        public class SiteInfo<T> : SiteInfo where T : class {
            public SiteInfo(DynamicMetaObjectBinder/*!*/ binder, MSAst.Expression/*!*/ expr, FieldInfo/*!*/ field, int index)
                : base(binder, expr, field, index, typeof(T), typeof(CallSite<T>)) { }

            public override CallSite MakeSite() {
                return CallSite<T>.Create(Binder);
            }
        }
        
        #endregion

        #region Dynamic CallSite Type Information

        private sealed class DelegateCache {
            public Type/*!*/ DelegateType;
            public Type/*!*/ SiteType;

            public Func<DynamicMetaObjectBinder/*!*/, SiteInfo/*!*/>/*!*/ NextSite;
            public FieldInfo/*!*/ TargetField; // SiteType.GetField("Target")
            public MethodInfo/*!*/ InvokeMethod; // DelegateType.GetMethod("Invoke")

            public Dictionary<Type/*!*/, DelegateCache> TypeChain;

            public void MakeDelegateType(Type/*!*/ retType, params MSAst.Expression/*!*/[]/*!*/ args) {
                DelegateType = GetDelegateType(retType, args);
                SiteType = typeof(CallSite<>).MakeGenericType(DelegateType);
                NextSite = (Func<DynamicMetaObjectBinder, SiteInfo>)Delegate.CreateDelegate(
                    typeof(Func<DynamicMetaObjectBinder, SiteInfo>),
                    typeof(SharedGlobalAllocator).GetMethod("NextSite").MakeGenericMethod(DelegateType)
                );
                TargetField = SiteType.GetField("Target");
                InvokeMethod = DelegateType.GetMethod("Invoke");
            }

            public static DelegateCache FirstCacheNode(Type/*!*/ argType) {
                DelegateCache nextCacheNode;
                if (!_delegateCache.TryGetValue(argType, out nextCacheNode)) {
                    nextCacheNode = new DelegateCache();
                    _delegateCache[argType] = nextCacheNode;
                }

                return nextCacheNode;
            }

            public DelegateCache NextCacheNode(Type/*!*/ argType) {
                Assert.NotNull(argType);
                
                DelegateCache nextCacheNode;
                if (TypeChain == null) {
                    TypeChain = new Dictionary<Type, DelegateCache>();
                }

                if (!TypeChain.TryGetValue(argType, out nextCacheNode)) {
                    nextCacheNode = new DelegateCache();
                    TypeChain[argType] = nextCacheNode;
                }

                return nextCacheNode;
            }
        }

        #endregion

        #region Reducible Expressions

        internal abstract class ReducibleExpression : MSAst.Expression {
            private readonly int _offset;
            private int _start = -1;
            private FieldInfo _fieldInfo;
            
            public ReducibleExpression(int offset) {
                _offset = offset;
            }

            public abstract string/*!*/ Name { get; }
            public abstract int FieldCount { get; }
            public abstract override Type/*!*/ Type { get; }
            protected abstract Type/*!*/ GetStorageType(int index);
            
            public FieldInfo FieldInfo {
                get {
                    return _fieldInfo;
                }
            }

            // Note: Because of a call to GetStorageType, which possibly resizes a storage
            //       array, a lock must be acquired prior to setting this property.
            public int Start {
                get {
                    return _start;
                }
                set {
                    Debug.Assert(_start < 0); // setter should only be called once
                    Debug.Assert(value >= 0);

                    _start = value;
                    int index = _offset + _start;
                    Type storageType = GetStorageType(index);
                    if (storageType != typeof(StorageData)) {
                        _fieldInfo = storageType.GetField(Name + string.Format("{0:000}", index % StorageData.StaticFields));
                    } else {
                        _fieldInfo = typeof(StorageData).GetField(Name + "s");
                    }
                }
            }

            public override MSAst.Expression/*!*/ Reduce() {
                Debug.Assert(_start >= 0);
                Assert.NotNull(_fieldInfo);

                int index = _offset + _start;
                int arrIndex = index - FieldCount;
                if (arrIndex < 0) {
                    return Ast.Field(null, _fieldInfo);
                } else {
                    return Ast.ArrayIndex(
                        Ast.Field(null, _fieldInfo),
                        Ast.Constant(arrIndex, typeof(int))
                    );
                }
            }

            public override MSAst.ExpressionType NodeType {
                get {
                    return MSAst.ExpressionType.Extension;
                }
            }

            protected override MSAst.Expression Accept(MSAst.ExpressionVisitor visitor) {
                return this;
            }

            protected override MSAst.Expression VisitChildren(MSAst.ExpressionVisitor visitor) {
                return this;
            }

            public override bool CanReduce {
                get {
                    return true;
                }
            }
        }

        internal sealed class ConstantExpression : ReducibleExpression {
            private Type/*!*/ _returnType;

            public ConstantExpression(int offset, Type/*!*/ returnType) : base(offset) {
                if (!returnType.IsValueType) {
                    _returnType = returnType;
                } else {
                    _returnType = typeof(object);
                }
            }

            public override string/*!*/ Name {
                get { return "Constant"; }
            }

            public override int FieldCount {
                get { return StorageData.ConstantTypes * StorageData.StaticFields; }
            }

            protected override Type/*!*/ GetStorageType(int index) {
                return StorageData.ConstantStorageType(index);
            }

            public override Type/*!*/ Type {
                get { return _returnType; }
            }

            public override MSAst.Expression/*!*/ Reduce() {
                if (_returnType == typeof(object)) {
                    return base.Reduce();
                } else {
                    return MSAst.Expression.Convert(base.Reduce(), _returnType);
                }
            }
        }

        internal sealed class SymbolExpression : ReducibleExpression {
            public SymbolExpression(int offset) : base(offset) { }

            public override string/*!*/ Name {
                get { return "Symbol"; }
            }

            public override int FieldCount {
                get { return StorageData.SymbolTypes * StorageData.StaticFields; }
            }

            protected override Type/*!*/ GetStorageType(int index) {
                return StorageData.SymbolStorageType(index);
            }

            public override Type/*!*/ Type {
                get { return typeof(SymbolId); }
            }
        }

        internal sealed class GlobalExpression : ReducibleExpression {
            public GlobalExpression(int offset) : base(offset) { }

            public override string/*!*/ Name {
                get { return "Global"; }
            }

            public override int FieldCount {
                get { return StorageData.GlobalTypes * StorageData.StaticFields; }
            }

            protected override Type/*!*/ GetStorageType(int index) {
                return StorageData.GlobalStorageType(index);
            }

            public override Type/*!*/ Type {
                get { return typeof(PythonGlobal); }
            }
        }

        #endregion
    }
}
