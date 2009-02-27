/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Diagnostics;
using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;
    using MSA = System.Linq.Expressions;

    /// <summary>
    /// target.method_id(args)
    /// </summary>
    public partial class MethodCall : CallExpression {
        private string/*!*/ _methodName;
        private readonly Expression _target;

        public string/*!*/ MethodName {
            get { return _methodName; }
        }

        public Expression Target {
            get { return _target; }
        }

        public MethodCall(Expression target, string/*!*/ methodName, Arguments args, SourceSpan location)
            : this(target, methodName, args, null, location) {
        }
    
        public MethodCall(Expression target, string/*!*/ methodName, Arguments args, Block block, SourceSpan location)
            : base(args, block, location) {
            Assert.NotEmpty(methodName);

            _methodName = methodName;
            _target = target;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            MSA.Expression transformedTarget;
            bool hasImplicitSelf;
            if (_target != null) {
                transformedTarget = _target.TransformRead(gen);
                hasImplicitSelf = false;
            } else {
                transformedTarget = gen.CurrentSelfVariable;
                hasImplicitSelf = true;
            }
            return TransformRead(this, gen, hasImplicitSelf, _methodName, transformedTarget, Arguments, Block, null, null);
        }

        // arguments: complex arguments (expressions, maplets, splat, block) 
        // singleArgument: siple argument (complex are not used)
        // assignmentRhsArgument: rhs of the assignment: target.method=(rhs)
        internal static MSA.Expression/*!*/ TransformRead(Expression/*!*/ node, AstGenerator/*!*/ gen, bool hasImplicitSelf, 
            string/*!*/ methodName, MSA.Expression/*!*/ transformedTarget,
            Arguments arguments, Block block, MSA.Expression singleArgument, MSA.Expression assignmentRhsArgument) {

            Debug.Assert(assignmentRhsArgument == null || block == null, "Block not allowed in assignment");
            Debug.Assert(singleArgument == null || arguments == null && assignmentRhsArgument == null);
            Assert.NotNull(gen, transformedTarget);
            Assert.NotEmpty(methodName);

            // Pass args in this order:
            // 1. instance
            // 2. block (if present)
            // 3. passed args: normal args, maplets, array
            // 4. RHS of assignment (if present)

            CallBuilder callBuilder = new CallBuilder(gen);
            callBuilder.Instance = transformedTarget;
            
            MSA.Expression blockArgVariable = null;
            MSA.Expression transformedBlock = null;

            if (block != null) {
                blockArgVariable = gen.CurrentScope.DefineHiddenVariable("#block-def", typeof(Proc));
                transformedBlock = block.Transform(gen);
                callBuilder.Block = blockArgVariable;
            }

            if (arguments != null) {
                arguments.TransformToCall(gen, callBuilder);
            } else if (singleArgument != null) {
                callBuilder.Add(singleArgument);
            }

            MSA.Expression rhsVariable = null;
            if (assignmentRhsArgument != null) {
                rhsVariable = gen.CurrentScope.DefineHiddenVariable("#rhs", assignmentRhsArgument.Type);
                callBuilder.RhsArgument = Ast.Assign(rhsVariable, assignmentRhsArgument);
            }

            var dynamicSite = callBuilder.MakeCallAction(methodName, hasImplicitSelf);
            gen.TraceCallSite(node, dynamicSite);

            MSA.Expression result = gen.DebugMark(dynamicSite, methodName);

            if (block != null) {
                result = gen.DebugMark(MakeCallWithBlockRetryable(gen, result, blockArgVariable, transformedBlock, block.IsDefinition),
                    "#RB: method call with a block ('" + methodName + "')");
            }

            if (assignmentRhsArgument != null) {
                result = AstFactory.Block(result, rhsVariable);
            }

            return result;
        }

        internal static MSA.Expression/*!*/ MakeCallWithBlockRetryable(AstGenerator/*!*/ gen, MSA.Expression/*!*/ invoke,
            MSA.Expression blockArgVariable, MSA.Expression transformedBlock, bool isBlockDefinition) {
            Assert.NotNull(invoke);
            Debug.Assert((blockArgVariable == null) == (transformedBlock == null));

            // see Ruby Language.doc/Control Flow Implementation/Method Call With a Block
            MSA.Expression resultVariable = gen.CurrentScope.DefineHiddenVariable("#method-result", typeof(object));
            MSA.ParameterExpression evalUnwinder = gen.CurrentScope.DefineHiddenVariable("#unwinder", typeof(EvalUnwinder));

            MSA.LabelTarget label = Ast.Label();
                    
            return AstFactory.Block(
                Ast.Assign(blockArgVariable, Ast.Convert(transformedBlock, blockArgVariable.Type)),
                AstFactory.Infinite(label, null,
                    (!isBlockDefinition) ?
                        (MSA.Expression)AstUtils.Empty() : 
                        (MSA.Expression)Methods.InitializeBlock.OpCall(blockArgVariable),

                    AstUtils.Try(
                        Ast.Assign(resultVariable, invoke)
                    ).Catch(evalUnwinder,
                        Ast.Assign(
                            resultVariable, 
                            Ast.Field(evalUnwinder, EvalUnwinder.ReturnValueField)
                        )
                    ),

                    // if result != RetrySingleton then break end
                    AstUtils.Unless(Methods.IsRetrySingleton.OpCall(AstFactory.Box(resultVariable)), Ast.Break(label)),

                    // if blockParam == #block then retry end
                    (gen.CurrentMethod.IsTopLevelCode) ? AstUtils.Empty() :
                        AstUtils.IfThen(Ast.Equal(gen.MakeMethodBlockParameterRead(), blockArgVariable), RetryStatement.TransformRetry(gen))
                
                ),
                resultVariable
            );
        }

        internal override MSA.Expression TransformDefinedCondition(AstGenerator/*!*/ gen) {
            if (_target != null) {
                return gen.TryCatchAny(
                    Methods.IsDefinedMethod.OpCall(
                        AstFactory.Box(_target.TransformRead(gen)), gen.CurrentScopeVariable, AstUtils.Constant(_methodName)
                    ),
                    AstUtils.Constant(false)
                );
            } else {
                return Methods.IsDefinedMethod.OpCall(gen.CurrentSelfVariable, gen.CurrentScopeVariable, AstUtils.Constant(_methodName));
            }
        }

        internal override string/*!*/ GetNodeName(AstGenerator/*!*/ gen) {
            return "method";
        }
    }
}
