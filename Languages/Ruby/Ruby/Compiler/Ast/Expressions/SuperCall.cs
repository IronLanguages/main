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

using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Runtime.Calls;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;

    /// <summary>
    /// super(args)
    /// super
    /// 
    /// The former case passes the arguments explicitly.
    /// The latter passes all of the arguments that were passed to the current method (including the block, if any)
    /// 
    /// Also works from a method defined using define_method.
    /// </summary>
    public partial class SuperCall : CallExpression {
        /// <summary>
        /// All non-block arguments are passed implicitly.
        /// </summary>
        public bool HasImplicitArguments {
            get { return Arguments == null; }
        }

        public SuperCall(Arguments args, Block block, SourceSpan location)
            : base(args, block, location) {
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            // variable assigned to the transformed block in MakeCallWithBlockRetryable:
            MSA.Expression blockArgVariable = gen.CurrentScope.DefineHiddenVariable("#super-call-block", typeof(Proc));

            // invoke super member action:
            var siteBuilder = new CallSiteBuilder(gen, gen.CurrentSelfVariable, blockArgVariable);

            // arguments:
            if (HasImplicitArguments) {
                // MRI 1.8: If a block is called via define_method stub its parameters are used here.
                // MRI 1.9: This scenario is not supported.
                // We don't support this either. Otherwise we would need to emit super call with dynamic parameters if gen.CurrentBlock != null.

                if (gen.CurrentMethod.Parameters != null) {
                    gen.CurrentMethod.Parameters.TransformForSuperCall(gen, siteBuilder);
                } else if (gen.CompilerOptions.TopLevelParameterNames != null) {
                    bool hasUnsplat = gen.CompilerOptions.TopLevelHasUnsplatParameter;
                    string[] names = gen.CompilerOptions.TopLevelParameterNames;

                    // dynamic lookup:
                    for (int i = 0; i < names.Length - (hasUnsplat ? 1 : 0); i++) {
                        siteBuilder.Add(Methods.GetLocalVariable.OpCall(gen.CurrentScopeVariable, AstUtils.Constant(names[i])));
                    }
                    if (hasUnsplat) {
                        siteBuilder.SplattedArgument =
                            Methods.GetLocalVariable.OpCall(gen.CurrentScopeVariable, AstUtils.Constant(names[names.Length - 1]));
                    }
                } else {
                    // this means we are not inside any method scope -> an exception will be thrown at the call site
                }
            } else {
                Arguments.TransformToCall(gen, siteBuilder);
            }

            // block:
            MSA.Expression transformedBlock;
            if (Block != null) {
                transformedBlock = Block.Transform(gen);
            } else {
                transformedBlock = gen.MakeMethodBlockParameterRead();
            }
            
            return gen.DebugMark(
                MethodCall.MakeCallWithBlockRetryable(gen,
                    siteBuilder.MakeSuperCallAction(gen.CurrentFrame.UniqueId, HasImplicitArguments), 
                    blockArgVariable, 
                    transformedBlock,
                    Block != null && Block.IsDefinition
                ),
                "#RB: super call ('" + gen.CurrentMethod.MethodName + "')"
            );
        }

        internal override MSA.Expression/*!*/ TransformDefinedCondition(AstGenerator/*!*/ gen) {
            // MRI doesn't evaluate the arguments 
            return AstUtils.LightDynamic(
                SuperCallAction.Make(gen.Context, RubyCallSignature.IsDefined(true), gen.CurrentFrame.UniqueId),
                typeof(bool),
                gen.CurrentScopeVariable,
                gen.CurrentSelfVariable
            );
        }

        internal override string/*!*/ GetNodeName(AstGenerator/*!*/ gen) {
            return "super";
        }
    }
}
