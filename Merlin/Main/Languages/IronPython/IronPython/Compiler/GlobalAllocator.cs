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

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;

using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    /// <summary>
    /// Provides specific behaviors for different compilation modes.  For example pre-compiled
    /// code, optimized code, collectible code all have different code gen properties.  For
    /// the most part these are how we access globals and call sites  and cache static fields.
    /// </summary>
    abstract class GlobalAllocator {
        private readonly Dictionary<PythonVariable/*!*/, MSAst.Expression/*!*/>/*!*/ _variables = new Dictionary<PythonVariable/*!*/, MSAst.Expression/*!*/>();
        

        protected GlobalAllocator() {
        }

        #region Customizable APIs

        public abstract ScriptCode MakeScriptCode(MSAst.Expression/*!*/ body, CompilerContext/*!*/ context, PythonAst/*!*/ ast);

        public abstract MSAst.Expression/*!*/ GlobalContext {
            get;
        }

        protected abstract MSAst.Expression/*!*/ GetGlobal(string/*!*/ name, AstGenerator/*!*/ ag, bool isLocal);

        public virtual MSAst.Expression/*!*/ GetConstant(object value) {
            return Ast.Constant(value);
        }

        public virtual MSAst.Expression/*!*/ GetSymbol(SymbolId name) {
            return Utils.Constant(name);
        }

        /// <summary>
        /// Generates any preparation code for a new class def or function def scope.
        /// </summary>
        public virtual MSAst.Expression[] PrepareScope(AstGenerator/*!*/ gen) {
            return AstGenerator.EmptyExpression;
        }

        #endregion

        #region Fixed Public API Surface

        public MSAst.Expression/*!*/ Assign(MSAst.Expression/*!*/ expression, MSAst.Expression value) {
            IPythonVariableExpression pyGlobal = expression as IPythonVariableExpression;
            if(pyGlobal != null) {
                return pyGlobal.Assign(value);
            }
            
            return Ast.Assign(expression, value);
        }

        public MSAst.Expression/*!*/ Delete(MSAst.Expression/*!*/ expression) {
            IPythonVariableExpression pyGlobal = expression as IPythonVariableExpression;
            if (pyGlobal != null) {
                return pyGlobal.Delete();
            }
            
            return Ast.Assign(expression, Ast.Field(null, typeof(Uninitialized).GetField("Instance")));
        }

        // TODO: Optimized overloads for various aritys.
        public virtual MSAst.Expression/*!*/ Dynamic(DynamicMetaObjectBinder/*!*/ binder, Type/*!*/ retType, MSAst.Expression/*!*/ arg0) {
            return Ast.Dynamic(binder, retType, arg0);
        }

        public virtual MSAst.Expression/*!*/ Dynamic(DynamicMetaObjectBinder/*!*/ binder, Type/*!*/ retType, MSAst.Expression/*!*/ arg0, MSAst.Expression/*!*/ arg1) {
            return Ast.Dynamic(binder, retType, arg0, arg1);
        }

        public virtual MSAst.Expression/*!*/ Dynamic(DynamicMetaObjectBinder/*!*/ binder, Type/*!*/ retType, MSAst.Expression/*!*/ arg0, MSAst.Expression/*!*/ arg1, MSAst.Expression/*!*/ arg2) {
            return Ast.Dynamic(binder, retType, arg0, arg1, arg2);
        }

        public virtual MSAst.Expression/*!*/ Dynamic(DynamicMetaObjectBinder/*!*/ binder, Type/*!*/ retType, MSAst.Expression/*!*/ arg0, MSAst.Expression/*!*/ arg1, MSAst.Expression/*!*/ arg2, MSAst.Expression/*!*/ arg3) {
            return Ast.Dynamic(binder, retType, arg0, arg1, arg2, arg3);
        }

        public virtual MSAst.Expression/*!*/ Dynamic(DynamicMetaObjectBinder/*!*/ binder, Type/*!*/ retType, params MSAst.Expression/*!*/[]/*!*/ args) {
            Assert.NotNull(binder, retType, args);
            Assert.NotNullItems(args);

            
            return Ast.Dynamic(binder, retType, args);
        }

        public MSAst.Expression/*!*/ CreateVariable(AstGenerator/*!*/ ag, PythonVariable/*!*/ variable, bool emitDictionary) {
            Assert.NotNull(ag, variable);

            Debug.Assert(variable.Kind != VariableKind.Parameter);
            
            string name = SymbolTable.IdToString(variable.Name);
            switch (variable.Kind) {
                case VariableKind.Global:
                case VariableKind.GlobalLocal:
                    return _variables[variable] = GetGlobal(name, ag, false);
                case VariableKind.Local:
                case VariableKind.HiddenLocal:
                    if (ag.IsGlobal) {
                        return _variables[variable] = GetGlobal(name, ag, true);
                    } else if (variable.AccessedInNestedScope || (emitDictionary && variable.Kind != VariableKind.HiddenLocal)) {
                        return ag.SetLocalLiftedVariable(variable, ag.LiftedVariable(variable, name, variable.AccessedInNestedScope));
                    } else {
                        return _variables[variable] = ag.Variable(typeof(object), name);
                    }
                default:
                    throw Assert.Unreachable;
            }
        }

        public void SetParameter(PythonVariable/*!*/ variable, MSAst.Expression/*!*/ parameter) {
            Assert.NotNull(variable, parameter);

            _variables[variable] = parameter;
        }

        public MSAst.Expression/*!*/ GetVariable(AstGenerator/*!*/ ag, PythonVariable/*!*/ variable) {
            Assert.NotNull(ag, variable);

            MSAst.Expression res;
            if(_variables.TryGetValue(variable, out res)) {
                return res;
            }

            return ag.LocalLifted[variable];
        }

        #endregion
    }
}
