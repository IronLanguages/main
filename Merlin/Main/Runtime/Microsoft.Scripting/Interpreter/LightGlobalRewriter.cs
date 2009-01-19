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
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Interpreter {

    internal class LookupNameExpression : Expression, IInstructionProvider {
        internal SymbolId _name;
        internal bool _isLocal;
        internal Expression _context;

        public LookupNameExpression(bool isLocal, SymbolId name, Expression context) {
            this._isLocal = isLocal;
            this._name = name;
            this._context = context;
        }

        public override bool CanReduce {
            get { return true; }
        }

        protected override Type GetExpressionType() {
            return typeof(object);
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Extension;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor) {
            visitor.Visit(_context);
            return this;
        }

        public override Expression Reduce() {
            return Expression.Call(
                typeof(ScriptingRuntimeHelpers).GetMethod(_isLocal ? "LookupName" : "LookupGlobalName"),
                new Expression[]{
                    _context,
                    AstUtils.Constant(_name)
                }
            );
        }


        #region IInstructionProvider Members

        public virtual Instruction GetInstruction(LightCompiler compiler) {
            compiler.Compile(_context);
            if (_isLocal) {
                return new LookupNameInstruction(_name);
            } else {
                return new LookupGlobalNameInstruction(_name);
            }
        }

        #endregion
    }

    internal class SetNameExpression : LookupNameExpression {
        private Expression _value;

        public SetNameExpression(bool isLocal, SymbolId name, Expression context, Expression value) : base(isLocal, name, context) {
            this._value = value;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor) {
            visitor.Visit(_value);
            return base.VisitChildren(visitor);
        }

        public override Expression Reduce() {
            return Expression.Call(
                typeof(ScriptingRuntimeHelpers).GetMethod(_isLocal ? "SetName" : "SetGlobalName"),
                new Expression[]{
                    _context,
                    AstUtils.Constant(_name),
                    _value
                }
            );
        }


        #region IInstructionProvider Members

        public override Instruction GetInstruction(LightCompiler compiler) {
            compiler.Compile(_context);
            compiler.Compile(_value);
            if (_isLocal) {
                return new SetNameInstruction(_name);
            } else {
                return new SetGlobalNameInstruction(_name);
            }
        }

        #endregion
    }



    internal class GlobalGetExpression : Expression, IInstructionProvider {
        private static System.Reflection.PropertyInfo _CurrentValueProperty =
            typeof(ModuleGlobalWrapper).GetProperty("CurrentValue");

        private ModuleGlobalWrapper _global;
        public GlobalGetExpression(ModuleGlobalWrapper global) {
            this._global = global;
        }

        public ModuleGlobalWrapper Global { get { return _global; } }
        
        public override bool CanReduce {
            get { return true; }
        }

        protected override Type GetExpressionType() {
            return typeof(object);
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Extension;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor) {
            return this;
        }

        public override Expression Reduce() {
            return Expression.Property(Expression.Constant(_global), _CurrentValueProperty);
        }


        #region IInstructionProvider Members

        public virtual Instruction GetInstruction(LightCompiler compiler) {
            return new GetGlobalInstruction(_global);
        }

        #endregion
    }

    internal class GlobalSetExpression : GlobalGetExpression {
        private Expression _value;
        public GlobalSetExpression(ModuleGlobalWrapper global, Expression value) : base(global) {
            this._value = value;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor) {
            visitor.Visit(_value);
            return this;
        }

        public override Expression Reduce() {
            var prop = base.Reduce();
            return Expression.Assign(prop, _value);
        }

        #region IInstructionProvider Members

        public override Instruction GetInstruction(LightCompiler compiler) {
            compiler.Compile(this._value);
            return new SetGlobalInstruction(Global);
        }

        #endregion
    }




    /// <summary>
    /// </summary>
    public class LightGlobalRewriter : ExpressionVisitor { 
        private Expression _context;
        private CodeContext _codeContext;
        public Scope Scope;        
        private Dictionary<GlobalVariableExpression, ModuleGlobalWrapper> _wrappers;

        public LambdaExpression RewriteLambda(LambdaExpression lambda, string name, LanguageContext languageContext, bool optimized) {
            Debug.Assert(_context == null);
            Debug.Assert(lambda.Parameters.Count == 0);

            if (optimized) {
                _wrappers = new Dictionary<GlobalVariableExpression, ModuleGlobalWrapper>();

                var customDictionary = new GlobalsDictionary();
                this.Scope = new Scope(customDictionary);

                //context.EnsureScopeExtension(scope.ModuleScope);
                //return new CodeContext(scope, context);

                _codeContext = new CodeContext(this.Scope, languageContext);
                _context = Expression.Constant(_codeContext);


                var ret = (LambdaExpression)Visit(lambda); //???
                customDictionary.SetData(new List<ModuleGlobalWrapper>(_wrappers.Values).ToArray());
                return ret;
            } else {
                // Fix up the top-level lambda to have a scope and language parameters
                var scopeParameter = Expression.Parameter(typeof(Scope), "$scope");
                var languageParameter = Expression.Parameter(typeof(LanguageContext), "$language");
                var contextVariable = Expression.Variable(typeof(CodeContext), "$globalContext");

                _context = contextVariable;
                lambda = (LambdaExpression)Visit(lambda);

                return Expression.Lambda<DlrMainCallTarget>(
                    AstUtils.AddScopedVariable(
                        lambda.Body,
                        contextVariable,
                        Expression.Call(typeof(ScriptingRuntimeHelpers).GetMethod("CreateTopLevelCodeContext"), scopeParameter, languageParameter)
                    ),
                    name,
                    new[] { scopeParameter, languageParameter }
                );
            }
        }

        private ModuleGlobalWrapper GetWrapper(GlobalVariableExpression node) {
            ModuleGlobalWrapper ret;
            if (!_wrappers.TryGetValue(node, out ret)) {
                ret = new ModuleGlobalWrapper(_codeContext, SymbolTable.StringToId(node.Name));
                _wrappers[node] = ret;
            }
            return ret;
        }

        private Expression RewriteGet(GlobalVariableExpression node) {
            if (_wrappers == null) {
                return new LookupNameExpression(node.IsLocal, SymbolTable.StringToId(node.Name), Context);
            } else {
                return new GlobalGetExpression(GetWrapper(node));
            }
        }

        private Expression RewriteSet(AssignmentExtensionExpression node) {
            var value = Visit(node.Value);
            var globalVar = (GlobalVariableExpression)node.Expression;
            if (_wrappers == null) {
                return new SetNameExpression(globalVar.IsLocal, SymbolTable.StringToId(globalVar.Name), Context, value);
            } else {
                return new GlobalSetExpression(GetWrapper(globalVar), value);
            }
        }

        #region rewriter overrides

        
        protected override Expression VisitExtension(Expression node) {
            if (node is YieldExpression ||
                node is GeneratorExpression ||
                node is FinallyFlowControlExpression) {
                // These should be rewritten last, when doing finaly compilation
                // for now, just walk them so we can find other nodes
                return base.VisitExtension(node);
            }

            GlobalVariableExpression global = node as GlobalVariableExpression;
            if (global != null) {
                return RewriteGet(global);
            }

            CodeContextExpression cc = node as CodeContextExpression;
            if (cc != null) {
                return _context;
            }

            CodeContextScopeExpression ccs = node as CodeContextScopeExpression;
            if (ccs != null) {
                return Rewrite(ccs);
            }

            AssignmentExtensionExpression aee = node as AssignmentExtensionExpression;
            if (aee != null) {
                return Rewrite(aee);
            }

            // Must remove extension nodes because they could contain
            // one of the above node types. See, e.g. DeleteUnboundExpression
            return Visit(node.ReduceExtensions());
        }

        private Expression Rewrite(AssignmentExtensionExpression node) {
            Expression lvalue = node.Expression;

            GlobalVariableExpression global = lvalue as GlobalVariableExpression;
            if (global != null) {
                return RewriteSet(node);
            }

            return node;
        }

        #endregion

        #region CodeContext support

        protected Expression Context {
            get { return _context; }
            set { _context = value; }
        }

        private Expression Rewrite(CodeContextScopeExpression ccs) {
            Expression saved = _context;
            ParameterExpression nested = Expression.Variable(typeof(CodeContext), "$frame");

            // rewrite body with nested context
            _context = nested;
            Expression body = Visit(ccs.Body);
            _context = saved;

            // wrap the body in a scope that initializes the nested context
            return AstUtils.AddScopedVariable(body, nested, Visit(ccs.NewContext));
        }

        #endregion
    }
}
