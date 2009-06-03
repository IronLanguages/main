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
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime {
    /// <summary>
    /// Represents a piece of code.  This can reference either a CompiledCode
    /// object or a Function.   The user can explicitly call FunctionCode by
    /// passing it into exec or eval.
    /// </summary>
    [PythonType("code")]
    public class FunctionCode {
        #region Private member variables
        //private Statement _body;

        private object _varnames;
        private ScriptCode _code;
        private PythonFunction _func;
        private string _filename;
        private int _lineNo;
        private FunctionAttributes _flags;      // future division, generator
        private LambdaExpression _lambda;
        private bool _shouldInterpret;
        private bool _emitDebugSymbols;
        #endregion

        internal FunctionCode(ScriptCode code, CompileFlags compilerFlags)
            : this(code) {

            if ((compilerFlags & CompileFlags.CO_FUTURE_DIVISION) != 0)
                _flags |= FunctionAttributes.FutureDivision;
        }

        internal FunctionCode(ScriptCode code) {
            _code = code;
        }

        internal FunctionCode(PythonFunction f, FunctionInfo funcInfo) {
            _func = f;
            _filename = funcInfo.Path;
            object fn;
            if (_filename == null && f.Context.GlobalScope.Dict.TryGetValue(Symbols.File, out fn) && fn is string) {
                _filename = (string)fn;
            }
            _lineNo = funcInfo.LineNumber;
            _flags = funcInfo.Flags;
            _lambda = funcInfo.Code;
            _shouldInterpret = funcInfo.ShouldInterpret;
            _emitDebugSymbols = funcInfo.EmitDebugSymbols;
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

        public object co_argcount {
            get {
                if (_code != null) return 0;
                int argCnt = _func.ArgNames.Length;
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

        public object co_consts {
            get {
                throw PythonOps.NotImplementedError("");
            }
        }

        public object co_filename {
            get {
                return _filename;
            }
        }

        public object co_firstlineno {
            get {
                return _lineNo;
            }
        }

        public object co_flags {
            get {
                return (int)_flags;
            }
        }

        public object co_freevars {
            get {
                throw PythonOps.NotImplementedError("");
            }
        }

        public object co_lnotab {
            get {
                throw PythonOps.NotImplementedError("");
            }
        }

        public object co_name {
            get {
                if (_func != null) return _func.__name__;
                if (_code != null) return _code.GetType().Name;

                throw PythonOps.NotImplementedError("");
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
        }

        internal void SetFilename(string sourceFile) {
            _filename = sourceFile;
        }

        internal void SetLineNumber(int line) {
            _lineNo = line;
        }

        internal object Call(Scope/*!*/ scope) {
            if (_code != null) {
                return _code.Run(scope);
            } else if (_func != null) {
                CallSite<Func<CallSite, CodeContext, PythonFunction, object>> site = PythonContext.GetContext(_func.Context).FunctionCallSite;
                return site.Target(site, DefaultContext.Default, _func);
            }

            throw PythonOps.TypeError("bad code");
        }

        #endregion

        #region Private helper functions

        private PythonTuple GetArgNames() {
            if (_code != null) return PythonTuple.MakeTuple();

            List<string> names = new List<string>();
            List<PythonTuple> nested = new List<PythonTuple>();


            for (int i = 0; i < _func.ArgNames.Length; i++) {
                if (_func.ArgNames[i].IndexOf('#') != -1 && _func.ArgNames[i].IndexOf('!') != -1) {
                    names.Add("." + (i * 2));
                    // TODO: need to get local variable names here!!!
                    //nested.Add(FunctionDefinition.DecodeTupleParamName(func.ArgNames[i]));
                } else {
                    names.Add(_func.ArgNames[i]);
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
            } else if (_func != null) {
                return _func == other._func;
            }

            throw PythonOps.TypeError("bad code");
        }

        public override int GetHashCode() {
            if (_code != null) {
                return _code.GetHashCode();
            } else if (_func != null) {
                return _func.GetHashCode();
            }

            throw PythonOps.TypeError("bad code");
        }

        internal Delegate GetCompiledCode() {
            if (_shouldInterpret) {
                Delegate result = Microsoft.Scripting.Generation.CompilerHelpers.LightCompile(Code);

                // If the adaptive compiler decides to compile this function, we
                // want to store the new compiled target. This saves us from going
                // through the interpreter stub every call.
                var lightLambda = result.Target as Microsoft.Scripting.Interpreter.LightLambda;
                if (lightLambda != null) {
                    lightLambda.Compile += SetCompiledTarget;
                }

                return result;
            }

            return Code.Compile();
        }

        private void SetCompiledTarget(object sender, Microsoft.Scripting.Interpreter.LightLambdaCompileEventArgs e) {
            _func.Target = e.Compiled;
        }
    }
}
