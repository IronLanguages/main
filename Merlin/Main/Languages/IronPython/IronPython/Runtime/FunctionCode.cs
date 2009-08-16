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
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Compiler;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime {
    /// <summary>
    /// Represents a piece of code.  This can reference either a CompiledCode
    /// object or a Function.   The user can explicitly call FunctionCode by
    /// passing it into exec or eval.
    /// </summary>
    [PythonType("code")]
    public class FunctionCode : IExpressionSerializable {
        [PythonHidden]
        public Delegate Target;                                     // the current target for the function.  This can change based upon adaptive compilation, recursion enforcement, and tracing.
        private Delegate _normalDelegate;                           // the normal delegate - this can be a compiled or interpreted delegate.

        private readonly ScriptCode _code;
        private readonly string _filename;                          // the filename that created the function co
        private readonly FunctionAttributes _flags;                 // future division, generator
        private LambdaExpression _lambda;                           // the original DLR lambda that contains the code
        private readonly bool _shouldInterpret;                     // true if we should interpret the code
        private readonly bool _debuggable;                          // true if the code should be compiled as debuggable code
        private readonly SourceSpan _span;                          // the source span for the source code
        private readonly string[] _argNames;                        // the argument names for the function
        private readonly PythonTuple _closureVars;                  // a tuple of variable names which have been closed over
        private readonly string _name;                              // the name of the function
        internal readonly string _initialDoc;                       // the initial doc string
        private PythonTuple _varnames;                              // lazily computed variable names

        // debugging/tracing support
        private readonly Dictionary<int, bool> _handlerLocations; // list of exception handler locations for debugging
        private readonly Dictionary<int, Dictionary<int, bool>> _loopAndFinallyLocations; // list of loop and finally locations for debugging
        private LambdaExpression _tracingLambda;                    // the transformed lambda used for tracing/debugging
        private Delegate _tracingDelegate;                          // the delegate used for tracing/debugging, if one has been created.  This can be interpreted or compiled.

        /// <summary>
        /// This is both the lock that is held while enumerating the threads or updating the thread accounting
        /// information.  It's also a marker CodeList which is put in place when we are enumerating the thread
        /// list and all additions need to block.
        /// 
        /// This lock is also acquired whenever we need to calculate how a function's delegate should be created 
        /// so that we don't race against sys.settrace/sys.setprofile.
        /// </summary>
        private static CodeList _CodeCreateAndUpdateDelegateLock = new CodeList();

        internal FunctionCode() {
            _closureVars = PythonTuple.EMPTY;
        }

        internal FunctionCode(string name, string filename, int lineNo) {
            _name = name;
            _filename = filename;
            _span = new SourceSpan(new SourceLocation(0, lineNo == 0 ? 1 : lineNo, 1), new SourceLocation(0, lineNo == 0 ? 1 : lineNo, 1));
            _closureVars = PythonTuple.EMPTY;
        }

        /// <summary>
        /// Constructor used to create a FunctionCode for code that's been serialized to disk.  
        /// 
        /// Code constructed this way cannot be interpreted or debugged using sys.settrace/sys.setprofile.
        /// 
        /// Function codes created this way do support recursion enforcement and are therefore registered in the global function code registry.
        /// </summary>
        internal FunctionCode(PythonContext context, Delegate code, string name, string documentation, string[] argNames, FunctionAttributes flags, SourceSpan span, string path, string[] closureVars) {
            _name = name;
            _span = span;
            _initialDoc = documentation;
            _argNames = argNames;
            _flags = flags;
            _span = span;
            _filename = path;
            if (_closureVars != null) {
                _closureVars = PythonTuple.MakeTuple((object[])closureVars);
            } else {
                _closureVars = PythonTuple.EMPTY;
            }

            _normalDelegate = code;

            // need to take this lock to ensure sys.settrace/sys.setprofile is not actively changing
            lock (_CodeCreateAndUpdateDelegateLock) {
                Target = AddRecursionCheck(context, code);
            }

            RegisterFunctionCode(context);
        }

        /// <summary>
        /// Constructor to create a FunctionCode at runtime.
        /// 
        /// Code constructed this way supports both being interpreted and debugged.  When necessary the code will
        /// be re-compiled or re-interpreted for that specific purpose.
        /// 
        /// Function codes created this way do support recursion enforcement and are therefore registered in the global function code registry.
        /// 
        /// the initial delegate provided here should NOT be the actual code.  It should always be a delegate which updates our Target lazily.
        /// </summary>
        internal FunctionCode(PythonContext context, Delegate initialDelegate, LambdaExpression code, string name, string documentation, string[] argNames, FunctionAttributes flags, SourceSpan span, string path, bool isDebuggable, bool shouldInterpret, IList<SymbolId> closureVars, Dictionary<int, Dictionary<int, bool>> loopLocations, Dictionary<int, bool> handlerLocations) {
            _lambda = code;
            _name = name;
            _span = span;
            _initialDoc = documentation;
            _argNames = argNames;
            _flags = flags;
            _span = span;
            _filename = path ?? "<string>";

            _shouldInterpret = shouldInterpret;
            if (closureVars != null) {
                _closureVars = PythonTuple.MakeTuple(SymbolTable.IdsToStrings(closureVars));
            } else {
                _closureVars = PythonTuple.EMPTY;
            }
            _handlerLocations = handlerLocations;
            _loopAndFinallyLocations = loopLocations;

            _debuggable = isDebuggable;

            Target = initialDelegate;
            RegisterFunctionCode(context);
        }

        internal FunctionCode(ScriptCode code, CompileFlags compilerFlags, string fileName)
            : this(code) {

            if ((compilerFlags & CompileFlags.CO_FUTURE_DIVISION) != 0) {
                _flags |= FunctionAttributes.FutureDivision;
            }
            _filename = fileName;
            _closureVars = PythonTuple.EMPTY;
        }

        internal FunctionCode(ScriptCode code) {
            _code = code;
            _closureVars = PythonTuple.EMPTY;
        }

        /// <summary>
        /// Registers the current function code in our global weak list of all function codes.
        /// 
        /// The weak list can be enumerated with GetAllCode().
        /// 
        /// Ultimately there are 3 types of threads we care about races with:
        ///     1. Other threads which are registering function codes
        ///     2. Threads calling sys.settrace which require the world to stop and get updated
        ///     3. Threads running cleanup (thread pool thread, or call to gc.collect).
        ///     
        /// The 1st two must have perfect synchronization.  We cannot have a thread registering
        /// a new function which another thread is trying to update all of the functions in the world.  Doing
        /// so would mean we could miss adding tracing to a thread.   
        /// 
        /// But the cleanup thread can run in parallel to either registrying or sys.settrace.  The only
        /// thing it needs to take a lock for is updating our accounting information about the
        /// number of code objects are alive.
        /// </summary>
        private void RegisterFunctionCode(PythonContext context) {
            if (_lambda == null) {
                return;
            }

            WeakReference codeRef = new WeakReference(this, true);
            CodeList prevCode;
            lock (_CodeCreateAndUpdateDelegateLock) {
                Debug.Assert(context._allCodes != _CodeCreateAndUpdateDelegateLock);
                // we do an interlocked operation here because this can run in parallel w/ the CodeCleanup thread.  The
                // lock only prevents us from running in parallel w/ an update to all of the functions in the world which
                // needs to be synchronized.
                do {
                    prevCode = context._allCodes;
                } while (Interlocked.CompareExchange(ref context._allCodes, new CodeList(codeRef, prevCode), prevCode) != prevCode);

                if (context._codeCount++ == context._nextCodeCleanup) {
                    // run cleanup of codes on another thread
                    CleanFunctionCodes(context, false);
                }
            }
        }

        internal static void CleanFunctionCodes(PythonContext context, bool synchronous) {
            if (synchronous) {
                CodeCleanup(context);
            } else {
                ThreadPool.QueueUserWorkItem(CodeCleanup, context);
            }
        }

        /// <summary>
        /// Enumerates all function codes for updating the current type of targets we generate.
        /// 
        /// While enumerating we hold a lock so that users cannot change sys.settrace/sys.setprofile
        /// until the lock is released.
        /// </summary>
        private static IEnumerable<FunctionCode> GetAllCode(PythonContext context) {
            // only a single thread can enumerate the current FunctionCodes at a time.
            lock (_CodeCreateAndUpdateDelegateLock) {
                CodeList curCodeList = Interlocked.Exchange(ref context._allCodes, _CodeCreateAndUpdateDelegateLock);
                Debug.Assert(curCodeList != _CodeCreateAndUpdateDelegateLock);

                CodeList initialCode = curCodeList;

                try {
                    while (curCodeList != null) {
                        WeakReference codeRef = curCodeList.Code;
                        FunctionCode target = (FunctionCode)codeRef.Target;

                        if (target != null) {
                            yield return target;
                        }

                        curCodeList = curCodeList.Next;
                    }
                } finally {
                    Interlocked.Exchange(ref context._allCodes, curCodeList);
                }
            }
        }

        internal static void UpdateAllCode(PythonContext context) {
            foreach (FunctionCode fc in GetAllCode(context)) {
                fc.UpdateDelegate(context, false);
            }
        }

        private static void CodeCleanup(object state) {
            PythonContext context = (PythonContext)state;

            // only allow 1 thread at a time to do cleanup (other threads can continue adding)
            lock (context._codeCleanupLock) {
                // the bulk of the work is in scanning the list, this proceeeds lock free
                int removed = 0, kept = 0, origCount = context._codeCount;
                CodeList prev = null;
                CodeList cur = GetRootCodeNoUpdating(context._allCodes);

                while (cur != null) {
                    if (!cur.Code.IsAlive) {
                        if (prev == null) {
                            if (Interlocked.CompareExchange(ref context._allCodes, cur.Next, cur) != cur) {
                                // someone else snuck in and added a new code entry, spin and try again.
                                cur = GetRootCodeNoUpdating(context._allCodes);
                                continue;
                            }
                            cur = cur.Next;
                            removed++;
                            continue;
                        } else {
                            // remove from the linked list, we're the only one who can change this.
                            Debug.Assert(prev.Next == cur);
                            removed++;
                            cur = prev.Next = cur.Next;
                            continue;
                        }
                    } else {
                        kept++;
                    }
                    prev = cur;
                    cur = cur.Next;
                }

                // finally update our bookkeeping statistics which requires locking but is fast.
                lock (_CodeCreateAndUpdateDelegateLock) {
                    // calculate the next cleanup, we want each pass to remove ~50% of all entries
                    const double removalGoal = .50;

                    if (context._codeCount == 0) {
                        // somehow we would have had to queue a bunch of function codes, have 1 thread
                        // clean them up all up, and a 2nd queued thread waiting to clean them up as well.
                        // At the same time there would have to be no live functions defined which means
                        // we're executing top-level code which is causing this to happen.
                        context._nextCodeCleanup = 200;
                        return;
                    }
                    //Console.WriteLine("Removed {0} entries, {1} remain", removed, context._codeCount);

                    Debug.Assert(removed <= context._codeCount);
                    double pctRemoved = (double)removed / (double)context._codeCount; // % of code removed
                    double targetRatio = pctRemoved / removalGoal;                    // how we need to adjust our last goal

                    // update the total and next node cleanup
                    int newCount = Interlocked.Add(ref context._codeCount, -removed);
                    Debug.Assert(newCount >= 0);

                    // set the new target for cleanup
                    int nextCleanup = targetRatio != 0 ? newCount + (int)(context._nextCodeCleanup / targetRatio) : -1;
                    if (nextCleanup > 0) {
                        // we're making good progress, use the newly calculated next cleanup point
                        context._nextCodeCleanup = nextCleanup;
                    } else {
                        // none or very small amount cleaned up, just schedule a cleanup for sometime in the future.
                        context._nextCodeCleanup += 500;
                    }

                    Debug.Assert(context._nextCodeCleanup >= context._codeCount, String.Format("{0} vs {1} ({2})", context._nextCodeCleanup, context._codeCount, targetRatio));
                }
            }
        }

        private static CodeList GetRootCodeNoUpdating(CodeList cur) {
            while (cur == _CodeCreateAndUpdateDelegateLock) {
                lock (_CodeCreateAndUpdateDelegateLock) {
                    // wait until enumerating thread is done, but it's alright
                    // if we got cur and then an enumeration started (because we'll
                    // just clear entries out)
                }

                cur = _CodeCreateAndUpdateDelegateLock;
            }
            return cur;
        }

        internal SourceSpan Span {
            get {
                return _span;
            }
        }

        internal string[] ArgNames {
            get {
                return _argNames;
            }
        }

        internal FunctionAttributes Flags {
            get {
                return _flags;
            }
        }


        #region Public constructors

        /*
        /// <summary>
        /// Standard python siganture
        /// </summary>
        /// <param name="argcount"></param>
        /// <param name="nlocals"></param>
        /// <param name="stacksize"></param>
        /// <param name="flags"></param>
        /// <param name="codestring"></param>
        /// <param name="constants"></param>
        /// <param name="names"></param>
        /// <param name="varnames"></param>
        /// <param name="filename"></param>
        /// <param name="name"></param>
        /// <param name="firstlineno"></param>
        /// <param name="nlotab"></param>
        /// <param name="freevars"></param>
        /// <param name="callvars"></param>
        public FunctionCode(int argcount, int nlocals, int stacksize, int flags, string codestring, object constants, Tuple names, Tuple varnames, string filename, string name, int firstlineno, object nlotab, [DefaultParameterValue(null)]object freevars, [DefaultParameterValue(null)]object callvars) {
        }*/

        #endregion

        #region Public Python API Surface

        public object co_varnames {
            get {
                if (_varnames == null) {
                    _varnames = GetArgNames();
                }
                return _varnames;
            }
        }

        public int co_argcount {
            get {
                int argCnt = ArgNames.Length;
                if ((_flags & FunctionAttributes.ArgumentList) != 0) argCnt--;
                if ((_flags & FunctionAttributes.KeywordDictionary) != 0) argCnt--;
                return argCnt;
            }
        }

        public object co_cellvars {
            get {
                throw PythonOps.NotImplementedError("");
            }
        }

        public object co_code {
            get {
                throw PythonOps.NotImplementedError("");
            }
        }

        public PythonTuple co_consts {
            get {
                if (this._initialDoc != null) {
                    return PythonTuple.MakeTuple(_initialDoc, null);
                }

                return PythonTuple.MakeTuple((object)null);
            }
        }

        public string co_filename {
            get {
                return _filename;
            }
        }

        public int co_firstlineno {
            get {
                return _span.Start.Line;
            }
        }

        public int co_flags {
            get {
                return (int)_flags;
            }
        }

        public object co_freevars {
            get {
                return _closureVars;
            }
        }

        public object co_lnotab {
            get {
                throw PythonOps.NotImplementedError("");
            }
        }

        public string co_name {
            get {
                return _name;
            }
        }

        public object co_names {
            get {
                throw PythonOps.NotImplementedError("");
            }
        }

        public object co_nlocals {
            get {
                throw PythonOps.NotImplementedError("");
            }
        }

        public object co_stacksize {
            get {
                throw PythonOps.NotImplementedError("");
            }
        }
        #endregion

        #region Internal API Surface

        internal LambdaExpression Code {
            get {
                return _lambda;
            }
            set {
                _lambda = value;
            }
        }

        internal object Call(CodeContext/*!*/ context, Scope/*!*/ scope) {
            if (_code != null) {
                return _code.Run(scope);
            }

            if (_closureVars != PythonTuple.EMPTY) {
                throw PythonOps.TypeError("cannot exec code object that contains free variables: {0}", _closureVars.__repr__(context));
            }

            if (Target == null) {
                UpdateDelegate(context.LanguageContext, true);
            }

            Func<CodeContext, CodeContext> classTarget = Target as Func<CodeContext, CodeContext>;
            if (classTarget != null) {
                return classTarget(new CodeContext(scope, context.LanguageContext));
            }

            Func<CodeContext, object> moduleCode = Target as Func<CodeContext, object>;
            if (moduleCode != null) {
                return moduleCode(new CodeContext(scope, context.LanguageContext));
            }

            Func<object> optimizedModuleCode = Target as Func<object>;
            if (optimizedModuleCode != null) {
                return optimizedModuleCode();
            }

            var func = new PythonFunction(context, this, null, ArrayUtils.EmptyObjects, new MutableTuple<object>());
            CallSite<Func<CallSite, CodeContext, PythonFunction, object>> site = PythonContext.GetContext(context).FunctionCallSite;
            return site.Target(site, context, func);
        }

        #endregion

        #region Private helper functions

        private PythonTuple GetArgNames() {
            if (_code != null) return PythonTuple.MakeTuple();

            List<string> names = new List<string>();
            List<PythonTuple> nested = new List<PythonTuple>();


            for (int i = 0; i < ArgNames.Length; i++) {
                if (ArgNames[i].IndexOf('#') != -1 && ArgNames[i].IndexOf('!') != -1) {
                    names.Add("." + (i * 2));
                    // TODO: need to get local variable names here!!!
                    //nested.Add(FunctionDefinition.DecodeTupleParamName(func.ArgNames[i]));
                } else {
                    names.Add(ArgNames[i]);
                }
            }

            for (int i = 0; i < nested.Count; i++) {
                ExpandArgsTuple(names, nested[i]);
            }
            return PythonTuple.Make(names);
        }

        private void ExpandArgsTuple(List<string> names, PythonTuple toExpand) {
            for (int i = 0; i < toExpand.__len__(); i++) {
                if (toExpand[i] is PythonTuple) {
                    ExpandArgsTuple(names, toExpand[i] as PythonTuple);
                } else {
                    names.Add(toExpand[i] as string);
                }
            }
        }

        #endregion

        public override bool Equals(object obj) {
            FunctionCode other = obj as FunctionCode;
            if (other == null) return false;

            if (_code != null) {
                return _code == other._code;
            }

            return _lambda == other._lambda;
        }

        public override int GetHashCode() {
            if (_code != null) {
                return _code.GetHashCode();
            }

            return _lambda.GetHashCode();
        }

        public int __cmp__(CodeContext/*!*/ context, [NotNull]FunctionCode/*!*/  other) {
            if (other == this) {
                return 0;
            }

            long lres = IdDispenser.GetId(this) - IdDispenser.GetId(other);
            return lres > 0 ? 1 : -1;
        }

        // these are present in CPython but always return NotImplemented.
        [return: MaybeNotImplemented]
        [Python3Warning("code inequality comparisons not supported in 3.x")]
        public static NotImplementedType operator >(FunctionCode self, FunctionCode other) {
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [Python3Warning("code inequality comparisons not supported in 3.x")]
        public static NotImplementedType operator <(FunctionCode self, FunctionCode other) {
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [Python3Warning("code inequality comparisons not supported in 3.x")]
        public static NotImplementedType operator >=(FunctionCode self, FunctionCode other) {
            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [Python3Warning("code inequality comparisons not supported in 3.x")]
        public static NotImplementedType operator <=(FunctionCode self, FunctionCode other) {
            return PythonOps.NotImplemented;
        }

        /// <summary>
        /// Called the 1st time a function is invoked by our OriginalCallTarget* methods
        /// over in PythonCallTargets.  This computes the real delegate which needs to be
        /// created for the function.  Usually this means starting off interpretering.  It 
        /// also involves adding the wrapper function for recursion enforcement.
        /// 
        /// Because this can race against sys.settrace/setprofile we need to take our 
        /// _ThreadIsEnumeratingAndAccountingLock to ensure no one is actively changing all
        /// of the live functions.
        /// </summary>
        internal void LazyCompileFirstTarget(PythonFunction function) {
            lock (_CodeCreateAndUpdateDelegateLock) {
                UpdateDelegate(PythonContext.GetContext(function.Context), true);
            }
        }

        /// <summary>
        /// Updates the delegate based upon current Python context settings for recursion enforcement
        /// and for tracing.
        /// </summary>
        internal void UpdateDelegate(PythonContext context, bool forceCreation) {
            Delegate finalTarget;

            if (context._enableTracing && _lambda != null) {
                if (_tracingLambda == null) {
                    if (!forceCreation) {
                        // the user just called sys.settrace(), don't force re-compilation of every method in the system.  Instead
                        // we'll just re-compile them as they're invoked.
                        PythonCallTargets.GetPythonTargetType(_argNames.Length > PythonCallTargets.MaxArgs, _argNames.Length, out Target);
                        return;
                    }
                    _tracingLambda = GetGeneratorOrNormalLambdaTracing(context);
                }

                if (_tracingDelegate == null) {
                    _tracingDelegate = CompileLambda(_tracingLambda, new TargetUpdaterForCompilation(context, this).SetCompiledTargetTracing);
                }

                finalTarget = _tracingDelegate;
            } else {
                if (_normalDelegate == null) {
                    _normalDelegate = CompileLambda(GetGeneratorOrNormalLambda(), new TargetUpdaterForCompilation(context, this).SetCompiledTarget);
                }

                finalTarget = _normalDelegate;
            }

            finalTarget = AddRecursionCheck(context, finalTarget);

            Target = finalTarget;
        }

        /// <summary>
        /// Called to set the initial target delegate when the user has passed -X:Debug to enable
        /// .NET style debugging.
        /// </summary>
        internal void SetDebugTarget(PythonContext context, Delegate target) {
            if (Target == null) {
                _normalDelegate = target;

                Target = AddRecursionCheck(context, target);
            }
        }

        /// <summary>
        /// Gets the LambdaExpression for tracing.  
        /// 
        /// If this is a generator function code then the lambda gets tranformed into the correct generator code.
        /// </summary>
        private LambdaExpression GetGeneratorOrNormalLambdaTracing(PythonContext context) {
            var debugProperties = new PythonDebuggingPayload(this, _loopAndFinallyLocations, _handlerLocations);

            var debugInfo = new Microsoft.Scripting.Debugging.CompilerServices.DebugLambdaInfo(
                null,           // IDebugCompilerSupport
                null,           // lambda alias
                false,          // optimize for leaf frames
                null,           // hidden variables
                null,           // variable aliases
                debugProperties // custom payload
            );

            if ((_flags & FunctionAttributes.Generator) == 0) {
                return context.DebugContext.TransformLambda(_lambda, debugInfo);
            }

            return Expression.Lambda(
                Code.Type,
                new GeneratorRewriter(
                    _name,
                    Code.Body
                ).Reduce(
                    _shouldInterpret,
                    _debuggable,
                    Code.Parameters,
                    x => (Expression<Func<MutableTuple, object>>)context.DebugContext.TransformLambda(x, debugInfo)
                ),
                Code.Name,
                Code.Parameters
            );
        }

        /// <summary>
        /// Gets the correct final LambdaExpression for this piece of code.
        /// 
        /// This is either just _lambda or _lambda re-written to be a generator expression.
        /// </summary>
        private LambdaExpression GetGeneratorOrNormalLambda() {
            LambdaExpression finalCode;
            if ((_flags & FunctionAttributes.Generator) == 0) {
                finalCode = Code;
            } else {
                finalCode = Code.ToGenerator(_shouldInterpret, _debuggable);
            }
            return finalCode;
        }

        private Delegate CompileLambda(LambdaExpression code, EventHandler<LightLambdaCompileEventArgs> handler) {
            if (_debuggable) {
                return CompilerHelpers.CompileToMethod(code, DebugInfoGenerator.CreatePdbGenerator(), true);
            } else if (_shouldInterpret) {
                Delegate result = CompilerHelpers.LightCompile(code);

                // If the adaptive compiler decides to compile this function, we
                // want to store the new compiled target. This saves us from going
                // through the interpreter stub every call.
                var lightLambda = result.Target as Microsoft.Scripting.Interpreter.LightLambda;
                if (lightLambda != null) {
                    lightLambda.Compile += handler;
                }

                return result;
            }

            return code.Compile();
        }

        internal Delegate AddRecursionCheck(PythonContext context, Delegate finalTarget) {
            if (context.RecursionLimit != Int32.MaxValue) {
                switch (_argNames.Length) {
                    #region Generated Python Recursion Delegate Switch

                    // *** BEGIN GENERATED CODE ***
                    // generated by function: gen_recursion_delegate_switch from: generate_calls.py

                    case 0:
                        finalTarget = new Func<PythonFunction, object>(new PythonFunctionRecursionCheck0((Func<PythonFunction, object>)finalTarget).CallTarget);
                        break;
                    case 1:
                        finalTarget = new Func<PythonFunction, object, object>(new PythonFunctionRecursionCheck1((Func<PythonFunction, object, object>)finalTarget).CallTarget);
                        break;
                    case 2:
                        finalTarget = new Func<PythonFunction, object, object, object>(new PythonFunctionRecursionCheck2((Func<PythonFunction, object, object, object>)finalTarget).CallTarget);
                        break;
                    case 3:
                        finalTarget = new Func<PythonFunction, object, object, object, object>(new PythonFunctionRecursionCheck3((Func<PythonFunction, object, object, object, object>)finalTarget).CallTarget);
                        break;
                    case 4:
                        finalTarget = new Func<PythonFunction, object, object, object, object, object>(new PythonFunctionRecursionCheck4((Func<PythonFunction, object, object, object, object, object>)finalTarget).CallTarget);
                        break;
                    case 5:
                        finalTarget = new Func<PythonFunction, object, object, object, object, object, object>(new PythonFunctionRecursionCheck5((Func<PythonFunction, object, object, object, object, object, object>)finalTarget).CallTarget);
                        break;
                    case 6:
                        finalTarget = new Func<PythonFunction, object, object, object, object, object, object, object>(new PythonFunctionRecursionCheck6((Func<PythonFunction, object, object, object, object, object, object, object>)finalTarget).CallTarget);
                        break;
                    case 7:
                        finalTarget = new Func<PythonFunction, object, object, object, object, object, object, object, object>(new PythonFunctionRecursionCheck7((Func<PythonFunction, object, object, object, object, object, object, object, object>)finalTarget).CallTarget);
                        break;
                    case 8:
                        finalTarget = new Func<PythonFunction, object, object, object, object, object, object, object, object, object>(new PythonFunctionRecursionCheck8((Func<PythonFunction, object, object, object, object, object, object, object, object, object>)finalTarget).CallTarget);
                        break;
                    case 9:
                        finalTarget = new Func<PythonFunction, object, object, object, object, object, object, object, object, object, object>(new PythonFunctionRecursionCheck9((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object>)finalTarget).CallTarget);
                        break;
                    case 10:
                        finalTarget = new Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object>(new PythonFunctionRecursionCheck10((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object>)finalTarget).CallTarget);
                        break;
                    case 11:
                        finalTarget = new Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object>(new PythonFunctionRecursionCheck11((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object>)finalTarget).CallTarget);
                        break;
                    case 12:
                        finalTarget = new Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object>(new PythonFunctionRecursionCheck12((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object>)finalTarget).CallTarget);
                        break;
                    case 13:
                        finalTarget = new Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object>(new PythonFunctionRecursionCheck13((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object>)finalTarget).CallTarget);
                        break;
                    case 14:
                        finalTarget = new Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>(new PythonFunctionRecursionCheck14((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>)finalTarget).CallTarget);
                        break;
                    case 15:
                        finalTarget = new Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>(new PythonFunctionRecursionCheck15((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>)finalTarget).CallTarget);
                        break;

                    // *** END GENERATED CODE ***

                    #endregion
                    default:
                        finalTarget = new Func<PythonFunction, object[], object>(new PythonFunctionRecursionCheckN((Func<PythonFunction, object[], object>)finalTarget).CallTarget);
                        break;

                }
            }
            return finalTarget;
        }

        class TargetUpdaterForCompilation {
            private readonly PythonContext _context;
            private readonly FunctionCode _code;

            public TargetUpdaterForCompilation(PythonContext context, FunctionCode code) {
                _code = code;
                _context = context;
            }

            public void SetCompiledTarget(object sender, Microsoft.Scripting.Interpreter.LightLambdaCompileEventArgs e) {
                _code.Target = _code.AddRecursionCheck(_context, _code._normalDelegate = e.Compiled);
            }

            public void SetCompiledTargetTracing(object sender, Microsoft.Scripting.Interpreter.LightLambdaCompileEventArgs e) {
                _code.Target = _code.AddRecursionCheck(_context, _code._tracingDelegate = e.Compiled);
            }
        }

        #region IExpressionSerializable Members

        Expression IExpressionSerializable.CreateExpression() {
            return Expression.Call(
                typeof(PythonOps).GetMethod("MakeFunctionCode"),
                Compiler.Ast.ArrayGlobalAllocator._globalContext,
                Expression.Constant(_name),
                Expression.Constant(_initialDoc, typeof(string)),
                Expression.NewArrayInit(
                    typeof(string),
                    ArrayUtils.ConvertAll(_argNames, (x) => Expression.Constant(x))
                ),
                Expression.Constant(_flags),
                Expression.New(
                    typeof(SourceSpan).GetConstructor(new Type[] { typeof(SourceLocation), typeof(SourceLocation) }),
                    Expression.New(
                        typeof(SourceLocation).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int) }),
                        Expression.Constant(_span.Start.Index),
                        Expression.Constant(_span.Start.Line),
                        Expression.Constant(_span.Start.Column)
                    ),
                    Expression.New(
                        typeof(SourceLocation).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int) }),
                        Expression.Constant(_span.End.Index),
                        Expression.Constant(_span.End.Line),
                        Expression.Constant(_span.End.Column)
                    )
                ),
                Expression.Constant(_filename),
                GetGeneratorOrNormalLambda(),
                _closureVars.Count > 0 ?
                    (Expression)Expression.NewArrayInit(
                        typeof(string),
                        ArrayUtils.ConvertAll(_closureVars._data, (x) => Expression.Constant(x))
                    ) :
                    (Expression)Expression.Constant(null, typeof(string[]))
            );
        }

        #endregion

        /// <summary>
        /// Extremely light weight linked list of weak references used for tracking
        /// all of the FunctionCode objects which get created and need to be updated
        /// for purposes of recursion enforcement or tracing.
        /// </summary>
        internal class CodeList {
            public readonly WeakReference Code;
            public CodeList Next;

            public CodeList() { }

            public CodeList(WeakReference code, CodeList next) {
                Code = code;
                Next = next;
            }
        }
    }

    internal class PythonDebuggingPayload {
        public readonly Dictionary<int, Dictionary<int, bool>> LoopAndFinallyLocations;
        public readonly Dictionary<int, bool> HandlerLocations;
        public readonly FunctionCode Code;

        public PythonDebuggingPayload(FunctionCode code, Dictionary<int, Dictionary<int, bool>> loopLocations, Dictionary<int, bool> handlerLocations) {
            Code = code;
            LoopAndFinallyLocations = loopLocations;
            HandlerLocations = handlerLocations;
        }
    }
}
