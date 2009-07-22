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

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime;

using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    /// <summary>
    /// A global allocator that puts all of the globals into an array access.  The array is an
    /// array of PythonGlobal objects.  We then just close over the array for any inner functions.
    /// 
    /// Once compiled a RuntimeScriptCode is produced which is closed over the entire execution
    /// environment.
    /// </summary>
    class ArrayGlobalAllocator : GlobalAllocator {
        private readonly Dictionary<string, GlobalInfo> _globals = new Dictionary<string, GlobalInfo>();
        private readonly Dictionary<SymbolId, PythonGlobal> _globalVals = new Dictionary<SymbolId, PythonGlobal>();
        private readonly MSAst.ParameterExpression/*!*/ _globalArray;
        private readonly CodeContext _context;
        private readonly Scope _scope;
        private readonly GlobalArrayConstant _array;
        internal static readonly MSAst.ParameterExpression/*!*/ _globalContext = Ast.Parameter(typeof(CodeContext), "$globalContext");

        public ArrayGlobalAllocator(PythonContext/*!*/ context) {
            _globalArray = Ast.Parameter(typeof(PythonGlobal[]), "$globalArray");
            _scope = new Scope(new PythonDictionary(new GlobalDictionaryStorage(_globalVals)));
            _context = new CodeContext(_scope, context);
            _array = new GlobalArrayConstant();
        }

        public override ScriptCode/*!*/ MakeScriptCode(MSAst.Expression/*!*/ body, CompilerContext/*!*/ context, PythonAst/*!*/ ast) {
            // create the CodeContext
            PythonGlobal[] globalArray = new PythonGlobal[_globals.Count];

            // now fill in the dictionary and create the array
            foreach (var global in _globals) {
                SymbolId globalName = SymbolTable.StringToId(global.Key);
                
                globalArray[global.Value.Index] = _globalVals[globalName];
            }
            
            _array.Array = globalArray;

            // finally build the funcion that's closed over the array and
            string name = ((PythonCompilerOptions)context.Options).ModuleName ?? "<unnamed>";
            var func = Ast.Lambda<Func<object>>(
                Ast.Block(
                    new[] { _globalArray, _globalContext },
                    Ast.Assign(_globalArray, Ast.Constant(globalArray)),
                    Ast.Assign(_globalContext, Ast.Constant(_context)),
                    body
                ),
                name,
                new MSAst.ParameterExpression[0]
            );

            return new RuntimeScriptCode(context, func, ast, _context);
        }

        public override MSAst.Expression[] PrepareScope(AstGenerator/*!*/ gen) {
            gen.AddHiddenVariable(_globalArray);
            return new MSAst.Expression[] {
                Ast.Assign(_globalArray, _array)
            };
        }

        /// <summary>
        /// Provides a wrapper expression for our PythonGlobal array of global variables.
        /// 
        /// This always reduces to a PythonGlobal[] but we update it at the very end of the
        /// compilation process.  This enables us to create nodes which refer to this
        /// array and burn them in as constants even before the array has been created.
        /// </summary>
        private class GlobalArrayConstant : MSAst.Expression {
            public PythonGlobal[] Array;

            public override bool CanReduce {
                get {
                    return true;
                }
            }

            public sealed override MSAst.ExpressionType NodeType {
                get { return MSAst.ExpressionType.Extension; }
            }

            public sealed override Type Type {
                get { return typeof(PythonGlobal[]); }
            }

            public override System.Linq.Expressions.Expression Reduce() {
                // type specified for a better debugging experience (otherwise it ends up null)
                return MSAst.Expression.Constant(Array, typeof(PythonGlobal[]));
            }
        }

        protected MSAst.ParameterExpression/*!*/ GlobalArray {
            get { return _globalArray; }
        }

        public override MSAst.Expression/*!*/ GlobalContext {
            get { return _globalContext; }
        }

        protected override MSAst.Expression/*!*/ GetGlobal(string/*!*/ name, AstGenerator/*!*/ ag, bool isLocal) {
            SymbolId idname = SymbolTable.StringToId(name);
            PythonGlobal global = _globalVals[idname] = new PythonGlobal(_context, idname);
            return new PythonGlobalVariableExpression(GetGlobalInfo(name).Expression, global);
        }

        public string[] GetNames() {
            string[] res = new string[_globals.Count];
            foreach (var kvp in _globals) {
                res[kvp.Value.Index] = kvp.Key;
            }
            return res;
        }

        private GlobalInfo/*!*/ GetGlobalInfo(string/*!*/ name) {
            GlobalInfo global;
            if (!_globals.TryGetValue(name, out global)) {
                _globals[name] = global = new GlobalInfo(
                    _globals.Count,
                    Ast.ArrayIndex(
                        _globalArray,
                        Ast.Constant(_globals.Count)
                    )
                );
            }
            return global;
        }

        class GlobalInfo {
            public readonly int Index;
            public readonly MSAst.Expression/*!*/ Expression;

            public GlobalInfo(int index, MSAst.Expression/*!*/ expression) {
                Index = index;
                Expression = expression;
            }
        }
    }
}
