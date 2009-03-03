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
using System.Diagnostics;
using System.Dynamic;
using System.Threading;
using IronPython.Runtime;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    public class FunctionDefinition : ScopeStatement {
        protected Statement _body;
        private SourceLocation _header;
        private readonly SymbolId _name;
        private readonly Parameter[] _parameters;
        private IList<Expression> _decorators;
        private SourceUnit _sourceUnit;
        private bool _generator;                        // The function is a generator

        // true if this function can set sys.exc_info(). Only functions with an except block can set that.
        private bool _canSetSysExcInfo;
        private bool _containsTryFinally;               // true if the function contains try/finally, used for generator optimization

        private PythonVariable _variable;               // The variable corresponding to the function name or null for lambdas

        public bool IsLambda {
            get {
                return _name.IsEmpty;
            }
        }

        public FunctionDefinition(SymbolId name, Parameter[] parameters, SourceUnit sourceUnit)
            : this(name, parameters, null, sourceUnit) {
        }

        public FunctionDefinition(SymbolId name, Parameter[] parameters, Statement body, SourceUnit sourceUnit) {
            _name = name;
            _parameters = parameters;
            _body = body;
            _sourceUnit = sourceUnit;
        }

        public Parameter[] Parameters {
            get { return _parameters; }
        }

        public Statement Body {
            get { return _body; }
            set { _body = value; }
        }

        public SourceLocation Header {
            get { return _header; }
            set { _header = value; }
        }

        public SymbolId Name {
            get { return _name; }
        }

        public IList<Expression> Decorators {
            get { return _decorators; }
            set { _decorators = value; }
        }

        public bool IsGenerator {
            get { return _generator; }
            set { _generator = value; }
        }

        // Called by parser to mark that this function can set sys.exc_info(). 
        // An alternative technique would be to just walk the body after the parse and look for a except block.
        internal bool CanSetSysExcInfo {
            set { _canSetSysExcInfo = value; }
        }

        internal bool ContainsTryFinally {
            get { return _containsTryFinally; }
            set { _containsTryFinally = value; }
        }

        internal PythonVariable Variable {
            get { return _variable; }
            set { _variable = value; }
        }

        protected override bool ExposesLocalVariables {
            get { return NeedsLocalsDictionary; }
        }

        private static FunctionAttributes ComputeFlags(Parameter[] parameters) {
            FunctionAttributes fa = FunctionAttributes.None;
            if (parameters != null) {
                int i;
                for (i = 0; i < parameters.Length; i++) {
                    Parameter p = parameters[i];
                    if (p.IsDictionary || p.IsList) break;
                }
                // Check for the list and dictionary parameters, which must be the last(two)
                if (i < parameters.Length && parameters[i].IsList) {
                    i++;
                    fa |= FunctionAttributes.ArgumentList;
                }
                if (i < parameters.Length && parameters[i].IsDictionary) {
                    i++;
                    fa |= FunctionAttributes.KeywordDictionary;
                }
                
                // All parameters must now be exhausted
                Debug.Assert(i == parameters.Length);
            }
            return fa;
        }

        internal override bool TryBindOuter(SymbolId name, out PythonVariable variable) {
            // Functions expose their locals to direct access
            ContainsNestedFreeVariables = true;
            return TryGetVariable(name, out variable);
        }

        internal override PythonVariable BindName(PythonNameBinder binder, SymbolId name) {
            PythonVariable variable;

            // First try variables local to this scope
            if (TryGetVariable(name, out variable)) {
                return variable;
            }

            // Try to bind in outer scopes
            for (ScopeStatement parent = Parent; parent != null; parent = parent.Parent) {
                if (parent.TryBindOuter(name, out variable)) {
                    IsClosure = true;
                    variable.AccessedInNestedScope = true;
                    return variable;
                }
            }

            // Unbound variable
            if (ContainsUnqualifiedExec || ContainsImportStar || NeedsLocalsDictionary) {
                // If the context contains unqualified exec, new locals can be introduced
                // We introduce the locals for every free variable to optimize the name based
                // lookup.
                EnsureHiddenVariable(name);
                return null;
            } else {
                // Create a global variable to bind to.
                return GetGlobalScope().EnsureGlobalVariable(binder, name);
            }
        }

        internal override void Bind(PythonNameBinder binder) {
            base.Bind(binder);
            Verify(binder);
        }

        private void Verify(PythonNameBinder binder) {
            if (ContainsImportStar && IsClosure) {
                binder.ReportSyntaxError(
                    String.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "import * is not allowed in function '{0}' because it is a nested function",
                        SymbolTable.IdToString(Name)),
                    this);
            }
            if (ContainsImportStar && Parent is FunctionDefinition) {
                binder.ReportSyntaxError(
                    String.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "import * is not allowed in function '{0}' because it is a nested function",
                        SymbolTable.IdToString(Name)),
                    this);
            }
            if (ContainsImportStar && ContainsNestedFreeVariables) {
                binder.ReportSyntaxError(
                    String.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "import * is not allowed in function '{0}' because it contains a nested function with free variables",
                        SymbolTable.IdToString(Name)),
                    this);
            }
            if (ContainsUnqualifiedExec && ContainsNestedFreeVariables) {
                binder.ReportSyntaxError(
                    String.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "unqualified exec is not allowed in function '{0}' because it contains a nested function with free variables",
                        SymbolTable.IdToString(Name)),
                    this);
            }
            if (ContainsUnqualifiedExec && IsClosure) {
                binder.ReportSyntaxError(
                    String.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "unqualified exec is not allowed in function '{0}' because it is a nested function",
                        SymbolTable.IdToString(Name)),
                    this);
            }
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            Debug.Assert(_variable != null, "Shouldn't be called by lamda expression");

            ag.DisableInterpreter = true;
            MSAst.Expression function = TransformToFunctionExpression(ag);
            return ag.AddDebugInfo(ag.Globals.Assign(ag.Globals.GetVariable(_variable), function), new SourceSpan(Start, Header));
        }

        private static int _lambdaId;

        internal MSAst.Expression TransformToFunctionExpression(AstGenerator ag) {
            string name;

            if (IsLambda) {
                name = "<lambda$" + Interlocked.Increment(ref _lambdaId) + ">";
            } else {
                name = SymbolTable.IdToString(_name);
            }

            // Create AST generator to generate the body with
            AstGenerator bodyGen = new AstGenerator(ag, name, IsGenerator, false);
            if (NeedsLocalsDictionary || ContainsNestedFreeVariables) {
                bodyGen.CreateNestedContext();
            }

            FunctionAttributes flags = ComputeFlags(_parameters);
            bool needsWrapperMethod = flags != FunctionAttributes.None || _parameters.Length > PythonCallTargets.MaxArgs;
            
            // Transform the parameters.
            // Populate the list of the parameter names and defaults.
            List<MSAst.Expression> defaults = new List<MSAst.Expression>(0);
            List<MSAst.Expression> names = new List<MSAst.Expression>();

            TransformParameters(ag, bodyGen, defaults, names, needsWrapperMethod);
        
            bodyGen.DisableInterpreter = true;

            List<MSAst.Expression> statements = new List<MSAst.Expression>();

            // Create variables and references. Since references refer to
            // parameters, do this after parameters have been created.
            CreateVariables(bodyGen, statements);

            // Initialize parameters - unpack tuples.
            // Since tuples unpack into locals, this must be done after locals have been created.
            InitializeParameters(bodyGen, statements, needsWrapperMethod);

            // For generators, we need to do a check before the first statement for Generator.Throw() / Generator.Close().
            // The exception traceback needs to come from the generator's method body, and so we must do the check and throw
            // from inside the generator.
            if (IsGenerator) {
                MSAst.Expression s1 = YieldExpression.CreateCheckThrowExpression(bodyGen, SourceSpan.None);
                statements.Add(s1);
            }

            MSAst.ParameterExpression extracted = null;
            if (!IsGenerator && _canSetSysExcInfo) {
                // need to allocate the exception here so we don't share w/ exceptions made & freed
                // during the body.
                extracted = bodyGen.GetTemporary("$ex", typeof(Exception));
            }

            // Transform the body and add the resulting statements into the list
            TransformBody(bodyGen, statements);

            if (ag.DebugMode) {
                // add beginning and ending break points for the function.
                if (GetExpressionEnd(statements[statements.Count - 1]) != Body.End) {
                    statements.Add(ag.AddDebugInfo(AstUtils.Empty(), new SourceSpan(Body.End, Body.End)));
                }
            }

            MSAst.Expression body = Ast.Block(new ReadOnlyCollection<MSAst.Expression>(statements.ToArray()));

            // If this function can modify sys.exc_info() (_canSetSysExcInfo), then it must restore the result on finish.
            // 
            // Wrap in 
            //   $temp = PythonOps.SaveCurrentException()
            //   <body>
            //   PythonOps.RestoreCurrentException($temp)
            // Skip this if we're a generator. For generators, the try finally is handled by the PythonGenerator class 
            //  before it's invoked. This is because the restoration must occur at every place the function returns from 
            //  a yield point. That's different than the finally semantics in a generator.
            if (extracted != null) {
                MSAst.Expression s = AstUtils.Try(
                    Ast.Assign(
                        extracted,
                        Ast.Call(
                            AstGenerator.GetHelperMethod("SaveCurrentException")
                        )
                    ),
                    body
                ).Finally(
                    Ast.Call(
                        AstGenerator.GetHelperMethod("RestoreCurrentException"), extracted
                    )
                );
                body = s;
            }

            body = bodyGen.WrapScopeStatements(body);
            body = bodyGen.AddReturnTarget(body);

            if (_canSetSysExcInfo) {
                flags |= FunctionAttributes.CanSetSysExcInfo;
            }
            MSAst.Expression bodyStmt = bodyGen.MakeBody(body, NeedsLocalsDictionary, true);
            if (ContainsTryFinally) {
                flags |= FunctionAttributes.ContainsTryFinally;
            }

            MSAst.Expression code;
            if (IsGenerator) {
                code = AstUtils.GeneratorLambda(
                    GetGeneratorDelegateType(_parameters, needsWrapperMethod),
                    bodyGen.GeneratorLabel,
                    bodyStmt,
                    bodyGen.Name + "$" + _lambdaId++,
                    bodyGen.Parameters
                );
                flags |= FunctionAttributes.Generator;
            } else {
                code = Ast.Lambda(
                    GetDelegateType(_parameters, needsWrapperMethod),
                    AstGenerator.AddDefaultReturn(bodyStmt, typeof(object)),
                    bodyGen.Name + "$" + _lambdaId++,
                    bodyGen.Parameters
                );
            }

            MSAst.Expression ret = Ast.Call(
                null,                                                                                   // instance
                typeof(PythonOps).GetMethod("MakeFunction"),                                            // method
                new ReadOnlyCollection<MSAst.Expression>(
                    new [] {
                        ag.LocalContext,                                                                // 1. Emit CodeContext
                        AstUtils.Constant(name),                                                             // 2. FunctionName
                        code,                                                                           // 3. delegate
                        names.Count == 0 ?                                                              // 4. parameter names
                            AstUtils.Constant(null, typeof(string[])) :
                            (MSAst.Expression)Ast.NewArrayInit(typeof(string), names),
                        defaults.Count == 0 ?                                                           // 5. default values
                            AstUtils.Constant(null, typeof(object[])) :
                            (MSAst.Expression)Ast.NewArrayInit(typeof(object), defaults),                                   
                        AstUtils.Constant(flags),                                                            // 6. flags
                        AstUtils.Constant(ag.GetDocumentation(_body), typeof(string)),                       // 7. doc string or null
                        AstUtils.Constant(this.Start.Line),                                                  // 8. line number
                        AstUtils.Constant(_sourceUnit.Path, typeof(string))                                  // 9. filename
                    }
                )
            );

            ret = ag.AddDecorators(ret, _decorators);

            return ret;
        }

        private SourceLocation GetExpressionEnd(MSAst.Expression expression) {
            return SourceLocation.None;
        }

        private void TransformParameters(AstGenerator outer, AstGenerator inner, List<MSAst.Expression> defaults, List<MSAst.Expression> names, bool needsWrapperMethod) {
            if (inner.IsGenerator) {
                inner.CreateGeneratorParameter();
            }

            if (needsWrapperMethod) {
                // define a single parameter which takes all arguments
                inner.Parameter(typeof(object[]), "allArgs");
            }

            for (int i = 0; i < _parameters.Length; i++) {
                // Create the parameter in the inner code block
                Parameter p = _parameters[i];
                p.Transform(inner, needsWrapperMethod);

                // Transform the default value
                if (p.DefaultValue != null) {
                    defaults.Add(
                        outer.TransformAndConvert(p.DefaultValue, typeof(object))
                    );
                }

                names.Add(
                    AstUtils.Constant(
                        SymbolTable.IdToString(p.Name)
                    )
                );
            }
        }

        private void InitializeParameters(AstGenerator ag, List<MSAst.Expression> init, bool needsWrapperMethod) {
            for (int i = 0; i < _parameters.Length; i++) {
                Parameter p = _parameters[i];
                if (needsWrapperMethod) {
                    // if our method signature is object[] we need to first unpack the argument
                    // from the incoming array.
                    init.Add(
                        Ast.Assign(
                            ag.Globals.GetVariable(p.Variable),
                            Ast.ArrayIndex(
                                IsGenerator ? ag.Parameters[1] : ag.Parameters[0],
                                Ast.Constant(i)
                            )
                        )
                    );
                }

                p.Init(ag, init);
            }
        }

        private void TransformBody(AstGenerator ag, List<MSAst.Expression> statements) {
            SuiteStatement suite = _body as SuiteStatement;

            // Special case suite statement to avoid unnecessary allocation of extra node.
            if (suite != null) {
                foreach (Statement one in suite.Statements) {
                    MSAst.Expression transforned = ag.Transform(one);
                    if (transforned != null) {
                        statements.Add(transforned);
                    }
                }
            } else {
                MSAst.Expression transformed = ag.Transform(_body);
                if (transformed != null) {
                    statements.Add(transformed);
                }
            }
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_parameters != null) {
                    foreach (Parameter p in _parameters) {
                        p.Walk(walker);
                    }
                }
                if (_decorators != null) {
                    foreach (Expression decorator in _decorators) {
                        decorator.Walk(walker);
                    }
                }
                if (_body != null) {
                    _body.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }

        /// <summary>
        /// Determines delegate type for the Python function
        /// </summary>
        private static Type GetDelegateType(Parameter[] parameters, bool wrapper) {
            return PythonCallTargets.GetPythonTargetType(wrapper, parameters.Length);
        }

        private static Type GetGeneratorDelegateType(Parameter[] parameters, bool wrapper) {
            return PythonCallTargets.GetGeneratorTargetType(wrapper, parameters.Length);
        }

        internal override bool CanThrow {
            get {
                return false;
            }
        }
    }
}
