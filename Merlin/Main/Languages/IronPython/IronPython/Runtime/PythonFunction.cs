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
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Compiler;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

using SpecialNameAttribute = System.Runtime.CompilerServices.SpecialNameAttribute;

namespace IronPython.Runtime {

    /// <summary>
    /// Created for a user-defined function.  
    /// </summary>
    [PythonType("function")]
    public sealed partial class PythonFunction : PythonTypeSlot, IWeakReferenceable, IMembersList, IDynamicMetaObjectProvider, ICodeFormattable, Binding.IFastInvokable {
        private readonly CodeContext/*!*/ _context;     // the creating code context of the function
        [PythonHidden]
        public readonly Delegate Target;                // the target delegate to be invoked when called (should come from function code)
        private readonly FunctionAttributes _flags;     // * args, ** args, generator, and other attributes... 
        private readonly int _nparams;                  // number of arguments minus arg list / arg dict parameters
        private readonly string/*!*/[]/*!*/ _argNames;  // the names of each of the arguments on the method

        private string/*!*/ _name;                      // the name of the method
        private object[]/*!*/ _defaults;                // the default parameters of the method
        private object _module;                         // the name of the module that the function lives in
        private object _doc;                            // the documentation associated with the function
        private IAttributesCollection _dict;            // a dictionary to story arbitrary members on the function object

        private int _id, _compat;                       // ID/Compat flags used for testing in rules
        private FunctionCode _code;                     // the Python function code object.  Not currently used for much by us...        

        private static int[] _depth_fast = new int[20]; // hi-perf thread static data to avoid hitting a real thread static
        [ThreadStatic] private static int DepthSlow;    // current depth stored in a real thread static with fast depth runs out
        internal static int _MaximumDepth = 1001;       // maximum recursion depth allowed before we throw 
        internal static bool EnforceRecursion = false;  // true to enforce maximum depth, false otherwise
        [MultiRuntimeAware]
        private static int _CurrentId = 1;              // The current ID for functions which are called in complex ways.

        /// <summary>
        /// Python ctor - maps to function.__new__
        /// </summary>
        public PythonFunction(CodeContext context, FunctionCode code, IAttributesCollection globals) {
            throw new NotImplementedException();
        }

        internal PythonFunction(CodeContext/*!*/ context, string/*!*/ name, Delegate target, string[] argNames, object[] defaults, FunctionAttributes flags) {
            Assert.NotNull(context, name);
            Assert.NotNull(context.Scope);

            _name = name;
            _context = context;
            _argNames = argNames ?? ArrayUtils.EmptyStrings;
            _defaults = defaults ?? ArrayUtils.EmptyObjects;
            _flags = flags;
            _nparams = _argNames.Length;
            Target = target;
            _name = name;

            if ((flags & FunctionAttributes.KeywordDictionary) != 0) {
                _nparams--;
            }

            if ((flags & FunctionAttributes.ArgumentList) != 0) {
                _nparams--;
            }

            Debug.Assert(_defaults.Length <= _nparams);

            object modName;
            if (context.GlobalScope.Dict.TryGetValue(Symbols.Name, out modName)) {
                _module = modName;
            }
            
            _compat = CalculatedCachedCompat();
            _code = new FunctionCode(this);
        }

        #region Public APIs

        public object func_globals {
            get {
                return new PythonDictionary(new GlobalScopeDictionaryStorage(_context.Scope));
            }
        }

        public PythonTuple func_defaults {
            get {
                if (_defaults.Length == 0) return null;

                return new PythonTuple(_defaults);
            }
            set {
                if (value == null) {
                    _defaults = ArrayUtils.EmptyObjects;
                } else {
                    _defaults = value.ToArray();
                }
                _compat = CalculatedCachedCompat();
            }
        }

        public PythonTuple func_closure {
            get {
                Scope curScope = Context.Scope;
                List<ClosureCell> cells = new List<ClosureCell>();
                while (curScope != null) {
                    // Check for LocalsDictionary because we don't want to get
                    // the global scope here
                    LocalsDictionary funcEnv = curScope.Dict as LocalsDictionary;
                    if (funcEnv != null) {
                        foreach (SymbolId si in funcEnv.GetExtraKeys()) {
                            cells.Add(new ClosureCell(curScope.Dict[si]));
                        }
                    }

                    curScope = curScope.Parent;
                }

                if (cells.Count != 0) {
                    return PythonTuple.MakeTuple(cells.ToArray());
                }
                return null;
            }
            set {
                throw PythonOps.TypeError("readonly attribute");
            }
        }

        public string __name__ {
            get { return func_name; }
            set { func_name = value; }
        }

        public string func_name {
            get { return _name; }
            set {
                if (_name == null) throw PythonOps.TypeError("func_name must be set to a string object");
                _name = value;
            }
        }

        public IAttributesCollection __dict__ {
            get { return func_dict; }
            set { func_dict = value; }
        }

        public IAttributesCollection/*!*/ func_dict {
            get { return EnsureDict(); }
            set {
                if (value == null) throw PythonOps.TypeError("setting function's dictionary to non-dict");

                _dict = value;
            }
        }

        public object __doc__ {
            get { return _doc; }
            set { _doc = value; }
        }

        public object func_doc {
            get { return __doc__; }
            set { __doc__ = value; }
        }

        public object __module__ {
            get { return _module; }
            set { _module = value; }
        }

        public FunctionCode func_code {
            get {
                return _code; 
            }
            set {
                if (value == null) throw PythonOps.TypeError("func_code must be set to a code object");
                _code = value;
            }
        }

        public object __call__(CodeContext/*!*/ context, params object[] args) {
            return PythonCalls.Call(context, this, args);
        }

        public object __call__(CodeContext/*!*/ context, [ParamDictionary]IAttributesCollection dict, params object[] args) {
            return PythonCalls.CallWithKeywordArgs(context, this, args, dict);
        }

        #endregion

        #region Internal APIs

        internal string[] ArgNames {
            get { return _argNames; }
        }

        internal CodeContext Context {
            get {
                return _context;
            }
        }

        internal string GetSignatureString() {
            StringBuilder sb = new StringBuilder(__name__);
            sb.Append('(');
            for (int i = 0; i < _argNames.Length; i++) {
                if (i != 0) sb.Append(", ");

                if (i == ExpandDictPosition) {
                    sb.Append("**");
                } else if (i == ExpandListPosition) {
                    sb.Append("*");
                }

                sb.Append(ArgNames[i]);

                if (i < NormalArgumentCount) {
                    int noDefaults = NormalArgumentCount - Defaults.Length; // number of args w/o defaults
                    if (i - noDefaults >= 0) {
                        sb.Append('=');
                        sb.Append(PythonOps.Repr(Context, Defaults[i - noDefaults]));
                    }
                }
            }
            sb.Append(')');
            return sb.ToString();
        }

        /// <summary>
        /// Captures the # of args and whether we have kw / arg lists.  This
        /// enables us to share sites for simple calls (calls that don't directly
        /// provide named arguments or the list/dict params).
        /// </summary>
        internal int FunctionCompatibility {
            get {
                // TODO: Invalidate sites when EnforceRecursion changes instead of 
                // tracking this info in a compat flag.
                
                if (EnforceRecursion) {
                    return _compat | unchecked((int)0x80000000);
                }
                return _compat;
            }
        }

        /// <summary>
        /// Calculates the _compat value which is used for call-compatibility checks
        /// for simple calls.  Whenver any of the dependent values are updated this
        /// must be called again.
        /// 
        /// The dependent values include:
        ///     _nparams - this is readonly, and never requies an update
        ///     _defaults - the user can mutate this (func_defaults) and that forces
        ///                 an update
        ///     expand dict/list - based on nparams and flags, both read-only
        ///     
        /// Bits are allocated as:
        ///     00003fff - Normal argument count
        ///     0fffb000 - Default count
        ///     10000000 - unused
        ///     20000000 - expand list
        ///     40000000 - expand dict
        ///     80000000 - enforce recursion
        ///     
        /// Enforce recursion is added at runtime.
        /// </summary>
        private int CalculatedCachedCompat() {
            return NormalArgumentCount |
                Defaults.Length << 14 |
                ((ExpandDictPosition != -1) ? 0x40000000 : 0) |
                ((ExpandListPosition != -1) ? 0x20000000 : 0);
        }

        /// <summary>
        /// Generators w/ exception handling need to have some data stored
        /// on them so that we appropriately set/restore the exception state.
        /// </summary>
        internal bool IsGeneratorWithExceptionHandling {
            get {
                return ((_flags & (FunctionAttributes.CanSetSysExcInfo | FunctionAttributes.Generator)) == (FunctionAttributes.CanSetSysExcInfo | FunctionAttributes.Generator));
            }
        }

        /// <summary>
        /// Returns an ID for the function if one has been assigned, or zero if the
        /// function has not yet required the use of an ID.
        /// </summary>
        internal int FunctionID {
            get {
                return _id;
            }
        }

        /// <summary>
        /// Gets the position for the expand list argument or -1 if the function doesn't have an expand list parameter.
        /// </summary>
        internal int ExpandListPosition {
            get {
                if ((_flags & FunctionAttributes.ArgumentList) != 0) {
                    return _nparams;
                }

                return -1;
            }
        }

        /// <summary>
        /// Gets the position for the expand dictionary argument or -1 if the function doesn't have an expand dictionary parameter.
        /// </summary>
        internal int ExpandDictPosition {
            get {
                if ((_flags & FunctionAttributes.KeywordDictionary) != 0) {
                    if ((_flags & FunctionAttributes.ArgumentList) != 0) {
                        return _nparams + 1;
                    }
                    return _nparams;
                }
                return -1;
            }
        }

        /// <summary>
        /// Gets the number of normal (not params or kw-params) parameters.
        /// </summary>
        internal int NormalArgumentCount {
            get {
                return _nparams;
            }
        }

        /// <summary>
        /// Gets the number of extra arguments (params or kw-params)
        /// </summary>
        internal int ExtraArguments {
            get {
                if ((_flags & FunctionAttributes.ArgumentList) != 0) {
                    if ((_flags & FunctionAttributes.KeywordDictionary) != 0) {
                        return 2;
                    }
                    return 1;

                } else if ((_flags & FunctionAttributes.KeywordDictionary) != 0) {
                    return 1;
                }
                return 0;
            }
        }

        internal FunctionAttributes Flags {
            get {
                return _flags;
            }
        }

        internal object[] Defaults {
            get { return _defaults; }
        }

        internal Exception BadArgumentError(int count) {
            return BinderOps.TypeErrorForIncorrectArgumentCount(__name__, NormalArgumentCount, Defaults.Length, count, ExpandListPosition != -1, false);
        }

        internal Exception BadKeywordArgumentError(int count) {
            return BinderOps.TypeErrorForIncorrectArgumentCount(__name__, NormalArgumentCount, Defaults.Length, count, ExpandListPosition != -1, true);
        }

        internal static void SetRecursionLimit(int limit) {
            if (limit < 0) throw PythonOps.ValueError("recursion limit must be positive");
            PythonFunction.EnforceRecursion = (limit != Int32.MaxValue);
            PythonFunction._MaximumDepth = limit;
        }

        #endregion
       
        #region Custom member lookup operators

        [SpecialName]
        public void SetMemberAfter(CodeContext context, string name, object value) {
            EnsureDict();

            _dict[SymbolTable.StringToId(name)] = value;
        }

        [SpecialName]
        public object GetBoundMember(CodeContext context, string name) {
            object value;
            if (_dict != null && _dict.TryGetValue(SymbolTable.StringToId(name), out value)) {
                return value;
            }
            return OperationFailed.Value;
        }

        [SpecialName]
        public bool DeleteMember(CodeContext context, string name) {
            switch (name) {
                case "func_dict":
                case "__dict__":
                    throw PythonOps.TypeError("function's dictionary may not be deleted");
                case "__doc__":
                case "func_doc":
                    _doc = null;
                    return true;
                case "func_defaults":
                    _defaults = ArrayUtils.EmptyObjects;
                    _compat = CalculatedCachedCompat();
                    return true;
            }

            if (_dict == null) return false;

            return _dict.Remove(SymbolTable.StringToId(name));
        }

        IList<object> IMembersList.GetMemberNames(CodeContext context) {
            List list;
            if (_dict == null) {
                list = PythonOps.MakeList();
            } else {
                list = PythonOps.MakeListFromSequence(_dict);
            }
            list.AddNoLock(SymbolTable.IdToString(Symbols.Module));

            list.extend(TypeCache.Function.GetMemberNames(context, this));
            return list;
        }

        #endregion

        #region IWeakReferenceable Members

        WeakRefTracker IWeakReferenceable.GetWeakRef() {
            if (_dict != null) {
                object weakRef;
                if (_dict.TryGetValue(Symbols.WeakRef, out weakRef)) {
                    return weakRef as WeakRefTracker;
                }
            }
            return null;
        }

        bool IWeakReferenceable.SetWeakRef(WeakRefTracker value) {
            EnsureDict();
            _dict[Symbols.WeakRef] = value;
            return true;
        }

        void IWeakReferenceable.SetFinalizer(WeakRefTracker value) {
            ((IWeakReferenceable)this).SetWeakRef(value);
        }

        #endregion

        #region Private APIs

        private IAttributesCollection EnsureDict() {
            if (_dict == null) {
                Interlocked.CompareExchange(ref _dict, (IAttributesCollection)PythonDictionary.MakeSymbolDictionary(), null);
            }
            return _dict;
        }

        internal static int Depth {
            get {
                // ManagedThreadId starts at 1 and increases as we get more threads.
                // Therefore we keep track of a limited number of threads in an array
                // that only gets created once, and we access each of the elements
                // from only a single thread.
                uint tid = (uint)Thread.CurrentThread.ManagedThreadId;

                return (tid < _depth_fast.Length) ? _depth_fast[tid] : DepthSlow;
            }
            set {
                uint tid = (uint)Thread.CurrentThread.ManagedThreadId;

                if (tid < _depth_fast.Length)
                    _depth_fast[tid] = value;
                else
                    DepthSlow = value;
            }
        }

        internal void EnsureID() {
            if (_id == 0) {
                Interlocked.CompareExchange(ref _id, Interlocked.Increment(ref _CurrentId), 0);
            }
        }

        #endregion

        #region PythonTypeSlot Overrides

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            value = new Method(this, instance, owner);
            return true;
        }

        internal override bool GetAlwaysSucceeds {
            get {
                return true;
            }
        }

        #endregion

        #region ICodeFormattable Members

        public string/*!*/ __repr__(CodeContext/*!*/ context) {
            return string.Format("<function {0} at {1}>", func_name, PythonOps.HexId(this));
        }

        #endregion

        #region IDynamicMetaObjectProvider Members

        DynamicMetaObject/*!*/ IDynamicMetaObjectProvider.GetMetaObject(Expression/*!*/ parameter) {
            return new Binding.MetaPythonFunction(parameter, BindingRestrictions.Empty, this);
        }

        #endregion
    }

    [PythonType("cell")]
    public sealed class ClosureCell : ICodeFormattable, IValueEquality {
        private object _value;

        internal ClosureCell(object value) {
            _value = value;
        }

        public object cell_contents {
            get {
                return _value;
            }
        }

        #region ICodeFormattable Members

        public string/*!*/ __repr__(CodeContext/*!*/ context) {
            return String.Format("<cell at {0}: {1} object at {2}>",
                IdDispenser.GetId(this),
                PythonTypeOps.GetName(_value),
                IdDispenser.GetId(_value));
        }

        #endregion

        #region IValueEquality Members

        int IValueEquality.GetValueHashCode() {
            throw PythonOps.TypeError("unhashable type: cell");
        }

        bool IValueEquality.ValueEquals(object other) {
            return __cmp__(other) == 0;
        }

        #endregion

        public int __cmp__(object other) {
            ClosureCell cc = other as ClosureCell;
            if (cc == null) throw PythonOps.TypeError("cell.__cmp__(x,y) expected cell, got {0}", PythonTypeOps.GetName(other));

            return PythonOps.Compare(_value, cc._value);
        }
    }
}
