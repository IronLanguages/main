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
using System.Diagnostics;
using System.Dynamic.Binders;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Compiler;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;

using Ast = System.Linq.Expressions.Expression;
using AstFactory = IronRuby.Compiler.Ast.AstFactory;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Builtins {

    public enum ProcKind {
        Block,
        Proc,
        Lambda
    }

    public partial class Proc : IDuplicable {
        // Self object captured by the block definition, if any.
        // Although we could load self from scope in Ruby defined blocks, we cannot do so when we don't have a scope.
        private readonly object _self;

        // Local scope inside the proc captured by the block definition:
        private readonly RubyScope/*!*/ _scope;

        private readonly BlockDispatcher/*!*/ _dispatcher;
        private ProcKind _kind;

        // we need to remember the block's owner and proc-converter frames:
        private RuntimeFlowControl _owner;
        private RuntimeFlowControl _converter;

        internal RuntimeFlowControl Owner { get { return _owner; } set { _owner = value; } }
        internal RuntimeFlowControl Converter { get { return _converter; } set { _converter = value; } }

        public ProcKind Kind {
            get { return _kind; }
            // friend: RuntimeFlowControl
            internal set { _kind = value; }
        }

        public BlockDispatcher/*!*/ Dispatcher {
            get { return _dispatcher; }
        }

        public object Self {
            get { return _self; }
        }

        public RubyScope/*!*/ LocalScope {
            get { return _scope; }
        }

        internal static PropertyInfo/*!*/ SelfProperty { get { return typeof(Proc).GetProperty("Self"); } }

        #region Construction, Conversion

        internal Proc(ProcKind kind, object self, RubyScope/*!*/ scope, BlockDispatcher/*!*/ dispatcher) {
            Assert.NotNull(scope, dispatcher);
            _kind = kind;
            _self = self;
            _scope = scope;
            _dispatcher = dispatcher;
        }

        protected Proc(Proc/*!*/ proc)
            : this(proc.Kind, proc.Self, proc.LocalScope, proc.Dispatcher) {
            _owner = proc._owner;
            _converter = proc._converter;
        }

        /// <summary>
        /// Creates a copy of the proc that has the same target, context and self object as this block.
        /// </summary>
        public Proc/*!*/ Create(Proc/*!*/ proc) {
            return new Proc(proc);
        }

        /// <summary>
        /// Creates a lambda Proc that has the same target, context and self object as this block.
        /// Doesn't preserve the class of the Proc.
        /// </summary>
        public Proc/*!*/ ToLambda() {
            Proc result = new Proc(this);
            result.Kind = ProcKind.Lambda;
            return result;
        }

        /// <summary>
        /// Creates a copy of the proc that has the same target, context, self object as this instance.
        /// Doesn't copy instance data.
        /// Preserves the class of the Proc.
        /// </summary>
        public virtual Proc/*!*/ Copy() {
            return new Proc(this);
        }

        object IDuplicable.Duplicate(RubyContext/*!*/ context, bool copySingletonMembers) {
            var result = Copy();
            context.CopyInstanceData(this, result, copySingletonMembers);
            return result;
        }

        public static RubyMemberInfo/*!*/ ToLambdaMethodInfo(Proc/*!*/ lambda, string/*!*/ definitionName, RubyMethodVisibility visibility,
            RubyModule/*!*/ owner) {
            return new RubyLambdaMethodInfo(lambda, definitionName, (RubyMemberFlags)visibility, owner);
        }

        #endregion

        #region Dynamic Operations

        internal static void SetCallActionRule(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, bool testTarget) {
            Assert.NotNull(metaBuilder, args);

            var convertedTarget = AstUtils.Convert(args.TargetExpression, typeof(Proc));

            // test for target type:
            if (testTarget) {
                metaBuilder.AddTypeRestriction(args.Target.GetType(), args.TargetExpression);
            }

            SetProcCallRule(
                metaBuilder,
                convertedTarget,                              // proc object  
                Ast.Property(convertedTarget, SelfProperty),  // self captured by the block closure
                null,
                args
            );
        }

        /// <summary>
        /// From control flow perspective it "calls" the proc.
        /// </summary>
        internal static void SetProcCallRule(
            MetaObjectBuilder/*!*/ metaBuilder,
            Expression/*!*/ procExpression,     // proc object
            Expression/*!*/ selfExpression,     // self passed to the proc
            Expression callingMethodExpression, // RubyLambdaMethodInfo passed to the proc via BlockParam
            CallArguments/*!*/ args             // user arguments passed to the proc
        ) {
            var bfcVariable = metaBuilder.GetTemporary(typeof(BlockParam), "#bfc");
            var resultVariable = metaBuilder.GetTemporary(typeof(object), "#result");

            metaBuilder.Result = AstFactory.Block(
                Ast.Assign(bfcVariable,
                    (callingMethodExpression != null) ?
                        Methods.CreateBfcForMethodProcCall.OpCall(
                            AstUtils.Convert(procExpression, typeof(Proc)),
                            callingMethodExpression
                        ) :
                        Methods.CreateBfcForProcCall.OpCall(
                            AstUtils.Convert(procExpression, typeof(Proc))
                        )
                ),
                Ast.Assign(resultVariable, AstFactory.YieldExpression(
                    args.GetSimpleArgumentExpressions(),
                    args.GetSplattedArgumentExpression(),
                    args.GetRhsArgumentExpression(),
                    bfcVariable,
                    selfExpression
                )),
                Methods.MethodProcCall.OpCall(bfcVariable, resultVariable),
                resultVariable
            );
        }

        #region Call // TODO: generate

        // Call overloads don't check parameter count, this is done by Proc#call.

        public object Call() {
            var blockParam = RubyOps.CreateBfcForProcCall(this);
            var result = RubyOps.Yield0(_self, blockParam);
            RubyOps.MethodProcCall(blockParam, result);
            return result;
        }

        public object Call(object arg1) {
            var blockParam = RubyOps.CreateBfcForProcCall(this);

            // lambda calls are weird:
            var result = (_kind == ProcKind.Lambda) ?
                RubyOps.YieldNoAutoSplat1(arg1, _self, blockParam) :
                RubyOps.Yield1(arg1, _self, blockParam);

            RubyOps.MethodProcCall(blockParam, result);
            return result;
        }

        public object Call(object arg1, object arg2) {
            var blockParam = RubyOps.CreateBfcForProcCall(this);
            var result = RubyOps.Yield2(arg1, arg2, _self, blockParam);
            RubyOps.MethodProcCall(blockParam, result);
            return result;
        }

        public object Call(object arg1, object arg2, object arg3) {
            var blockParam = RubyOps.CreateBfcForProcCall(this);
            var result = RubyOps.Yield3(arg1, arg2, arg3, _self, blockParam);
            RubyOps.MethodProcCall(blockParam, result);
            return result;
        }

        public object Call(object arg1, object arg2, object arg3, object arg4) {
            var blockParam = RubyOps.CreateBfcForProcCall(this);
            var result = RubyOps.Yield4(arg1, arg2, arg3, arg4, _self, blockParam);
            RubyOps.MethodProcCall(blockParam, result);
            return result;
        }

        public object Call(params object[]/*!*/ args) {
            switch (args.Length) {
                case 0: return Call();
                case 1: return Call(args[0]);
                case 2: return Call(args[0], args[1]);
                case 3: return Call(args[0], args[1], args[2]);
                case 4: return Call(args[0], args[1], args[2], args[3]);
            }

            var blockParam = RubyOps.CreateBfcForProcCall(this);
            var result = RubyOps.YieldN(args, _self, blockParam);
            RubyOps.MethodProcCall(blockParam, result);
            return result;
        }

        public object CallN(object[]/*!*/ args) {
            Debug.Assert(args.Length > 4);
            var blockParam = RubyOps.CreateBfcForProcCall(this);
            var result = RubyOps.YieldN(args, _self, blockParam);
            RubyOps.MethodProcCall(blockParam, result);
            return result;
        }

        #endregion

        #endregion

        #region Block helper methods

        // TODO: remove context, rename to CreateBlock
        public static Proc/*!*/ Create(RubyContext/*!*/ context, Func<BlockParam, object, object>/*!*/ clrMethod) {
            return Create(context, (BlockCallTarget1)BlockCallback1, clrMethod, 1);
        }

        public static Proc/*!*/ Create(RubyContext/*!*/ context, Func<BlockParam, object, object, object>/*!*/ clrMethod) {
            return Create(context, (BlockCallTarget2)BlockCallback2, clrMethod, 2);
        }

        public static Proc/*!*/ Create(RubyContext/*!*/ context, Func<BlockParam, object, object, object, object>/*!*/ clrMethod) {
            return Create(context, (BlockCallTarget3)BlockCallback3, clrMethod, 3);
        }

        public static Proc/*!*/ Create(RubyContext/*!*/ context, Delegate/*!*/ clrMethod, object self, int parameterCount) {
            // scope is used to get to the execution context:
            return new Proc(ProcKind.Block, self, context.EmptyScope, BlockDispatcher.Create(clrMethod, parameterCount, BlockSignatureAttributes.None));
        }

        // The following methods are block implementations, therefore they need to have signature like a block.
        // We need to get to the real delegate to call. We capture it into the self object.
        public static object BlockCallback1(BlockParam/*!*/ block, object self, object arg0) {
            var clrMethod = (Func<BlockParam, object, object>)self;
            return clrMethod(block, arg0);
        }

        public static object BlockCallback2(BlockParam/*!*/ block, object self, object arg0, object arg1) {
            var clrMethod = (Func<BlockParam, object, object, object>)self;
            return clrMethod(block, arg0, arg1);
        }

        public static object BlockCallback3(BlockParam/*!*/ block, object self, object arg0, object arg1, object arg2) {
            var clrMethod = (Func<BlockParam, object, object, object, object>)self;
            return clrMethod(block, arg0, arg1, arg2);
        }

        #endregion
    }
}
