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
using System.Collections.ObjectModel;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Runtime.Operations;

using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    public class ClassDefinition : ScopeStatement {
        private SourceLocation _header;
        private readonly SymbolId _name;
        private readonly Statement _body;
        private readonly Expression[] _bases;
        private IList<Expression> _decorators;

        private PythonVariable _variable;           // Variable corresponding to the class name
        private PythonVariable _modVariable;        // Variable for the the __module__ (module name)
        private PythonVariable _docVariable;        // Variable for the __doc__ attribute
        private PythonVariable _modNameVariable;    // Variable for the module's __name__

        private static int _classId;

        private static MSAst.ParameterExpression _parentContextParam = Ast.Parameter(typeof(CodeContext), "$parentContext");

        public ClassDefinition(SymbolId name, Expression[] bases, Statement body) {
            _name = name;
            _bases = bases;
            _body = body;
        }

        public SourceLocation Header {
            get { return _header; }
            set { _header = value; }
        }

        public SymbolId Name {
            get { return _name; }
        }

        public Expression[] Bases {
            get { return _bases; }
        }

        public Statement Body {
            get { return _body; }
        }

        public IList<Expression> Decorators {
            get {
                return _decorators;
            }
            set {
                _decorators = value;
            }
        }

        internal PythonVariable Variable {
            get { return _variable; }
            set { _variable = value; }
        }

        internal PythonVariable ModVariable {
            get { return _modVariable; }
            set { _modVariable = value; }
        }

        internal PythonVariable DocVariable {
            get { return _docVariable; }
            set { _docVariable = value; }
        }

        internal PythonVariable ModuleNameVariable {
            get { return _modNameVariable; }
            set { _modNameVariable = value; }
        }

        internal override bool ExposesLocalVariable(PythonVariable variable) {
            return true;
        }

        internal override PythonVariable BindName(PythonNameBinder binder, SymbolId name) {
            PythonVariable variable;

            // Python semantics: The variables bound local in the class
            // scope are accessed by name - the dictionary behavior of classes
            if (TryGetVariable(name, out variable)) {
                return variable.Kind == VariableKind.Local 
                    || variable.Kind == VariableKind.GlobalLocal 
                    || variable.Kind == VariableKind.HiddenLocal 
                     ? null : variable;
            }

            // Try to bind in outer scopes, if we have an unqualified exec we need to leave the
            // variables as free for the same reason that locals are accessed by name.
            for (ScopeStatement parent = Parent; parent != null; parent = parent.Parent) {
                if (parent.TryBindOuter(name, out variable)) {
                    variable.AccessedInNestedScope = true;
                    return variable;
                }
            }

            return null;
        }
        
        internal override MSAst.Expression Transform(AstGenerator ag) {
            string className = SymbolTable.IdToString(_name);
            AstGenerator classGen = new AstGenerator(ag, className, false, "class " + className);
            classGen.Parameter(_parentContextParam);
            // we always need to create a nested context for class defs
            classGen.CreateNestedContext();

            List<MSAst.Expression> init = new List<MSAst.Expression>();
            
            classGen.AddHiddenVariable(ArrayGlobalAllocator._globalContext);
            init.Add(Ast.Assign(ArrayGlobalAllocator._globalContext, Ast.Call(typeof(PythonOps).GetMethod("GetGlobalContext"), _parentContextParam)));
            init.AddRange(classGen.Globals.PrepareScope(classGen));

            CreateVariables(classGen, _parentContextParam, init, true, false);

            List<MSAst.Expression> statements = new List<MSAst.Expression>();
            // Create the body
            MSAst.Expression bodyStmt = classGen.Transform(_body);
            
            // __module__ = __name__
            MSAst.Expression modStmt = classGen.Globals.Assign(
                classGen.Globals.GetVariable(classGen, _modVariable), 
                classGen.Globals.GetVariable(classGen, _modNameVariable));

            string doc = classGen.GetDocumentation(_body);
            if (doc != null) {
                statements.Add(
                    classGen.Globals.Assign(
                        classGen.Globals.GetVariable(classGen, _docVariable),
                        AstUtils.Constant(doc)
                    )
                );
            }

            if (_body.CanThrow && ag.PyContext.PythonOptions.Frames) {
                bodyStmt = FunctionDefinition.AddFrame(classGen.LocalContext, Ast.Constant(null, typeof(PythonFunction)), bodyStmt);
                classGen.AddHiddenVariable(FunctionDefinition._functionStack);
            }

            bodyStmt = classGen.WrapScopeStatements(
                Ast.Block(
                    statements.Count == 0 ?
                        AstGenerator.EmptyBlock :
                        Ast.Block(new ReadOnlyCollection<MSAst.Expression>(statements)),
                    modStmt,
                    bodyStmt,
                    classGen.LocalContext    // return value
                )
            );

            var lambda = Ast.Lambda<Func<CodeContext, CodeContext>>(
                classGen.MakeBody(_parentContextParam, init.ToArray(), bodyStmt, false),
                classGen.Name + "$" + _classId++,
                classGen.Parameters
            );

            MSAst.Expression classDef = Ast.Call(
                AstGenerator.GetHelperMethod("MakeClass"),
                ag.EmitDebugSymbols ? 
                    (MSAst.Expression)lambda : 
                    (MSAst.Expression)Ast.Constant(new LazyCode<Func<CodeContext, CodeContext>>(lambda, ag.ShouldInterpret), typeof(object)),
                ag.LocalContext,
                AstUtils.Constant(SymbolTable.IdToString(_name)),
                Ast.NewArrayInit(
                    typeof(object),
                    ag.TransformAndConvert(_bases, typeof(object))
                ),
                AstUtils.Constant(FindSelfNames())
            );

            classDef = ag.AddDecorators(classDef, _decorators);

            return ag.AddDebugInfo(ag.Globals.Assign(ag.Globals.GetVariable(ag, _variable), classDef), new SourceSpan(Start, Header));
        }

        /// <summary>
        /// Gets the closure tuple from our parent context.
        /// </summary>
        public override MSAst.Expression GetClosureTuple() {
            return MSAst.Expression.Call(
                typeof(PythonOps).GetMethod("GetClosureTupleFromContext"),
                _parentContextParam
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_decorators != null) {
                    foreach (Expression decorator in _decorators) {
                        decorator.Walk(walker);
                    }
                }
                if (_bases != null) {
                    foreach (Expression b in _bases) {
                        b.Walk(walker);
                    }
                }
                if (_body != null) {
                    _body.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }

        private string FindSelfNames() {
            SuiteStatement stmts = Body as SuiteStatement;
            if (stmts == null) return "";

            foreach (Statement stmt in stmts.Statements) {
                FunctionDefinition def = stmt as FunctionDefinition;
                if (def != null && def.Name == SymbolTable.StringToId("__init__")) {
                    return string.Join(",", SelfNameFinder.FindNames(def));
                }
            }
            return "";
        }

        private class SelfNameFinder : PythonWalker {
            private readonly FunctionDefinition _function;
            private readonly Parameter _self;

            public SelfNameFinder(FunctionDefinition function, Parameter self) {
                _function = function;
                _self = self;
            }

            public static string[] FindNames(FunctionDefinition function) {
                Parameter[] parameters = function.Parameters;

                if (parameters.Length > 0) {
                    SelfNameFinder finder = new SelfNameFinder(function, parameters[0]);
                    function.Body.Walk(finder);
                    return SymbolTable.IdsToStrings(new List<SymbolId>(finder._names.Keys));
                } else {
                    // no point analyzing function with no parameters
                    return ArrayUtils.EmptyStrings;
                }
            }

            private Dictionary<SymbolId, bool> _names = new Dictionary<SymbolId, bool>();

            private bool IsSelfReference(Expression expr) {
                NameExpression ne = expr as NameExpression;
                if (ne == null) return false;

                PythonVariable variable;
                if (_function.TryGetVariable(ne.Name, out variable) && variable == _self.Variable) {
                    return true;
                }

                return false;
            }

            // Don't recurse into class or function definitions
            public override bool Walk(ClassDefinition node) {
                return false;
            }
            public override bool Walk(FunctionDefinition node) {
                return false;
            }

            public override bool Walk(AssignmentStatement node) {
                foreach (Expression lhs in node.Left) {
                    MemberExpression me = lhs as MemberExpression;
                    if (me != null) {
                        if (IsSelfReference(me.Target)) {
                            _names[me.Name] = true;
                        }
                    }
                }
                return true;
            }
        }
    }
}
