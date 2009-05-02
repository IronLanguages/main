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
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Text.RegularExpressions;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime.Calls;
using System.Reflection;
using IronRuby.Compiler.Generation;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Scripting.Interpreter;

namespace IronRuby.Runtime {

    public sealed class RuntimeFlowControl {
        internal bool IsActiveMethod;
    }

    public enum ScopeKind {
        TopLevel,
        Method,
        Module,
        Block
    }
        
#if !SILVERLIGHT
    [DebuggerTypeProxy(typeof(RubyScope.DebugView))]
#endif
    public abstract class RubyScope {
        private sealed class EmptyRuntimeVariables : IRuntimeVariables {
            int IRuntimeVariables.Count {
                get { return 0; }
            }

            object IRuntimeVariables.this[int index] {
                get { throw new IndexOutOfRangeException(); }
                set { throw new IndexOutOfRangeException(); }
            }
        }

        internal static readonly LocalsDictionary _EmptyLocals = new LocalsDictionary(new EmptyRuntimeVariables(), new SymbolId[0]);

        internal bool InLoop;
        internal bool InRescue;

        private IAttributesCollection/*!*/ _frame;
        private readonly RubyTopLevelScope/*!*/ _top;
        private readonly RubyScope _parent;

        private readonly object _selfObject;

        // cached ImmediateClassOf(_selfObject):
        private RubyClass _selfImmediateClass;

        private readonly RuntimeFlowControl/*!*/ _runtimeFlowControl; // TODO: merge?

        // set by private/public/protected/module_function
        private RubyMethodAttributes _methodAttributes;

        public abstract ScopeKind Kind { get; }
        public abstract bool InheritsLocalVariables { get; }

        public virtual RubyModule Module {
            get { return null; }
        }

        public object SelfObject {
            get { return _selfObject; }
        }

        internal RubyClass/*!*/ SelfImmediateClass {
            get {
                if (_selfImmediateClass == null) {
                    // thread-safe, since all threads will calculate the same result:
                    _selfImmediateClass = RubyContext.GetImmediateClassOf(_selfObject);
                }
                return _selfImmediateClass; 
            }
        }

        public RubyMethodVisibility Visibility {
            get { return (RubyMethodVisibility)(_methodAttributes & RubyMethodAttributes.VisibilityMask); }
        }

        public RubyMethodAttributes MethodAttributes {
            get { return _methodAttributes; }
            set { _methodAttributes = value; }
        }

        public RuntimeFlowControl/*!*/ RuntimeFlowControl {
            get { return _runtimeFlowControl; }
        }

        public RubyGlobalScope/*!*/ GlobalScope {
            get { return _top.RubyGlobalScope; }
        }

        public RubyTopLevelScope/*!*/ Top {
            get { return _top; }
        }

        public RubyContext/*!*/ RubyContext {
            get { return _top.RubyContext; }
        }

        public IAttributesCollection/*!*/ Frame {
            get {
                // TODO: Debug.Assert(_frame != null);
                return _frame; 
            }
            internal set {
                Debug.Assert(_frame == null && value != null);
                _frame = value;
            }
        }

        internal InterpretedFrame InterpretedFrame { get; set; }

        public RubyScope Parent {
            get { return _parent; }
        }

        // top scope:
        protected RubyScope(RuntimeFlowControl/*!*/ runtimeFlowControl, object selfObject) {
            _top = (RubyTopLevelScope)this;
            _parent = null;
            _selfObject = selfObject;
            _runtimeFlowControl = runtimeFlowControl;
            _methodAttributes = RubyMethodAttributes.PrivateInstance;
        }

        // other scopes:
        protected RubyScope(RubyScope/*!*/ parent, RuntimeFlowControl/*!*/ runtimeFlowControl, object selfObject) {
            Assert.NotNull(parent);
            _parent = parent;
            _top = parent.Top;
            _selfObject = selfObject;
            _runtimeFlowControl = runtimeFlowControl;
            _methodAttributes = RubyMethodAttributes.PrivateInstance;
        }

        public bool IsEmpty {
            get { return RubyContext.EmptyScope == this; }
        }

        protected virtual bool IsClosureScope {
            get { return false; }
        }

#if DEBUG
        private string _debugName;

        public override string ToString() {
            return _debugName;
        }
#endif
        [Conditional("DEBUG")]
        public void SetDebugName(string name) {
#if DEBUG
            _debugName = name;
#endif
        }

        // TODO:
        public List<string/*!*/>/*!*/ GetVisibleLocalNames() {
            var result = new List<string>();
            RubyScope scope = this;
            while (true) {
                foreach (object name in scope.Frame.Keys) {
                    string strName = name as string;
                    if (strName != null && !strName.StartsWith("#")) {
                        result.Add(strName);
                    }
                }

                if (!scope.InheritsLocalVariables) {
                    return result;
                }

                scope = scope.Parent;
            }
        }

        internal object ResolveLocalVariable(SymbolId name) {
            RubyScope scope = this;
            while (true) {
                object value;
                if (scope.Frame.TryGetValue(name, out value)) {
                    return value;
                }

                if (!scope.InheritsLocalVariables) {
                    return null;
                }

                scope = scope.Parent;
            }
        }

        internal object ResolveAndSetLocalVariable(SymbolId name, object value) {
            RubyScope scope = this;
            while (true) {
                if (scope.Frame.ContainsKey(name)) {
                    return scope.Frame[name] = value;
                }

                if (!scope.InheritsLocalVariables) {
                    return this.Frame[name] = value;
                }

                scope = scope.Parent;
            }
        }

        public RubyModule/*!*/ GetInnerMostModuleForConstantLookup() {
            return GetInnerMostModule(false, RubyContext.ObjectClass);
        }

        public RubyModule/*!*/ GetInnerMostModuleForMethodLookup() {
            return GetInnerMostModule(false, Top.MethodLookupModule ?? RubyContext.ObjectClass);
        }

        public RubyModule/*!*/ GetInnerMostModuleForClassVariableLookup() {
            return GetInnerMostModule(true, RubyContext.ObjectClass);
        }
        
        private RubyModule/*!*/ GetInnerMostModule(bool skipSingletons, RubyModule/*!*/ fallbackModule) {
            RubyScope scope = this;
            do {
                RubyModule result = scope.Module;
                if (result != null && (!skipSingletons || !result.IsSingletonClass)) {
                    return result;
                }
                scope = scope.Parent;
            } while (scope != null);
            return fallbackModule;
        }

        public RubyMethodScope GetInnerMostMethodScope() {
            RubyScope scope = this;
            while (scope != null && scope.Kind != ScopeKind.Method) {
                scope = scope.Parent;
            }
            return (RubyMethodScope)scope;
        }

        public RubyClosureScope/*!*/ GetInnerMostClosureScope() {
            RubyScope scope = this;
            while (scope != null && !scope.IsClosureScope) {
                scope = scope.Parent;
            }
            return (RubyClosureScope)scope;
        }

        public void GetInnerMostBlockOrMethodScope(out RubyBlockScope blockScope, out RubyMethodScope methodScope) {
            methodScope = null;
            blockScope = null;
            RubyScope scope = this;
            while (scope != null) {
                switch (scope.Kind) {
                    case ScopeKind.Block:
                        blockScope = (RubyBlockScope)scope;
                        return;

                    case ScopeKind.Method:
                        methodScope = (RubyMethodScope)scope;
                        return;
                }

                scope = scope.Parent;
            }
        }

        internal void GetSuperCallTarget(out RubyModule declaringModule, out string/*!*/ methodName, out object self) {
            RubyScope scope = this;
            while (true) {
                Debug.Assert(scope != null);

                switch (scope.Kind) {
                    case ScopeKind.Method:
                        RubyMethodScope methodScope = (RubyMethodScope)scope;
                        // See RubyOps.DefineMethod for why we can use Method here.
                        declaringModule = methodScope.Method.DeclaringModule;
                        methodName = methodScope.Method.DefinitionName;
                        self = scope.SelfObject;
                        return;

                    case ScopeKind.Block:
                        BlockParam blockParam = ((RubyBlockScope)scope).BlockFlowControl;
                        if (blockParam.MethodName != null) {
                            declaringModule = blockParam.MethodLookupModule;
                            methodName = blockParam.MethodName;
                            self = scope.SelfObject;
                            return;
                        }
                        break;

                    case ScopeKind.TopLevel:
                        throw RubyOps.MakeTopLevelSuperException();
                }

                scope = scope.Parent;
            }
        }

        public RubyScope/*!*/ GetMethodAttributesDefinitionScope() {
            RubyScope scope = this;
            while (true) {
                if (scope.Kind == ScopeKind.Block) {
                    BlockParam blockParam = ((RubyBlockScope)scope).BlockFlowControl;
                    if (blockParam.MethodLookupModule != null && blockParam.MethodName == null) {
                        return scope;
                    }
                } else {
                    return scope;
                }

                scope = scope.Parent;
            }
        }

        internal RubyModule/*!*/ GetMethodDefinitionOwner() {
            // MRI 1.9: skips all module_eval and define_method blocks.
            // MRI 1.8: skips module_eval and define_method blocks above method scope.
            // IronRuby: Fallback to the top-level singleton class when hosted.
            if (RubyContext.RubyOptions.Compatibility == RubyCompatibility.Ruby19) {
                return GetInnerMostModuleForMethodLookup();
            }

            RubyScope scope = this;
            while (true) {
                Debug.Assert(scope != null);

                switch (scope.Kind) {
                    case ScopeKind.TopLevel:
                        Debug.Assert(scope == Top);
                        return Top.MethodLookupModule ?? Top.TopModuleOrObject;

                    case ScopeKind.Module:
                        Debug.Assert(scope.Module != null);
                        return scope.Module;

                    case ScopeKind.Method:
                        return scope.GetInnerMostModuleForMethodLookup();

                    case ScopeKind.Block:
                        BlockParam blockParam = ((RubyBlockScope)scope).BlockFlowControl;
                        if (blockParam.MethodLookupModule != null) {
                            return blockParam.MethodLookupModule;
                        }
                        break;
                }

                scope = scope.Parent;
            }
        }

        // thread-safe:
        // dynamic dispatch to "const_missing" if not found
        public object ResolveConstant(bool autoload, string/*!*/ name) {
            object result;

            if (TryResolveConstant(autoload, name, out result)) {
                return result;
            }

            RubyContext.CheckConstantName(name);
            var owner = GetInnerMostModuleForConstantLookup();
            return owner.Context.ConstantMissing(owner, name);
        }

        // thread-safe:
        public bool TryResolveConstant(bool autoload, string/*!*/ name, out object result) {
            var context = RubyContext;
            using (context.ClassHierarchyLocker()) {
                RubyGlobalScope autoloadScope = autoload ? GlobalScope : null;
                RubyScope scope = this;

                // lexical lookup first:
                RubyModule innerMostModule = null;
                do {
                    RubyModule module = scope.Module;

                    if (module != null) {
                        if (module.TryGetConstant(context, autoloadScope, name, out result)) {
                            return true;
                        }

                        // remember the module:
                        if (innerMostModule == null) {
                            innerMostModule = module;
                        }
                    }

                    scope = scope.Parent;
                } while (scope != null);

                // check the inner most module and it's base classes/mixins:
                if (innerMostModule != null && innerMostModule.TryResolveConstant(context, autoloadScope, name, out result)) {
                    return true;
                }

                return RubyContext.ObjectClass.TryResolveConstant(context, autoloadScope, name, out result);
            }
        }

        #region Debug View
#if !SILVERLIGHT
        internal sealed class DebugView {
            private readonly RubyScope/*!*/ _scope;
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            private readonly string/*!*/ _selfClassName;
            
            public DebugView(RubyScope/*!*/ scope) {
                Assert.NotNull(scope);
                _scope = scope;
                _selfClassName = _scope.RubyContext.GetImmediateClassOf(_scope._selfObject).GetDisplayName(_scope.RubyContext, true).ConvertToString();               
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public VariableView[]/*!*/ A0 {
                get {
                    List<VariableView> result = new List<VariableView>();
                    RubyScope scope = _scope;
                    while (true) {
                        foreach (KeyValuePair<SymbolId, object> variable in scope._frame.SymbolAttributes) {
                            string name = SymbolTable.IdToString(variable.Key);
                            if (!name.StartsWith("#")) {
                                string className = _scope.RubyContext.GetImmediateClassOf(variable.Value).GetDisplayName(_scope.RubyContext, true).ConvertToString();
                                if (scope != _scope) {
                                    name += " (outer)";
                                }
                                result.Add(new VariableView(name, variable.Value, className));
                            }
                        }

                        if (!scope.InheritsLocalVariables) {
                            break;
                        }
                        scope = scope.Parent;
                    }
                    return result.ToArray();
                }
            }

            [DebuggerDisplay("{A1}", Name = "self", Type = "{_selfClassName,nq}")]
            public object A1 {
                get { return _scope._selfObject; }
            }

            [DebuggerDisplay("{B}", Name = "MethodAttributes", Type = "")]
            public RubyMethodAttributes B {
                get { return _scope._methodAttributes; }
            }

            [DebuggerDisplay("{C}", Name = "ParentScope", Type = "")]
            public RubyScope C {
                get { return (RubyScope)_scope.Parent; }
            }

            [DebuggerDisplay("", Name = "RawVariables", Type = "")]
            public System.Collections.Hashtable/*!*/ D {
                get {
                    System.Collections.Hashtable result = new System.Collections.Hashtable();
                    foreach (KeyValuePair<SymbolId, object> variable in _scope._frame.SymbolAttributes) {
                        result.Add(variable.Key, variable.Value);
                    }
                    return result;
                }
            }

            [DebuggerDisplay("{_value}", Name = "{_name,nq}", Type = "{_valueClassName,nq}")]
            internal struct VariableView {
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private readonly string/*!*/ _name;
                [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
                private readonly object _value;
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private readonly string/*!*/ _valueClassName;

                public VariableView(string/*!*/ name, object value, string/*!*/ valueClassName) {
                    _name = name;
                    _value = value;
                    _valueClassName = valueClassName;
                }
            }
        }
#endif
        #endregion
    }

    public abstract class RubyClosureScope : RubyScope {
        // $+
        private MatchData _currentMatch; // TODO: per method scope and top level scope, not block scope

        // $_
        private object _lastInputLine; // TODO: per method scope and top level scope, not block scope

        // top scope:
        protected RubyClosureScope(RuntimeFlowControl/*!*/ runtimeFlowControl, object selfObject)
            : base(runtimeFlowControl, selfObject) {
        }

        // other scopes:
        protected RubyClosureScope(RubyScope/*!*/ parent, RuntimeFlowControl/*!*/ runtimeFlowControl, object selfObject)
            : base(parent, runtimeFlowControl, selfObject) {
        }

        protected override bool IsClosureScope {
            get { return true; }
        }

        public MatchData CurrentMatch {
            get { return _currentMatch; }
            set { _currentMatch = value; }
        }

        public object LastInputLine {
            get { return _lastInputLine; }
            set { _lastInputLine = value; }
        }

        internal MutableString GetCurrentMatchGroup(int index) {
            Debug.Assert(index >= 0);

            // we don't need to check index range, Groups indexer returns an unsuccessful group if out of range:
            Group group;
            if (_currentMatch != null && (group = _currentMatch.Groups[index]).Success) {
                return MutableString.Create(group.Value).TaintBy(_currentMatch.OriginalString);
            }

            return null;
        }

        internal MutableString GetCurrentMatchLastGroup() {
            if (_currentMatch != null) {
                // TODO: cache the last successful group index?
                for (int i = _currentMatch.Groups.Count - 1; i >= 0; i--) {
                    Group group = _currentMatch.Groups[i];
                    if (group.Success) {
                        return MutableString.Create(group.Value).TaintBy(_currentMatch.OriginalString);
                    }
                }
            }

            return null;
        }

#if TODO_DebugView
         [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            private readonly string/*!*/ _matchClassName;
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            private readonly string/*!*/ _lastInputLine;
var closureScope = scope as RubyClosureScope;
                if (closureScope != null) {
                    _matchClassName = _scope.RubyContext.GetImmediateClassOf(closureScope.CurrentMatch).GetDisplayName(true).ConvertToString();
                    _lastInputLine = _scope.RubyContext.GetImmediateClassOf(closureScope.LastInputLine).GetDisplayName(true).ConvertToString();
                }
                

            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            [DebuggerDisplay("{A2 != null ? A2.ToString() : \"nil\",nq}", Name = "$~", Type = "{_matchClassName,nq}")]
            public Match A2 {
                get { return (_scope._currentMatch != null) ? _scope._currentMatch.Match : null; }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            [DebuggerDisplay("{A2 != null ? A2.ToString() : \"nil\",nq}", Name = "$~", Type = "{_matchClassName,nq}")]
            public Match A2 {
                get { return (_scope._currentMatch != null) ? _scope._currentMatch.Match : null; }
            }
#endif
    }

    public sealed class RubyMethodScope : RubyClosureScope {
        private readonly RubyMethodInfo/*!*/ _method;
        private readonly Proc _blockParameter;
        
        public override ScopeKind Kind { get { return ScopeKind.Method; } }
        public override bool InheritsLocalVariables { get { return false; } }

        // Singleton module-function method shares this pointer with instance method. See RubyOps.DefineMethod for details.
        internal RubyMethodInfo Method {
            get { return _method; }
        }

        public Proc BlockParameter {
            get { return _blockParameter; }
        }

        internal RubyMethodScope(RubyScope/*!*/ parent, RubyMethodInfo/*!*/ method, Proc blockParameter, RuntimeFlowControl/*!*/ runtimeFlowControl, 
            object selfObject)
            : base(parent, runtimeFlowControl, selfObject) {
            _method = method;
            _blockParameter = blockParameter;
            MethodAttributes = RubyMethodAttributes.PublicInstance;
        }
    }

    public sealed class RubyModuleScope : RubyClosureScope {
        // TODO: readonly
        private RubyModule _module;
        private readonly bool _isEval;

        public override ScopeKind Kind { get { return ScopeKind.Module; } }
        public override bool InheritsLocalVariables { get { return _isEval; } }

        public override RubyModule Module { get { return _module; } }

        internal void SetModule(RubyModule/*!*/ module) { _module = module; }

        internal RubyModuleScope(RubyScope/*!*/ parent, RubyModule module, bool isEval,
            RuntimeFlowControl/*!*/ runtimeFlowControl, object selfObject)
            : base(parent, runtimeFlowControl, selfObject) {
            _module = module;
            _isEval = isEval;
            InLoop = parent.InLoop;
            InRescue = parent.InRescue;
            MethodAttributes = RubyMethodAttributes.PublicInstance;
        }
    }

    public sealed class RubyBlockScope : RubyScope {
        private readonly BlockParam _blockFlowControl;

        public override ScopeKind Kind { get { return ScopeKind.Block; } }
        public override bool InheritsLocalVariables { get { return true; } }

        public BlockParam BlockFlowControl {
            get { return _blockFlowControl; }
        }
        
        internal RubyBlockScope(RubyScope/*!*/ parent, RuntimeFlowControl/*!*/ runtimeFlowControl, BlockParam/*!*/ blockFlowControl, object selfObject)
            : base(parent, runtimeFlowControl, selfObject) {
            Assert.NotNull(blockFlowControl);
            _blockFlowControl = blockFlowControl;
        }
    }

    public class RubyTopLevelScope : RubyClosureScope {
        public override ScopeKind Kind { get { return ScopeKind.TopLevel; } }
        public override bool InheritsLocalVariables { get { return false; } }

        private readonly RubyGlobalScope/*!*/ _globalScope;
        private readonly RubyContext/*!*/ _context;
        private readonly RubyModule _methodLookupModule;
        private readonly RubyModule _wrappingModule;

        public RubyGlobalScope/*!*/ RubyGlobalScope {
            get {
                if (_globalScope == null) {
                    throw new InvalidOperationException("Empty scope has no global scope.");
                }
                return _globalScope; 
            }
        }

        internal new RubyContext/*!*/ RubyContext {
            get { return _context; }
        }

        public override RubyModule Module {
            get { return _wrappingModule; }            
        }

        /// <summary>
        /// Method and class lookup in top-level hosted scope behave like if it was instance_eval'd as a proc in MRI 1.8, i.e.
        /// methods are resolved in the singleton class of the main object.
        /// </summary>
        public RubyModule MethodLookupModule {
            get { return _methodLookupModule; }
        }

        internal RubyModule/*!*/ TopModuleOrObject {
            get { return _wrappingModule ?? _globalScope.Context.ObjectClass; }
        }

        // empty scope:
        internal RubyTopLevelScope(RubyContext/*!*/ context)
            : base(new RuntimeFlowControl(), null) {
            _context = context;
            Frame = _EmptyLocals;
        }

        internal RubyTopLevelScope(RubyGlobalScope/*!*/ globalScope, RubyModule scopeModule, RubyModule methodLookupModule,
            RuntimeFlowControl/*!*/ runtimeFlowControl, object selfObject)
            : base(runtimeFlowControl, selfObject) {
            Assert.NotNull(globalScope);
            _globalScope = globalScope;
            _context = globalScope.Context;
            _wrappingModule = scopeModule;
            _methodLookupModule = methodLookupModule;
        }

        #region Factories

        internal static RubyTopLevelScope/*!*/ CreateTopLevelScope(Scope/*!*/ globalScope, RubyContext/*!*/ context, bool isMain) {
            RubyGlobalScope rubyGlobalScope = context.InitializeGlobalScope(globalScope, false, false);

            RubyTopLevelScope scope = new RubyTopLevelScope(rubyGlobalScope, null, null, new RuntimeFlowControl(), rubyGlobalScope.MainObject);
            if (isMain) {
                scope.SetDebugName("top-main");
                context.ObjectClass.SetConstant("TOPLEVEL_BINDING", new Binding(scope));
            } else {
                scope.SetDebugName("top-required");
            }

            return scope;
        }

        internal static RubyTopLevelScope/*!*/ CreateHostedTopLevelScope(Scope/*!*/ globalScope, RubyContext/*!*/ context, bool bindGlobals) {
            RubyGlobalScope rubyGlobalScope = context.InitializeGlobalScope(globalScope, true, bindGlobals);

            // Reuse existing top-level scope if available:
            RubyTopLevelScope scope = rubyGlobalScope.TopLocalScope;
            if (scope == null) {
                scope = new RubyTopLevelScope(
                    rubyGlobalScope, null, bindGlobals ? rubyGlobalScope.MainSingleton : null, new RuntimeFlowControl(), rubyGlobalScope.MainObject
                );

                scope.SetDebugName(bindGlobals ? "top-level-bound" : "top-level");
                rubyGlobalScope.TopLocalScope = scope;
            } else {
                // If we reuse a local scope from previous execution all local variables are accessed dynamically.
                // Therefore we shouldn't have any new static local variables.
            }

            return scope;
        }

        internal static RubyTopLevelScope/*!*/ CreateWrappedTopLevelScope(Scope/*!*/ globalScope, RubyContext/*!*/ context) {
            RubyGlobalScope rubyGlobalScope = context.InitializeGlobalScope(globalScope, false, false);
            
            RubyModule module = context.CreateModule(null, null, null, null, null, null, null);
            object mainObject = new Object();
            context.CreateMainSingleton(mainObject, new[] { module });

            RubyTopLevelScope scope = new RubyTopLevelScope(rubyGlobalScope, module, null, new RuntimeFlowControl(), mainObject);
            scope.SetDebugName("top-level-wrapped");

            return scope;
        }

        // "method_missing" on main singleton in DLR Scope bound code.
        // Might be called via a site -> needs to be public in partial trust.
        public static object TopMethodMissing(RubyScope/*!*/ localScope, BlockParam block, object/*!*/ self, SymbolId name, [NotNull]params object[]/*!*/ args) {
            Assert.NotNull(localScope, self);
            Debug.Assert(!localScope.IsEmpty);
            Scope globalScope = localScope.GlobalScope.Scope;
            Debug.Assert(globalScope != null);

            // TODO: error when arguments non-empty, block != null, ...

            if (args.Length == 0) {
                object value;
                if (globalScope.TryGetName(name, out value)) {
                    return value;
                }

                string str = SymbolTable.IdToString(name);
                string unmangled = RubyUtils.TryUnmangleName(str);
                if (unmangled != null && globalScope.TryGetName(SymbolTable.StringToId(unmangled), out value)) {
                    return value;
                }

                if (str == "scope") {
                    return self;
                }
            } else if (args.Length == 1) {
                string str = SymbolTable.IdToString(name);
                if (str.LastCharacter() == '=') {
                    SymbolId plainName = SymbolTable.StringToId(str.Substring(0, str.Length - 1));
                    globalScope.SetName(plainName, args[0]);
                    return args[0];
                }
            }

            // TODO: call super
            throw RubyExceptions.CreateMethodMissing(localScope.RubyContext, self, SymbolTable.IdToString(name));
        }

        #endregion
    }
}
