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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Dynamic;
using System.Text;
using System.Threading;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using SpecialNameAttribute = System.Runtime.CompilerServices.SpecialNameAttribute;

namespace IronPython.Runtime {

    /// <summary>
    /// Created for a user-defined function.  
    /// </summary>
    [PythonType("function")]
    public sealed class PythonFunction : PythonTypeSlot, IWeakReferenceable, IMembersList, IDynamicMetaObjectProvider, ICodeFormattable, IOldDynamicObject {
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
        ///     10000000 - generator sys exc info
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
                ((ExpandListPosition != -1) ? 0x20000000 : 0) |
                (IsGeneratorWithExceptionHandling ? 0x10000000 : 0);
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

        #region IOldDynamicObject Members

        bool IOldDynamicObject.GetRule(OldDynamicAction action, CodeContext context, object[] args, RuleBuilder rule) {
            switch (action.Kind) {
                case DynamicActionKind.Call:
                    new FunctionBinderHelper(context, (OldCallAction)action, this, rule).MakeRule(ArrayUtils.RemoveFirst(args));
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Performs the actual work of binding to the function.
        /// 
        /// Overall this works by going through the arguments and attempting to bind all the outstanding known
        /// arguments - position arguments and named arguments which map to parameters are easy and handled
        /// in the 1st pass for GetArgumentsForRule.  We also pick up any extra named or position arguments which
        /// will need to be passed off to a kw argument or a params array.
        /// 
        /// After all the normal args have been assigned to do a 2nd pass in FinishArguments.  Here we assign
        /// a value to either a value from the params list, kw-dict, or defaults.  If there is ambiguity between
        /// this (e.g. we have a splatted params list, kw-dict, and defaults) we call a helper which extracts them
        /// in the proper order (first try the list, then the dict, then the defaults).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        class FunctionBinderHelper : BinderHelper<OldCallAction> {
            private PythonFunction _func;                           // the function we're calling
            private readonly RuleBuilder _rule;                     // the rule we're producing
            private ParameterExpression _dict, _params, _paramsLen; // splatted dictionary & params + the initial length of the params array, null if not provided.
            private List<Expression> _init;                         // a set of initialization code (e.g. creating a list for the params array)
            private Expression _error;                              // a custom error expression if the default needs to be overridden.
            private bool _extractedParams;                          // true if we needed to extract a parameter from the parameter list.
            private bool _extractedKeyword;                         // true if we needed to extract a parameter from the kw list.
            private Expression _userProvidedParams;                 // expression the user provided that should be expanded for params.
            private Expression _paramlessCheck;                     // tests when we have no parameters

            public FunctionBinderHelper(CodeContext context, OldCallAction action, PythonFunction function, RuleBuilder rule)
                : base(context, action) {
                _func = function;
                _rule = rule;
            }

            public void MakeRule(object[] args) {
                //Remove the passed in instance argument if present
                int instanceIndex = Action.Signature.IndexOf(ArgumentType.Instance);
                if (instanceIndex > -1) {
                    args = ArrayUtils.RemoveAt(args, instanceIndex);
                }

                _rule.Target = MakeTarget(args);
                _rule.Test = MakeTest();
            }

            /// <summary>
            /// Makes the target for our rule.
            /// </summary>
            private Expression MakeTarget(object[] args) {
                Expression[] invokeArgs = GetArgumentsForRule(args);

                if (invokeArgs != null) {
                    return AddInitialization(MakeFunctionInvoke(invokeArgs));
                }

                return MakeBadArgumentRule();
            }

            /// <summary>
            /// Makes the test for our rule.
            /// </summary>
            private Expression MakeTest() {
                if (!Action.Signature.HasKeywordArgument()) {
                    return MakeSimpleTest();
                }

                return MakeComplexTest();
            }

            /// <summary>
            /// Makes the test when we just have simple positional arguments.
            /// </summary>
            private Expression MakeSimpleTest() {
                return Ast.AndAlso(
                    _rule.MakeTypeTestExpression(_func.GetType(), 0),
                    Ast.AndAlso(
                        Ast.TypeIs(
                            Ast.Field(
                                Ast.Convert(_rule.Parameters[0], typeof(PythonFunction)),
                                typeof(PythonFunction),
                                "Target"
                            ),
                            _func.Target.GetType()
                        ),
                        Ast.Equal(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("FunctionGetCompatibility"),
                                Ast.Convert(_rule.Parameters[0], typeof(PythonFunction))
                            ),
                            Ast.Constant(_func.FunctionCompatibility))
                    )
                );
            }

            /// <summary>
            /// Makes the test when we have a keyword argument call or splatting.
            /// </summary>
            /// <returns></returns>
            private Expression MakeComplexTest() {
                if (_extractedKeyword) {
                    _func.EnsureID();

                    return Ast.AndAlso(
                        _rule.MakeTypeTestExpression(_func.GetType(), 0),
                        Ast.Equal(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("FunctionGetID"),
                                Ast.Convert(_rule.Parameters[0], typeof(PythonFunction))
                            ),
                            Ast.Constant(_func.FunctionID))
                    );
                }

                return Ast.AndAlso(
                    MakeSimpleTest(),
                    Ast.TypeIs(
                        Ast.Field(
                            GetFunctionParam(),
                            typeof(PythonFunction),
                            "Target"
                        ), 
                        _func.Target.GetType()
                    )
                );
            }

            /// <summary>
            /// Gets the array of expressions which correspond to each argument for the function.  These
            /// correspond with the function as it's defined in Python and must be transformed for our
            /// delegate type before being used.
            /// </summary>
            private Expression[] GetArgumentsForRule(object[] args) {
                Expression[] exprArgs = new Expression[_func.NormalArgumentCount + _func.ExtraArguments];
                List<Expression> extraArgs = null;
                Dictionary<string, Expression> namedArgs = null;
                int instanceIndex = Action.Signature.IndexOf(ArgumentType.Instance);

                // walk all the provided args and find out where they go...
                for (int i = 0; i < args.Length; i++) {
                    int parameterIndex = (instanceIndex == -1 || i < instanceIndex) ? i + 1 : i + 2;

                    switch (Action.Signature.GetArgumentKind(i)) {
                        case ArgumentType.Dictionary:
                            MakeDictionaryCopy(_rule.Parameters[parameterIndex]);
                            continue;

                        case ArgumentType.List:
                            _userProvidedParams = _rule.Parameters[parameterIndex];
                            continue;

                        case ArgumentType.Named:
                            _extractedKeyword = true;
                            bool foundName = false;
                            for (int j = 0; j < _func.NormalArgumentCount; j++) {
                                if (_func.ArgNames[j] == Action.Signature.GetArgumentName(i)) {
                                    if (exprArgs[j] != null) {
                                        // kw-argument provided for already provided normal argument.
                                        return null;
                                    }

                                    exprArgs[j] = _rule.Parameters[parameterIndex];
                                    foundName = true;
                                    break;
                                }
                            }

                            if (!foundName) {
                                if (namedArgs == null) namedArgs = new Dictionary<string, Expression>();
                                namedArgs[Action.Signature.GetArgumentName(i)] = _rule.Parameters[parameterIndex];
                            }
                            continue;
                    }

                    if (i < _func.NormalArgumentCount) {
                        exprArgs[i] = _rule.Parameters[parameterIndex];
                    } else {
                        if (extraArgs == null) extraArgs = new List<Expression>();
                        extraArgs.Add(_rule.Parameters[parameterIndex]);
                    }
                }

                if (!FinishArguments(exprArgs, extraArgs, namedArgs)) {
                    if (namedArgs != null && _func.ExpandDictPosition == -1) {
                        MakeUnexpectedKeywordError(namedArgs);
                    }

                    return null;
                }

                return GetArgumentsForTargetType(exprArgs);
            }

            /// <summary>
            /// Binds any missing arguments to values from params array, kw dictionary, or default values.
            /// </summary>
            private bool FinishArguments(Expression[] exprArgs, List<Expression> paramsArgs, Dictionary<string, Expression> namedArgs) {
                int noDefaults = _func.NormalArgumentCount - _func.Defaults.Length; // number of args w/o defaults

                for (int i = 0; i < _func.NormalArgumentCount; i++) {
                    if (exprArgs[i] != null) continue;

                    if (i < noDefaults) {
                        exprArgs[i] = ExtractNonDefaultValue(_func.ArgNames[i]);
                        if (exprArgs[i] == null) {
                            // can't get a value, this is an invalid call.
                            return false;
                        }
                    } else {
                        exprArgs[i] = ExtractDefaultValue(i, i - noDefaults);
                    }
                }

                if (!TryFinishList(exprArgs, paramsArgs) ||
                    !TryFinishDictionary(exprArgs, namedArgs))
                    return false;

                // add check for extra parameters.
                AddCheckForNoExtraParameters(exprArgs);

                return true;
            }

            /// <summary>
            /// Creates the argument for the list expansion parameter.
            /// </summary>
            private bool TryFinishList(Expression[] exprArgs, List<Expression> paramsArgs) {
                if (_func.ExpandListPosition != -1) {
                    if (_userProvidedParams != null) {
                        if (_params == null && paramsArgs == null) {
                            // we didn't extract any params, we can re-use a Tuple or
                            // make a single copy.
                            exprArgs[_func.ExpandListPosition] = Ast.Call(
                                typeof(PythonOps).GetMethod("GetOrCopyParamsTuple"),
                                AstUtils.Convert(_userProvidedParams, typeof(object))
                            );
                        } else {
                            // user provided a sequence to be expanded, and we may have used it,
                            // or we have extra args.
                            EnsureParams();

                            exprArgs[_func.ExpandListPosition] = Ast.Call(
                                typeof(PythonOps).GetMethod("MakeTupleFromSequence"),
                                AstUtils.Convert(_params, typeof(object))
                            );

                            if (paramsArgs != null) {
                                MakeParamsAddition(paramsArgs);
                            }
                        }
                    } else {
                        exprArgs[_func.ExpandListPosition] = MakeParamsTuple(paramsArgs);
                    }
                } else if (paramsArgs != null) {
                    // extra position args which are unused and no where to put them.
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Adds extra positional arguments to the start of the expanded list.
            /// </summary>
            private void MakeParamsAddition(List<Expression> paramsArgs) {
                _extractedParams = true;

                List<Expression> args = new List<Expression>(paramsArgs.Count + 1);
                args.Add(_params);
                args.AddRange(paramsArgs);

                EnsureInit();

                _init.Add(
                    AstUtils.ComplexCallHelper(
                        typeof(PythonOps).GetMethod("AddParamsArguments"),
                        args.ToArray()
                    )
                );
            }

            /// <summary>
            /// Creates the argument for the dictionary expansion parameter.
            /// </summary>
            private bool TryFinishDictionary(Expression[] exprArgs, Dictionary<string, Expression> namedArgs) {
                if (_func.ExpandDictPosition != -1) {
                    if (_dict != null) {
                        // used provided a dictionary to be expanded
                        exprArgs[_func.ExpandDictPosition] = _dict;
                        if (namedArgs != null) {
                            foreach (KeyValuePair<string, Expression> kvp in namedArgs) {
                                MakeDictionaryAddition(kvp);
                            }
                        }
                    } else {
                        exprArgs[_func.ExpandDictPosition] = MakeDictionary(namedArgs);
                    }
                } else if (namedArgs != null) {
                    // extra named args which are unused and no where to put them.
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Adds an unbound keyword argument into the dictionary.
            /// </summary>
            /// <param name="kvp"></param>
            private void MakeDictionaryAddition(KeyValuePair<string, Expression> kvp) {
                _init.Add(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("AddDictionaryArgument"),
                        AstUtils.Convert(GetFunctionParam(), typeof(PythonFunction)),
                        Ast.Constant(kvp.Key),
                        AstUtils.Convert(kvp.Value, typeof(object)),
                        AstUtils.Convert(_dict, typeof(IAttributesCollection))
                    )
                );
            }

            /// <summary>
            /// Adds a check to the last parameter (so it's evaluated after we've extracted
            /// all the parameters) to ensure that we don't have any extra params or kw-params
            /// when we don't have a params array or params dict to expand them into.
            /// </summary>
            private void AddCheckForNoExtraParameters(Expression[] exprArgs) {
                List<Expression> tests = new List<Expression>(3);

                // test we've used all of the extra parameters
                if (_func.ExpandListPosition == -1) {
                    if (_params != null) {
                        // we used some params, they should have gone down to zero...
                        tests.Add(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("CheckParamsZero"),
                                AstUtils.Convert(GetFunctionParam(), typeof(PythonFunction)),
                                _params
                            )
                        );
                    } else if (_userProvidedParams != null) {
                        // the user provided params, we didn't need any, and they should be zero
                        tests.Add(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("CheckUserParamsZero"),
                                AstUtils.Convert(GetFunctionParam(), typeof(PythonFunction)),
                                AstUtils.Convert(_userProvidedParams, typeof(object))
                            )
                        );
                    }
                }

                // test that we've used all the extra named arguments
                if (_func.ExpandDictPosition == -1 && _dict != null) {
                    tests.Add(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("CheckDictionaryZero"),
                            AstUtils.Convert(GetFunctionParam(), typeof(PythonFunction)),
                            AstUtils.Convert(_dict, typeof(IDictionary))
                        )
                    );
                }

                if (tests.Count != 0) {
                    if (exprArgs.Length != 0) {
                        // if we have arguments run the tests after the last arg is evaluated.
                        Expression last = exprArgs[exprArgs.Length - 1];
                        ParameterExpression temp = _rule.GetTemporary(last.Type, "$temp");
                        tests.Insert(0, Ast.Assign(temp, last));
                        tests.Add(temp);
                        exprArgs[exprArgs.Length - 1] = Ast.Block(tests.ToArray());
                    } else {
                        // otherwise run them right before the method call
                        _paramlessCheck = Ast.Block(tests.ToArray());
                    }
                }
            }

            /// <summary>
            /// Helper function to get a value (which has no default) from either the 
            /// params list or the dictionary (or both).
            /// </summary>
            private Expression ExtractNonDefaultValue(string name) {
                if (_userProvidedParams != null) {
                    // expanded params
                    if (_dict != null) {
                        // expanded params & dict
                        return ExtractFromListOrDictionary(name);
                    } else {
                        return ExtractNextParamsArg();
                    }
                } else if (_dict != null) {
                    // expanded dict
                    return ExtractDictionaryArgument(name);
                }

                // missing argument, no default, no expanded params or dict.
                return null;
            }

            /// <summary>
            /// Helper function to get the specified variable from the dictionary.
            /// </summary>
            private Expression ExtractDictionaryArgument(string name) {
                _extractedKeyword = true;

                return Ast.Call(
                    typeof(PythonOps).GetMethod("ExtractDictionaryArgument"),
                    AstUtils.Convert(GetFunctionParam(), typeof(PythonFunction)),              // function
                    Ast.Constant(name, typeof(string)),                                         // name
                    Ast.Constant(Action.Signature.ArgumentCount),                               // arg count
                    AstUtils.Convert(_dict, typeof(IAttributesCollection))    // dictionary
                );
            }

            /// <summary>
            /// Helper function to extract the variable from defaults, or to call a helper
            /// to check params / kw-dict / defaults to see which one contains the actual value.
            /// </summary>
            private Expression ExtractDefaultValue(int index, int dfltIndex) {
                if (_dict == null && _userProvidedParams == null) {
                    // we can pull the default directly
                    return Ast.Call(
                      typeof(PythonOps).GetMethod("FunctionGetDefaultValue"),
                      AstUtils.Convert(GetFunctionParam(), typeof(PythonFunction)),
                      Ast.Constant(dfltIndex)
                  );
                } else {
                    // we might have a conflict, check the default last.
                    if (_userProvidedParams != null) {
                        EnsureParams();
                    }
                    _extractedKeyword = true;
                    return Ast.Call(
                        typeof(PythonOps).GetMethod("GetFunctionParameterValue"),
                        AstUtils.Convert(GetFunctionParam(), typeof(PythonFunction)),
                        Ast.Constant(dfltIndex),
                        Ast.Constant(_func.ArgNames[index], typeof(string)),
                        VariableOrNull(_params, typeof(List)),
                        VariableOrNull(_dict, typeof(IAttributesCollection))
                    );
                }
            }

            /// <summary>
            /// Helper function to extract from the params list or dictionary depending upon
            /// which one has an available value.
            /// </summary>
            private Expression ExtractFromListOrDictionary(string name) {
                EnsureParams();

                _extractedKeyword = true;

                return Ast.Call(
                    typeof(PythonOps).GetMethod("ExtractAnyArgument"),
                    AstUtils.Convert(GetFunctionParam(), typeof(PythonFunction)),  // function
                    Ast.Constant(name, typeof(string)),                             // name
                    _paramsLen,                                    // arg count
                    _params,                                       // params list
                    AstUtils.Convert(_dict, typeof(IDictionary))  // dictionary
                );
            }

            private void EnsureParams() {
                if (!_extractedParams) {
                    Debug.Assert(_userProvidedParams != null);
                    MakeParamsCopy(_userProvidedParams);
                    _extractedParams = true;
                }
            }

            /// <summary>
            /// Helper function to extract the next argument from the params list.
            /// </summary>
            private Expression ExtractNextParamsArg() {
                if (!_extractedParams) {
                    MakeParamsCopy(_userProvidedParams);

                    _extractedParams = true;
                }

                return Ast.Call(
                    typeof(PythonOps).GetMethod("ExtractParamsArgument"),
                    AstUtils.Convert(GetFunctionParam(), typeof(PythonFunction)),  // function
                    Ast.Constant(Action.Signature.ArgumentCount),                   // arg count
                    _params                                        // list
                );
            }

            private Expression VariableOrNull(ParameterExpression var, Type type) {
                if (var != null) {
                    return AstUtils.Convert(
                        var,
                        type
                    );
                }
                return Ast.Constant(null, type);
            }

            /// <summary>
            /// Fixes up the argument list for the appropriate target delegate type.
            /// </summary>
            private Expression[] GetArgumentsForTargetType(Expression[] exprArgs) {
                Type target = _func.Target.GetType();
                if (target == typeof(IronPython.Compiler.CallTargetN) || target == typeof(IronPython.Compiler.GeneratorTargetN)) {
                    exprArgs = new Expression[] {
                        AstUtils.NewArrayHelper(typeof(object), exprArgs) 
                    };
                }

                return exprArgs;
            }

            /// <summary>
            /// Helper function to get the function argument strongly typed.
            /// </summary>
            private UnaryExpression GetFunctionParam() {
                return Ast.Convert(_rule.Parameters[0], _func.GetType());
            }

            /// <summary>
            /// Called when the user is expanding a dictionary - we copy the user
            /// dictionary and verify that it contains only valid string names.
            /// </summary>
            private void MakeDictionaryCopy(Expression userDict) {
                Debug.Assert(_dict == null);

                _dict = _rule.GetTemporary(typeof(PythonDictionary), "$dict");

                EnsureInit();
                _init.Add(
                    Ast.Assign(
                        _dict,
                        Ast.Call(
                            typeof(PythonOps).GetMethod("CopyAndVerifyDictionary"),
                            AstUtils.Convert(GetFunctionParam(), typeof(PythonFunction)),
                            AstUtils.Convert(userDict, typeof(IDictionary))
                        )
                    )
                );
            }

            /// <summary>
            /// Called when the user is expanding a params argument
            /// </summary>
            private void MakeParamsCopy(Expression userList) {
                Debug.Assert(_params == null);

                _params = _rule.GetTemporary(typeof(List), "$list");
                _paramsLen = _rule.GetTemporary(typeof(int), "$paramsLen");

                EnsureInit();

                _init.Add(
                    Ast.Assign(
                        _params,
                        Ast.Call(
                            typeof(PythonOps).GetMethod("CopyAndVerifyParamsList"),
                            AstUtils.Convert(GetFunctionParam(), typeof(PythonFunction)),
                            AstUtils.Convert(userList, typeof(object))
                        )
                    )
                );

                _init.Add(
                    Ast.Assign(_paramsLen,
                        Ast.Add(
                            Ast.Call(_params, typeof(List).GetMethod("__len__")),
                            Ast.Constant(Action.Signature.GetProvidedPositionalArgumentCount())
                        )
                    )
                );
            }

            /// <summary>
            /// Called when the user hasn't supplied a dictionary to be expanded but the
            /// function takes a dictionary to be expanded.
            /// </summary>
            private Expression MakeDictionary(Dictionary<string, Expression> namedArgs) {
                Debug.Assert(_dict == null);
                _dict = _rule.GetTemporary(typeof(PythonDictionary), "$dict");

                int count = 2;
                if (namedArgs != null) {
                    count += namedArgs.Count;
                }

                Expression[] dictCreator = new Expression[count];
                ParameterExpression dictRef = _dict;

                count = 0;
                dictCreator[count++] = Ast.Assign(
                    _dict,
                    Ast.Call(
                        typeof(PythonOps).GetMethod("MakeDict"),
                        Ast.Constant(0)
                    )
                );

                if (namedArgs != null) {
                    foreach (KeyValuePair<string, Expression> kvp in namedArgs) {
                        dictCreator[count++] = Ast.Call(
                            dictRef,
                            typeof(PythonDictionary).GetMethod("set_Item", new Type[] { typeof(object), typeof(object) }),
                            Ast.Constant(kvp.Key, typeof(object)),
                            AstUtils.Convert(kvp.Value, typeof(object))
                        );
                    }
                }

                dictCreator[count] = dictRef;

                return Ast.Block(dictCreator);
            }

            /// <summary>
            /// Helper function to create the expression for creating the actual tuple passed through.
            /// </summary>
            private Expression MakeParamsTuple(List<Expression> extraArgs) {
                if (extraArgs != null) {
                    return AstUtils.ComplexCallHelper(
                        typeof(PythonOps).GetMethod("MakeTuple"),
                        extraArgs.ToArray()
                    );
                }
                return Ast.Call(
                    typeof(PythonOps).GetMethod("MakeTuple"),
                    Ast.NewArrayInit(typeof(object))
                );
            }

            /// <summary>
            /// Generators follow different calling convention than 'regular' Python functions.
            /// When calling generator, we need to insert an additional argument to the call:
            /// an instance of the PythonGenerator. This is also result of the call.
            /// 
            /// DLR only provides generator lambdas that return IEnumerator so we are essentially
            /// wrapping it inside PythonGenerator.
            /// </summary>
            private bool IsGenerator(MethodInfo invoke) {
                ParameterInfo[] pis = invoke.GetParameters();
                return pis.Length > 0 && pis[0].ParameterType == typeof(PythonGenerator);
            }

            /// <summary>
            /// Creates the code to invoke the target delegate function w/ the specified arguments.
            /// </summary>
            private Expression MakeFunctionInvoke(Expression[] invokeArgs) {
                Type targetType = _func.Target.GetType();
                MethodInfo method = targetType.GetMethod("Invoke");

                // If calling generator, create the instance of PythonGenerator first
                // and add it into the list of arguments
                Expression pg = null;
                if (IsGenerator(method)) {
                    invokeArgs = ArrayUtils.Insert(
                        pg = _rule.GetTemporary(typeof(PythonGenerator), "$gen"),
                        invokeArgs
                    );
                }

                Expression invoke = AstUtils.SimpleCallHelper(
                    Ast.Convert(
                        Ast.Field(
                            GetFunctionParam(),
                            typeof(PythonFunction),
                            "Target"
                        ),
                        targetType
                    ),
                    method,
                    invokeArgs
                );

                int count = 1;
                if (_paramlessCheck != null) count++;
                if (pg != null) count += 2;

                if (count > 1) {
                    Expression[] comma = new Expression[count];

                    // Reset count for array fill
                    count = 0;

                    if (_paramlessCheck != null) {
                        comma[count++] = _paramlessCheck;
                    }

                    if (pg != null) {
                        // $gen = new PythonGenerator(context);
                        comma[count++] = Expression.Assign(
                            pg,
                            Expression.New(typeof(PythonGenerator).GetConstructor(new Type[] { typeof(CodeContext) }), _rule.Context)
                        );
                        // $gen.Next = <call DLR generator method>($gen, ....)
                        comma[count++] = Expression.Call(typeof(PythonOps).GetMethod("InitializePythonGenerator"), pg, invoke);

                        if (_func.IsGeneratorWithExceptionHandling) {
                            // function is a generator and it has try/except blocks.  We
                            // need to save/restore exception info but want to skip it
                            // for simple generators
                            pg = Ast.Call(
                                typeof(PythonOps).GetMethod("MarkGeneratorWithExceptionHandling"),
                                pg
                            );
                        }

                        // $gen is the value
                        comma[count++] = pg;
                    } else {
                        comma[count++] = invoke;
                    }

                    invoke = Expression.Block(comma);
                }

                return _rule.MakeReturn(Context.LanguageContext.Binder, invoke);
            }

            /// <summary>
            /// Appends the initialization code for the call to the function if any exists.
            /// </summary>
            private Expression AddInitialization(Expression body) {
                if (_init == null) return body;

                List<Expression> res = new List<Expression>(_init);
                res.Add(body);
                res.Add(Ast.Empty());
                return Ast.Block(res);
            }

            private void MakeUnexpectedKeywordError(Dictionary<string, Expression> namedArgs) {
                string name = null;
                foreach (string id in namedArgs.Keys) {
                    name = id;
                    break;
                }

                _error = Ast.Call(
                    typeof(PythonOps).GetMethod("UnexpectedKeywordArgumentError"),
                    AstUtils.Convert(GetFunctionParam(), typeof(PythonFunction)),
                    Ast.Constant(name, typeof(string))
                );
            }

            private Expression MakeBadArgumentRule() {
                if (_error != null) {
                    return _rule.MakeError(_error);
                }

                return _rule.MakeError(
                    Ast.Call(
                        typeof(PythonOps).GetMethod(Action.Signature.HasKeywordArgument() ? "BadKeywordArgumentError" : "FunctionBadArgumentError"),
                        AstUtils.Convert(GetFunctionParam(), typeof(PythonFunction)),
                        Ast.Constant(Action.Signature.GetProvidedPositionalArgumentCount())
                    )
                );
            }

            private void EnsureInit() {
                if (_init == null) _init = new List<Expression>();
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
