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

namespace IronRuby.Runtime {

    public enum ScopeKind {
        TopLevel,
        Method,
        Module,
        Block
    }
        
#if !SILVERLIGHT
    [DebuggerTypeProxy(typeof(RubyScope.DebugView))]
#endif
    public abstract class RubyScope : CodeContext {
        internal static readonly LocalsDictionary _EmptyLocals = new LocalsDictionary(new IStrongBox[0], new SymbolId[0]);

        private readonly IAttributesCollection/*!*/ _frame;

        private RubyTopLevelScope/*!*/ _top;

        private object _selfObject;
        private RuntimeFlowControl/*!*/ _runtimeFlowControl; // TODO: merge?

        // set by private/public/protected/module_function
        private RubyMethodAttributes _methodAttributes;

        public abstract ScopeKind Kind { get; }
        public abstract bool InheritsLocalVariables { get; }

        public virtual RubyModule Module {
            get { return null; }
        }

        public object SelfObject {
            get { return _selfObject; }
            internal set { _selfObject = value; }
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

        public sealed override Scope GlobalScope {
            get { return _top.RubyGlobalScope.Scope; }
        }

        public RubyTopLevelScope/*!*/ Top {
            get { return _top; }
        }

        public RubyContext/*!*/ RubyContext {
            get { return (RubyContext)LanguageContext; }
        }

        public IAttributesCollection/*!*/ Frame {
            get { return _frame; }
        }

        // top scope:
        protected RubyScope(LanguageContext/*!*/ language, IAttributesCollection/*!*/ frame)
            : base(null, language, null) {
            _frame = frame;
            _top = (RubyTopLevelScope)this;
        }

        // other scopes:
        protected RubyScope(RubyScope/*!*/ parent, IAttributesCollection/*!*/ frame)
            : base(null, parent.Top.LanguageContext, parent) {
            _frame = frame;
            _top = parent.Top;
        }
        
        internal void Initialize(RuntimeFlowControl/*!*/ runtimeFlowControl, RubyMethodAttributes methodAttributes, object selfObject) {
            Assert.NotNull(runtimeFlowControl);

            _selfObject = selfObject;
            _runtimeFlowControl = runtimeFlowControl;
            _methodAttributes = methodAttributes;
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

                scope = (RubyScope)scope.Parent;
            }
        }

        internal object ResolveLocalVariable(string/*!*/ name) {
            RubyScope scope = this;
            while (true) {
                object result;
                if (scope.Frame.TryGetValue(SymbolTable.StringToId(name), out result)) {
                    return result;
                }

                if (!scope.InheritsLocalVariables) {
                    return null;
                }

                scope = (RubyScope)scope.Parent;
            }
        }

        public RubyModule/*!*/ GetInnerMostModule() {
            return GetInnerMostModule(false);
        }
        
        public RubyModule/*!*/ GetInnerMostModule(bool skipSingletons) {
            RubyScope scope = this;
            do {
                RubyModule result = scope.Module;
                if (result != null && (!skipSingletons || !result.IsSingletonClass)) {
                    return result;
                }
                scope = (RubyScope)scope.Parent;
            } while (scope != null);
            return RubyContext.ObjectClass;
        }

        public RubyMethodScope GetInnerMostMethodScope() {
            RubyScope scope = this;
            while (scope != null && scope.Kind != ScopeKind.Method) {
                scope = (RubyScope)scope.Parent;
            }
            return (RubyMethodScope)scope;
        }

        public RubyClosureScope/*!*/ GetInnerMostClosureScope() {
            RubyScope scope = this;
            while (scope != null && !scope.IsClosureScope) {
                scope = (RubyScope)scope.Parent;
            }
            return (RubyClosureScope)scope;
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
                        BlockParam blockParam = ((RubyBlockScope)scope).BlockParameter;
                        if (blockParam.SuperMethodName != null) {
                            declaringModule = blockParam.ModuleDeclaration;
                            methodName = blockParam.SuperMethodName;
                            self = scope.SelfObject;
                            return;
                        }
                        break;

                    case ScopeKind.TopLevel:
                        throw RubyOps.MakeTopLevelSuperException();
                }

                scope = (RubyScope)scope.Parent;
            }
        }

        public RubyScope/*!*/ GetMethodAttributesDefinitionScope() {
            RubyScope scope = this;
            while (true) {
                if (scope.Kind == ScopeKind.Block) {
                    BlockParam blockParam = ((RubyBlockScope)scope).BlockParameter;
                    if (blockParam.ModuleDeclaration != null && blockParam.SuperMethodName == null) {
                        return scope;
                    }
                } else {
                    return scope;
                }

                scope = (RubyScope)scope.Parent;
            }
        }

        internal RubyModule/*!*/ GetMethodDefinitionOwner() {
            RubyScope scope = this;
            bool skipBlocks = false;
            while (true) {
                Debug.Assert(scope != null);

                switch (scope.Kind) {
                    case ScopeKind.TopLevel:
                        return ((RubyTopLevelScope)scope).TopModuleOrObject;

                    case ScopeKind.Module:
                        Debug.Assert(scope.Module != null);
                        return scope.Module;

                    case ScopeKind.Method:
                        // MRI 1.8: skips module_evals above
                        // MRI 1.9: just continue search for a module or module_eval
                        skipBlocks = RubyContext.RubyOptions.Compatibility == RubyCompatibility.Ruby18;
                        break;

                    case ScopeKind.Block:
                        if (!skipBlocks) {
                            BlockParam blockParam = ((RubyBlockScope)scope).BlockParameter;
                            if (blockParam.ModuleDeclaration != null) {
                                return blockParam.ModuleDeclaration;
                            }
                        }
                        break;
                }

                scope = (RubyScope)scope.Parent;
            }
        }

        // dynamic dispatch to "const_missing" if not found
        public object ResolveConstant(bool autoload, string/*!*/ name) {
            object result;

            if (TryResolveConstant(autoload, name, out result)) {
                return result;
            }

            RubyUtils.CheckConstantName(name);
            return RubySites.ModuleConstMissing(RubyContext, GetInnerMostModule(), name);
        }

        public bool TryResolveConstant(bool autoload, string/*!*/ name, out object result) {
            Scope autoloadScope = autoload ? GlobalScope : null;
            RubyScope scope = this;

            // lexical lookup first:
            RubyModule innerMostModule = null;
            do {
                RubyModule module = scope.Module;

                if (module != null) {
                    if (module.TryGetConstant(autoloadScope, name, out result)) {
                        return true;
                    }

                    // remember the module:
                    if (innerMostModule == null) {
                        innerMostModule = module;
                    }
                }

                scope = (RubyScope)scope.Parent;
            } while (scope != null);

            // check the inner most module and it's base classes/mixins:
            if (innerMostModule != null && innerMostModule.TryResolveConstant(autoloadScope, name, out result)) {
                return true;
            }

            return RubyContext.ObjectClass.TryResolveConstant(autoloadScope, name, out result);
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
                _selfClassName = _scope.RubyContext.GetImmediateClassOf(_scope._selfObject).GetDisplayName(true).ConvertToString();               
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
                                string className = _scope.RubyContext.GetImmediateClassOf(variable.Value).GetDisplayName(true).ConvertToString();
                                if (scope != _scope) {
                                    name += " (outer)";
                                }
                                result.Add(new VariableView(name, variable.Value, className));
                            }
                        }

                        if (!scope.InheritsLocalVariables) {
                            break;
                        }
                        scope = (RubyScope)scope.Parent;
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
        protected RubyClosureScope(LanguageContext/*!*/ language, IAttributesCollection/*!*/ frame)
            : base(language, frame) {
        }

        // other scopes:
        protected RubyClosureScope(RubyScope/*!*/ parent, IAttributesCollection/*!*/ frame)
            : base(parent, frame) {
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

        internal RubyMethodScope(RubyScope/*!*/ parent, IAttributesCollection/*!*/ frame, RubyMethodInfo/*!*/ method, Proc blockParameter)
            : base(parent, frame) {
            _method = method;
            _blockParameter = blockParameter;
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

        internal RubyModuleScope(RubyScope/*!*/ parent, IAttributesCollection/*!*/ frame, RubyModule module, bool isEval)
            : base(parent, frame) {
            _module = module;
            _isEval = isEval;
        }
    }

    public sealed class RubyBlockScope : RubyScope {
        public override ScopeKind Kind { get { return ScopeKind.Block; } }
        public override bool InheritsLocalVariables { get { return true; } }

        // TODO: readonly
        private BlockParam _blockParam;

        public BlockParam BlockParameter {
            get { return _blockParam; }
            internal set { _blockParam = value; }
        }
        
        internal RubyBlockScope(RubyScope/*!*/ parent, IAttributesCollection/*!*/ frame)
            : base(parent, frame) {
        }
    }

    public sealed class RubyTopLevelScope : RubyClosureScope {
        public override ScopeKind Kind { get { return ScopeKind.TopLevel; } }
        public override bool InheritsLocalVariables { get { return false; } }

        private readonly GlobalScopeExtension/*!*/ _globalScope;
        private RubyModule _definitionsModule; 

        public GlobalScopeExtension/*!*/ RubyGlobalScope {
            get {
                if (_globalScope == null) {
                    throw new InvalidOperationException("Empty scope has no global scope.");
                }
                return _globalScope; 
            }
        }

        public override RubyModule Module {
            get { return _definitionsModule; }            
        }

        internal void SetModule(RubyModule/*!*/ value) {
            _definitionsModule = value;
        }

        internal RubyModule/*!*/ TopModuleOrObject {
            get { return _definitionsModule ?? _globalScope.Context.ObjectClass; }
        }

        // empty scope:
        internal RubyTopLevelScope(LanguageContext/*!*/ context)
            : base(context, _EmptyLocals) {
        }

        internal RubyTopLevelScope(GlobalScopeExtension/*!*/ globalScope, RubyModule definitionsModule, IAttributesCollection/*!*/ frame) 
            : base(globalScope.Context, frame) {
            Assert.NotNull(globalScope);
            _globalScope = globalScope;
            _definitionsModule = definitionsModule;
        }
        
        // TODO: might be called by MI.Invoke -> needs to be public in partial trust
        // method_missing on top-level DLR created scope:
        public static object TopMethodMissing(RubyScope/*!*/ scope, BlockParam block, object/*!*/ self, SymbolId name, [NotNull]params object[]/*!*/ args) {
            Assert.NotNull(scope, self);
            Debug.Assert(!scope.IsEmpty);
            Scope globalScope = scope.GlobalScope;
            Debug.Assert(globalScope != null);

            // TODO: error when arguments non-empty, block != null, ...

            object value;
            if (globalScope.TryGetName(name, out value)) {
                return value;
            }

            // TODO: assignment
            //if (args.Length == 1) {
            //    string str = SymbolTable.IdToString(name);
            //    if (str.Length > 0 && str[str.Length - 1] == '=') {
            //        SymbolId plainName = SymbolTable.StringToId(str.Substring(0, str.Length - 1));
            //        if (globalScope.ContainsName(plainName)) {
            //            globalScope.SetName(plainName, args[0]);
            //        }
            //        return value;
            //    }
            //}

            // TODO: call super
            throw RubyExceptions.CreateMethodMissing(scope.RubyContext, self, SymbolTable.IdToString(name));
        }

        // TODO:
        // TOPLEVEL_BINDING gets the Binding instance for DLR created scope:
        //internal static Binding/*!*/ GetTopLevelBinding(CodeContext/*!*/ context) {
        //    return RubyUtils.GetScope(context).GlobalScope.Binding;
        //}

    }
}
