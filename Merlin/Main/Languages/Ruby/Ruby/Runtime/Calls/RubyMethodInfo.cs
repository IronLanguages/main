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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using System.Linq.Expressions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Compiler;
using MethodDeclaration = IronRuby.Compiler.Ast.MethodDeclaration;
using AstFactory = IronRuby.Compiler.Ast.AstFactory;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using Ast = System.Linq.Expressions.Expression;
using System.Reflection;

namespace IronRuby.Runtime.Calls {
    public sealed class RubyMethodInfo : RubyMemberInfo {
        // Delegate type for methods with many parameters.
        internal static readonly Type ParamsArrayDelegateType = typeof(Func<object, Proc, object[], object>);
        
        // Method AST:
        // - MethodDeclaration in normal execution mode (no -save flag), or after queried.
        // - MethodDeclaration.Serializable in -save mode.
        // - char[] in -load mode when the method's source code is deserialized from a compiled assembly.
        private object/*!*/ _ast;

        private readonly Delegate/*!*/ _method;
        private readonly string/*!*/ _definitionName;

        private readonly int _mandatoryParamCount;
        private readonly int _optionalParamCount;  
        private readonly bool _hasUnsplatParameter;

        public Delegate/*!*/ Method { get { return _method; } }
        public string/*!*/ DefinitionName { get { return _definitionName; } }
        public int MandatoryParamCount { get { return _mandatoryParamCount; } }
        public int OptionalParamCount { get { return _optionalParamCount; } }
        public bool HasUnsplatParameter { get { return _hasUnsplatParameter; } }

        // method:
        internal RubyMethodInfo(object/*!*/ ast, Delegate/*!*/ method, RubyModule/*!*/ declaringModule, 
            string/*!*/ definitionName, int mandatory, int optional, bool hasUnsplatParameter, RubyMemberFlags flags)
            : base(flags, declaringModule) {
            Assert.NotNull(ast, method, declaringModule, definitionName);

            _ast = ast;
            _method = method;
            _mandatoryParamCount = mandatory;
            _optionalParamCount = optional;
            _hasUnsplatParameter = hasUnsplatParameter;
            _definitionName = definitionName;
        }

        protected internal override RubyMemberInfo/*!*/ Copy(RubyMemberFlags flags, RubyModule/*!*/ module) {
            return new RubyMethodInfo(_ast, _method, module, _definitionName, _mandatoryParamCount, _optionalParamCount, 
                _hasUnsplatParameter, flags
            );
        }
        
        internal override bool IsRemovable {
            get { return true; }
        }

        public override RubyMemberInfo TrySelectOverload(Type/*!*/[]/*!*/ parameterTypes) {
            return parameterTypes.Length >= MandatoryParamCount 
                && (HasUnsplatParameter || parameterTypes.Length <= MandatoryParamCount + OptionalParamCount)
                && CollectionUtils.TrueForAll(parameterTypes, (type) => type == typeof(object)) ? this : null;
        }

        public override MemberInfo/*!*/[]/*!*/ GetMembers() {
            return new MemberInfo[] { _method.Method };
        }

        public override int GetArity() {
            if (_optionalParamCount > 0) {
                return -_mandatoryParamCount - 1;
            } else {
                return _mandatoryParamCount;
            }
        }

        public MethodDeclaration/*!*/ GetSyntaxTree() {
            // live tree:
            var tree = _ast as MethodDeclaration;
            if (tree != null) {
                return tree;
            }

            // live tree wrapped into IExpressionSerializable class:
            var serializableTree = _ast as MethodDeclaration.Serializable;
            if (serializableTree != null) {
                _ast = serializableTree.Method;
                return serializableTree.Method;
            }

            // TODO: serialized tree:
            throw new NotImplementedException();
        }

        #region Dynamic Sites

        internal override void BuildCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            Assert.NotNull(metaBuilder, args, name);

            // any user method can yield to a block (regardless of whether block parameter is present or not):
            metaBuilder.ControlFlowBuilder = RuleControlFlowBuilder;

            // 2 implicit args: self, block
            var argsBuilder = new ArgsBuilder(2, _mandatoryParamCount, _optionalParamCount, _hasUnsplatParameter);
            argsBuilder.SetImplicit(0, AstFactory.Box(args.TargetExpression));
            argsBuilder.SetImplicit(1, args.Signature.HasBlock ? AstUtils.Convert(args.GetBlockExpression(), typeof(Proc)) : AstFactory.NullOfProc);
            argsBuilder.AddCallArguments(metaBuilder, args);

            if (metaBuilder.Error) {
                return;
            }

            // box explicit arguments:
            var boxedArguments = argsBuilder.GetArguments();
            for (int i = 2; i < boxedArguments.Length; i++) {
                boxedArguments[i] = AstFactory.Box(boxedArguments[i]);
            }

            if (_method.GetType() == ParamsArrayDelegateType) {
                // Func<object, Proc, object[], object>
                metaBuilder.Result = AstFactory.CallDelegate(_method, new[] { 
                    boxedArguments[0], 
                    boxedArguments[1], 
                    Ast.NewArrayInit(typeof(object), ArrayUtils.ShiftLeft(boxedArguments, 2)) 
                });
            } else {
                metaBuilder.Result = AstFactory.CallDelegate(_method, boxedArguments);
            }
        }

        /// <summary>
        /// Takes current result and wraps it into try-filter(MethodUnwinder)-finally block that ensures correct "break" behavior for 
        /// Ruby method calls with a block given in arguments.
        /// 
        /// Sets up a RFC frame similarly to MethodDeclaration.
        /// </summary>
        public static void RuleControlFlowBuilder(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            // TODO (improvement):
            // We don't special case null block here, although we could (we would need a test for that then).
            // We could also statically know (via call-site flag) that the current method is not a proc-converter (passed by ref),
            // which would make such calls faster.
            if (metaBuilder.Error || !args.Signature.HasBlock) {
                return;
            } 

            Expression rfcVariable = metaBuilder.GetTemporary(typeof(RuntimeFlowControl), "#rfc");
            ParameterExpression methodUnwinder = metaBuilder.GetTemporary(typeof(MethodUnwinder), "#unwinder");
            Expression resultVariable = metaBuilder.GetTemporary(typeof(object), "#result");

            metaBuilder.Result = Ast.Block(
                // initialize frame (RFC):
                Ast.Assign(rfcVariable, Methods.CreateRfcForMethod.OpCall(AstUtils.Convert(args.GetBlockExpression(), typeof(Proc)))),
                AstUtils.Try(
                    Ast.Assign(resultVariable, metaBuilder.Result)
                ).Filter(methodUnwinder, Ast.Equal(Ast.Field(methodUnwinder, MethodUnwinder.TargetFrameField), rfcVariable),

                    // return unwinder.ReturnValue;
                    Ast.Assign(resultVariable, Ast.Field(methodUnwinder, MethodUnwinder.ReturnValueField))

                ).Finally(
                    // we need to mark the RFC dead snce the block might escape and break later:
                    Methods.LeaveMethodFrame.OpCall(rfcVariable)
                ), 
                resultVariable
            );
        }

        #endregion
    }
}
