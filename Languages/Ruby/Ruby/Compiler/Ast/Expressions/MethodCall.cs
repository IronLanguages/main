/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

using System.Diagnostics;
using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    using AstBlock = Microsoft.Scripting.Ast.BlockBuilder;
    
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

            MSA.Expression blockArgVariable;
            MSA.Expression transformedBlock;

            if (block != null) {
                blockArgVariable = gen.CurrentScope.DefineHiddenVariable("#block-def", typeof(Proc));
                transformedBlock = block.Transform(gen);
            } else {
                blockArgVariable = transformedBlock = null;
            }

            var siteBuilder = new CallSiteBuilder(gen, transformedTarget, blockArgVariable);

            if (arguments != null) {
                arguments.TransformToCall(gen, siteBuilder);
            } else if (singleArgument != null) {
                siteBuilder.Add(singleArgument);
            }

            MSA.Expression rhsVariable = null;
            if (assignmentRhsArgument != null) {
                rhsVariable = gen.CurrentScope.DefineHiddenVariable("#rhs", assignmentRhsArgument.Type);
                siteBuilder.RhsArgument = Ast.Assign(rhsVariable, assignmentRhsArgument);
            }

            var dynamicSite = siteBuilder.MakeCallAction(methodName, hasImplicitSelf);
#if FEATURE_CALL_SITE_TRACER
            if (gen.Context.CallSiteCreated != null) {
                gen.Context.CallSiteCreated(node, dynamicSite);
            }
#endif

            MSA.Expression result = gen.DebugMark(dynamicSite, methodName);

            if (block != null) {
                result = gen.DebugMark(MakeCallWithBlockRetryable(gen, result, blockArgVariable, transformedBlock, block.IsDefinition),
                    "#RB: method call with a block ('" + methodName + "')");
            }

            if (assignmentRhsArgument != null) {
                result = Ast.Block(result, rhsVariable);
            }

            return result;
        }

        internal static MSA.Expression/*!*/ MakeCallWithBlockRetryable(AstGenerator/*!*/ gen, MSA.Expression/*!*/ invoke,
            MSA.Expression blockArgVariable, MSA.Expression transformedBlock, bool isBlockDefinition) {
            Assert.NotNull(invoke);
            Debug.Assert((blockArgVariable == null) == (transformedBlock == null));

            // see Ruby Language.doc/Control Flow Implementation/Method Call With a Block
            MSA.Expression resultVariable = gen.CurrentScope.DefineHiddenVariable("#method-result", typeof(object));
            MSA.ParameterExpression evalUnwinder;

            MSA.LabelTarget retryLabel = Ast.Label("retry");

            var result = new AstBlock {
                Ast.Assign(blockArgVariable, Ast.Convert(transformedBlock, blockArgVariable.Type)),

                Ast.Label(retryLabel),

                (isBlockDefinition) ? Methods.InitializeBlock.OpCall(blockArgVariable) : null,

                AstUtils.Try(
                    Ast.Assign(resultVariable, invoke)
                ).Catch(evalUnwinder = Ast.Parameter(typeof(EvalUnwinder), "#u"),
                    Ast.Assign(
                        resultVariable, 
                        Ast.Field(evalUnwinder, EvalUnwinder.ReturnValueField)
                    )
                ),

                Ast.IfThen(Ast.TypeEqual(resultVariable, typeof(BlockReturnResult)),
                    Ast.IfThenElse(Methods.IsRetrySingleton.OpCall(resultVariable),
                        // retry:
                        AstUtils.IfThenElse(Ast.Equal(gen.MakeMethodBlockParameterRead(), blockArgVariable),
                            RetryStatement.TransformRetry(gen),
                            Ast.Goto(retryLabel)
                        ),
                        // return:
                        gen.Return(ReturnStatement.Propagate(gen, resultVariable))
                    )
                ),

                resultVariable
            };

            return result;
        }

        internal override MSA.Expression TransformDefinedCondition(AstGenerator/*!*/ gen) {
            // MRI doesn't evaluate the arguments 
            MSA.Expression result = AstUtils.LightDynamic(
                RubyCallAction.Make(gen.Context, _methodName, RubyCallSignature.IsDefined(_target == null)), 
                typeof(bool),
                gen.CurrentScopeVariable,
                (_target != null) ? AstUtils.Box(_target.TransformRead(gen)) : gen.CurrentSelfVariable
            );

            return (_target != null) ? gen.TryCatchAny(result, AstFactory.False) : result;
        }

        internal override string/*!*/ GetNodeName(AstGenerator/*!*/ gen) {
            return "method";
        }
    }
}
