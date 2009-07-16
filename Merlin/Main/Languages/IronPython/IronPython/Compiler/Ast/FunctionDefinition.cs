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
using System.Text;
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Runtime.Operations;

using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = System.Linq.Expressions;

using Debugging = Microsoft.Scripting.Debugging;

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
        // the scope contains variables that are bound to parent scope forming a closure:
        private bool _closure;

        private PythonVariable _variable;               // The variable corresponding to the function name or null for lambdas
        internal PythonVariable _nameVariable;          // the variable that refers to the global __name__

        private List<SymbolId> _closureVars;

        private static MSAst.ParameterExpression _functionParam = Ast.Parameter(typeof(PythonFunction), "$function");
        internal static MSAst.ParameterExpression _functionStack = Ast.Variable(typeof(List<FunctionStack>), "funcStack");

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

        /// <summary>
        /// True if this scope accesses a variable from an outer scope.
        /// </summary>
        internal bool IsClosure {
            get { return _closure; }
            set { _closure = value; }
        }

        internal PythonVariable Variable {
            get { return _variable; }
            set { _variable = value; }
        }

        internal override bool ExposesLocalVariable(PythonVariable variable) {
            return NeedsLocalsDictionary; 
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
                    if (_closureVars == null) {
                        _closureVars = new List<SymbolId>();
                    }
                    if (!_closureVars.Contains(name)) {
                        _closureVars.Add(name);
                    }
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

        /// <summary>
        /// Pulls the closure tuple from our function/generator which is flowed into each function call.
        /// </summary>
        public override MSAst.Expression/*!*/ GetClosureTuple() {
            return MSAst.Expression.Call(
                null,
                typeof(PythonOps).GetMethod("GetClosureTupleFromFunction"),
                _functionParam
            );
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
            Debug.Assert(_variable != null, "Shouldn't be called by lambda expression");

            MSAst.Expression function = TransformToFunctionExpression(ag);
            return ag.AddDebugInfo(ag.Globals.Assign(ag.Globals.GetVariable(ag, _variable), function), new SourceSpan(Start, Header));
        }

        private static int _lambdaId;

        private string MakeProfilerName(string name) {
            var sb = new StringBuilder("def ");
            sb.Append(name);
            sb.Append('(');
            bool comma = false;
            foreach (var p in _parameters) {
                if (comma) {
                    sb.Append(", ");
                } else {
                    comma = true;
                }
                sb.Append(p.Name);
            }
            sb.Append(')');
            return sb.ToString();
        }

        internal MSAst.Expression TransformToFunctionExpression(AstGenerator ag) {
            string name;

            if (IsLambda) {
                name = "<lambda$" + Interlocked.Increment(ref _lambdaId) + ">";
            } else {
                name = SymbolTable.IdToString(_name);
            }

            if (ag.PyContext.PythonOptions.FullFrames) {
                // force a dictionary if we have enabled full frames for sys._getframe support
                NeedsLocalsDictionary = true;
            }

            // Create AST generator to generate the body with
            AstGenerator bodyGen = new AstGenerator(ag, name, IsGenerator, MakeProfilerName(name));

            FunctionAttributes flags = ComputeFlags(_parameters);
            bool needsWrapperMethod = _parameters.Length > PythonCallTargets.MaxArgs;
            
            // Transform the parameters.
            // Populate the list of the parameter names and defaults.
            List<MSAst.Expression> defaults = new List<MSAst.Expression>(0);
            List<MSAst.Expression> names = new List<MSAst.Expression>();

            List<MSAst.Expression> init = new List<MSAst.Expression>();
            TransformParameters(ag, bodyGen, defaults, names, needsWrapperMethod, init);

            MSAst.Expression parentContext;
            
            parentContext = MSAst.Expression.Call(typeof(PythonOps).GetMethod("GetParentContextFromFunction"), _functionParam);

            bodyGen.AddHiddenVariable(ArrayGlobalAllocator._globalContext);
            init.Add(Ast.Assign(ArrayGlobalAllocator._globalContext, Ast.Call(typeof(PythonOps).GetMethod("GetGlobalContext"), parentContext)));
            init.AddRange(bodyGen.Globals.PrepareScope(bodyGen));

            // Create variables and references. Since references refer to
            // parameters, do this after parameters have been created.
            CreateVariables(bodyGen, parentContext, init, NeedsLocalsDictionary, NeedsLocalsDictionary);

            // Initialize parameters - unpack tuples.
            // Since tuples unpack into locals, this must be done after locals have been created.
            InitializeParameters(bodyGen, init, needsWrapperMethod);

            List<MSAst.Expression> statements = new List<MSAst.Expression>();

            // For generators, we need to do a check before the first statement for Generator.Throw() / Generator.Close().
            // The exception traceback needs to come from the generator's method body, and so we must do the check and throw
            // from inside the generator.
            if (IsGenerator) {
                MSAst.Expression s1 = YieldExpression.CreateCheckThrowExpression(bodyGen, SourceSpan.None);
                statements.Add(s1);
            }

            if (NeedsLocalsDictionary || ContainsNestedFreeVariables) {
                bodyGen.CreateNestedContext();
            }

            MSAst.ParameterExpression extracted = null;
            if (!IsGenerator && _canSetSysExcInfo) {
                // need to allocate the exception here so we don't share w/ exceptions made & freed
                // during the body.
                extracted = bodyGen.GetTemporary("$ex", typeof(Exception));
            }

            // Transform the body and add the resulting statements into the list
            if (!TryTransformBody(bodyGen, statements)) {
                // there's an error in the body
                return null;
            }

            // add beginning sequence point
            statements.Insert(0, bodyGen.AddDebugInfo(
                AstUtils.Empty(),
                new SourceSpan(new SourceLocation(0, Start.Line, Start.Column), new SourceLocation(0, Start.Line, Int32.MaxValue))));

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

            if (_body.CanThrow && ag.PyContext.PythonOptions.Frames) {
                body = AddFrame(bodyGen.LocalContext, _functionParam, body);
                bodyGen.AddHiddenVariable(_functionStack);
            }

            body = bodyGen.AddProfiling(body);
            body = bodyGen.WrapScopeStatements(body);
            body = bodyGen.AddReturnTarget(body);

            if (_canSetSysExcInfo) {
                flags |= FunctionAttributes.CanSetSysExcInfo;
            }

            if (ContainsTryFinally) {
                flags |= FunctionAttributes.ContainsTryFinally;
            }

            if (IsGenerator) {
                flags |= FunctionAttributes.Generator;
            }

            MSAst.Expression bodyStmt = bodyGen.MakeBody(
                parentContext, 
                init.ToArray(), 
                body, 
                true
            );

            Delegate originalDelegate;
            MSAst.LambdaExpression code; code = Ast.Lambda(
                GetDelegateType(_parameters, needsWrapperMethod, out originalDelegate),
                AstGenerator.AddDefaultReturn(bodyStmt, typeof(object)),
                bodyGen.Name + "$" + _lambdaId++,
                bodyGen.Parameters
            );
            

            // create the function code object which all function instances will share
            MSAst.Expression funcCode = Ast.Constant(
                new FunctionCode(
                    ag.PyContext,
                    ag.EmitDebugSymbols ? null : originalDelegate,
                    code,
                    name,
                    ag.GetDocumentation(_body),
                    ArrayUtils.ConvertAll(_parameters, (val) => SymbolTable.IdToString(val.Name)),
                    flags,
                    Span,
                    _sourceUnit.Path,
                    ag.EmitDebugSymbols,
                    ag.ShouldInterpret,
                    _closureVars,
                    bodyGen.LoopLocationsNoCreate,
                    bodyGen.HandlerLocationsNoCreate
                )                
            );

            MSAst.Expression ret;
            if (ag.EmitDebugSymbols && !ag.PyContext.EnableTracing) {
                // we need to compile all of the debuggable code together at once otherwise mdbg gets confused.  If we're
                // in tracing mode we'll still compile things one off though just to keep things simple.  The code will still
                // be debuggable but naive debuggers like mdbg will have more issues.
                ret = Ast.Call(
                    typeof(PythonOps).GetMethod("MakeFunctionDebug"),                                    // method
                    ag.LocalContext,                                                                // 1. Emit CodeContext
                    funcCode,                                                                       // 2. FunctionCode
                    ((IPythonGlobalExpression)ag.Globals.GetVariable(ag, _nameVariable)).RawValue(),// 3. module name
                    defaults.Count == 0 ?                                                           // 4. default values
                        AstUtils.Constant(null, typeof(object[])) :
                        (MSAst.Expression)Ast.NewArrayInit(typeof(object), defaults),
                    IsGenerator ? 
                        (MSAst.Expression)new PythonGeneratorExpression(code) :
                        (MSAst.Expression)code
                );
            } else {
                ret = Ast.Call(
                    typeof(PythonOps).GetMethod("MakeFunction"),                                    // method
                    ag.LocalContext,                                                                // 1. Emit CodeContext
                    funcCode,                                                                       // 2. FunctionCode
                    ((IPythonGlobalExpression)ag.Globals.GetVariable(ag, _nameVariable)).RawValue(),// 3. module name
                    defaults.Count == 0 ?                                                           // 4. default values
                        AstUtils.Constant(null, typeof(object[])) :
                        (MSAst.Expression)Ast.NewArrayInit(typeof(object), defaults)
                );
            }

            ret = ag.AddDecorators(ret, _decorators);

            return ret;
        }

        /// <summary>
        /// Creates a method frame for tracking purposes and enforces recursion
        /// </summary>
        internal static MSAst.Expression AddFrame(MSAst.Expression localContext, MSAst.Expression function, MSAst.Expression body) {
            body = AstUtils.Try(
                Ast.Assign(
                    _functionStack,
                    Ast.Call(
                        typeof(PythonOps).GetMethod("PushFrame"),
                        localContext,
                        function
                    )
                ),
                body
            ).Finally(
                Ast.Call(
                    _functionStack,
                    typeof(List<FunctionStack>).GetMethod("RemoveAt"),
                    Ast.Add(
                        Ast.Property(
                            _functionStack,
                            "Count"
                        ),
                        Ast.Constant(-1)
                    )
                )                
            );

            return body;
        }

        private void TransformParameters(AstGenerator outer, AstGenerator inner, List<MSAst.Expression> defaults, List<MSAst.Expression> names, bool needsWrapperMethod, List<MSAst.Expression> init) {
            inner.Parameter(_functionParam);

            if (needsWrapperMethod) {
                // define a single parameter which takes all arguments
                inner.Parameter(typeof(object[]), "allArgs");
            }

            for (int i = 0; i < _parameters.Length; i++) {
                // Create the parameter in the inner code block
                Parameter p = _parameters[i];
                p.Transform(inner, needsWrapperMethod, NeedsLocalsDictionary, init);

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
                        ag.Globals.Assign(
                            ag.Globals.GetVariable(ag, p.Variable),
                            Ast.ArrayIndex(
                                ag.Parameters[1],
                                Ast.Constant(i)
                            )
                        )
                    );
                }

                p.Init(ag, init);
            }
        }

        private bool TryTransformBody(AstGenerator ag, List<MSAst.Expression> statements) {
            SuiteStatement suite = _body as SuiteStatement;

            // Special case suite statement to avoid unnecessary allocation of extra node.
            if (suite != null) {
                foreach (Statement one in suite.Statements) {
                    MSAst.Expression transforned = ag.Transform(one);
                    if (transforned != null) {
                        statements.Add(transforned);
                    } else {
                        return false;
                    }
                }
            } else {
                MSAst.Expression transformed = ag.Transform(_body);
                if (transformed != null) {
                    statements.Add(transformed);
                } else {
                    return false;
                }
            }

            return true;
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
        private static Type GetDelegateType(Parameter[] parameters, bool wrapper, out Delegate originalTarget) {
            return PythonCallTargets.GetPythonTargetType(wrapper, parameters.Length, out originalTarget);
        }

        internal override bool CanThrow {
            get {
                return false;
            }
        }
    }
}
