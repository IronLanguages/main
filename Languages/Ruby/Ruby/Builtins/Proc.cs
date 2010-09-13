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
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Compiler;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;

namespace IronRuby.Builtins {
    using Ast = Expression;
    using AstFactory = IronRuby.Compiler.Ast.AstFactory;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    using BlockCallTarget0 = Func<BlockParam, object, object>;
    using BlockCallTarget1 = Func<BlockParam, object, object, object>;
    using BlockCallTarget2 = Func<BlockParam, object, object, object, object>;
    using BlockCallTarget3 = Func<BlockParam, object, object, object, object, object>;
    using BlockCallTarget4 = Func<BlockParam, object, object, object, object, object, object>;
    using BlockCallTargetN = Func<BlockParam, object, object[], object>;
    using BlockCallTargetUnsplatN = Func<BlockParam, object, object[], RubyArray, object>;

    public enum ProcKind {
        Block,
        Proc,
        Lambda
    }

    public partial class Proc : IDuplicable {
        // Self object captured by the block definition, if any.
        // Although we could load self from scope in Ruby defined blocks, we cannot do so when we don't have a scope.
        private readonly object _self;

        // The scope that defines this block.
        private readonly RubyScope/*!*/ _scope;

        private readonly BlockDispatcher/*!*/ _dispatcher;

        // TODO: can we remove _kind and use _method and Converter fields intead?
        private ProcKind _kind;
        private RubyLambdaMethodInfo _method;

        // The frame that converted this block to a proc:
        internal RuntimeFlowControl Converter { get; set; }

        public ProcKind Kind {
            get { return _kind; }
            // friend: RuntimeFlowControl
            internal set { _kind = value; }
        }

        internal RubyLambdaMethodInfo Method {
            get { return _method; }
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

        public string SourcePath {
            get { return _dispatcher.SourcePath; }
        }

        public int SourceLine {
            get { return _dispatcher.SourceLine; }
        }

        #region Construction, Conversion

        internal Proc(ProcKind kind, object self, RubyScope/*!*/ scope, BlockDispatcher/*!*/ dispatcher) {
            Assert.NotNull(scope, dispatcher);
            Debug.Assert(dispatcher.Method != null);
            _kind = kind;
            _self = self;
            _scope = scope;
            _dispatcher = dispatcher;
        }

        protected Proc(Proc/*!*/ proc)
            : this(proc.Kind, proc.Self, proc.LocalScope, proc.Dispatcher) {
            Converter = proc.Converter;
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
        public Proc/*!*/ ToLambda(RubyLambdaMethodInfo method) {
            Proc result = new Proc(this);
            result.Kind = ProcKind.Lambda;
            result._method = method;
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

        // Proc doesn't have "initialize_copy", it's entirely initialized in dup:
        object IDuplicable.Duplicate(RubyContext/*!*/ context, bool copySingletonMembers) {
            var result = Copy();
            context.CopyInstanceData(this, result, copySingletonMembers);
            return result;
        }

        public static RubyLambdaMethodInfo/*!*/ ToLambdaMethodInfo(Proc/*!*/ block, string/*!*/ definitionName, RubyMethodVisibility visibility,
            RubyModule/*!*/ owner) {
            return new RubyLambdaMethodInfo(block, definitionName, (RubyMemberFlags)visibility, owner);
        }

        #endregion

        #region Dynamic Operations: Invoke

        internal void BuildInvoke(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            Assert.NotNull(metaBuilder, args);

            var convertedTarget = AstUtils.Convert(args.TargetExpression, typeof(Proc));

            // test for target type:
            metaBuilder.AddTypeRestriction(args.Target.GetType(), args.TargetExpression);

            BuildCall(
                metaBuilder,
                convertedTarget,                       // proc object  
                Methods.GetProcSelf.OpCall(convertedTarget),  // self captured by the block closure
                args
            );
        }

        /// <summary>
        /// From control flow perspective it "calls" the proc.
        /// </summary>
        internal static void BuildCall(
            MetaObjectBuilder/*!*/ metaBuilder,
            Expression/*!*/ procExpression,     // proc object
            Expression/*!*/ selfExpression,     // self passed to the proc
            CallArguments/*!*/ args             // user arguments passed to the proc
        ) {
            var bfcVariable = metaBuilder.GetTemporary(typeof(BlockParam), "#bfc");

            metaBuilder.Result = Ast.Block(
                Ast.Assign(bfcVariable, Methods.CreateBfcForProcCall.OpCall(AstUtils.Convert(procExpression, typeof(Proc)))),
                Methods.MethodProcCall.OpCall(bfcVariable, 
                    AstFactory.YieldExpression(
                        args.RubyContext,
                        args.GetSimpleArgumentExpressions(),
                        args.GetSplattedArgumentExpression(),
                        args.GetRhsArgumentExpression(),
                        args.GetBlockExpression(),
                        bfcVariable,
                        selfExpression
                    )
                )
            );
        }

        #endregion

        #region Call // TODO: generate

        // Call overloads don't check parameter count, this is done by Proc#call.

        public object Call(Proc procArg) {
            var blockParam = RubyOps.CreateBfcForProcCall(this);
            return RubyOps.MethodProcCall(blockParam, RubyOps.Yield0(procArg, _self, blockParam));
        }

        public object Call(Proc procArg, object arg1) {
            var blockParam = RubyOps.CreateBfcForProcCall(this);

            // lambda calls are weird:
            var result = (_kind == ProcKind.Lambda) ?
                RubyOps.YieldNoAutoSplat1(arg1, procArg, _self, blockParam) :
                RubyOps.Yield1(arg1, procArg, _self, blockParam);

            return RubyOps.MethodProcCall(blockParam, result);
        }

        public object Call(Proc procArg, object arg1, object arg2) {
            var blockParam = RubyOps.CreateBfcForProcCall(this);
            return RubyOps.MethodProcCall(blockParam, RubyOps.Yield2(arg1, arg2, procArg, _self, blockParam));
        }

        public object Call(Proc procArg, object arg1, object arg2, object arg3) {
            var blockParam = RubyOps.CreateBfcForProcCall(this);
            return RubyOps.MethodProcCall(blockParam, RubyOps.Yield3(arg1, arg2, arg3, procArg, _self, blockParam));
        }

        public object Call(Proc procArg, object arg1, object arg2, object arg3, object arg4) {
            var blockParam = RubyOps.CreateBfcForProcCall(this);
            return RubyOps.MethodProcCall(blockParam, RubyOps.Yield4(arg1, arg2, arg3, arg4, procArg, _self, blockParam));
        }

        public object Call(Proc procArg, params object[]/*!*/ args) {
            switch (args.Length) {
                case 0: return Call(procArg);
                case 1: return Call(procArg, args[0]);
                case 2: return Call(procArg, args[0], args[1]);
                case 3: return Call(procArg, args[0], args[1], args[2]);
                case 4: return Call(procArg, args[0], args[1], args[2], args[3]);
            }

            var blockParam = RubyOps.CreateBfcForProcCall(this);
            return RubyOps.MethodProcCall(blockParam, RubyOps.YieldN(args, procArg, _self, blockParam));
        }

        public object CallN(Proc procArg, object[]/*!*/ args) {
            Debug.Assert(args.Length > 4);
            var blockParam = RubyOps.CreateBfcForProcCall(this);
            return RubyOps.MethodProcCall(blockParam, RubyOps.YieldN(args, procArg, _self, blockParam));
        }

        #endregion

        #region Block helper methods

        public static Proc/*!*/ Create(RubyContext/*!*/ context, BlockCallTarget1/*!*/ clrMethod) {
            return Create(context, 1, BlockDispatcher.MakeAttributes(BlockSignatureAttributes.None, -1), clrMethod);
        }

        public static Proc/*!*/ CreateSimple(RubyContext/*!*/ context, BlockCallTarget1/*!*/ clrMethod) {
            return Create(context, 1, BlockSignatureAttributes.None, clrMethod);
        }

        public static Proc/*!*/ Create(RubyContext/*!*/ context, BlockCallTarget2/*!*/ clrMethod) {
            return Create(context, 2, BlockSignatureAttributes.None, clrMethod);
        }

        public static Proc/*!*/ Create(RubyContext/*!*/ context, BlockCallTarget3/*!*/ clrMethod) {
            return Create(context, 3, BlockSignatureAttributes.None, clrMethod);
        }

        public static Proc/*!*/ Create(RubyContext/*!*/ context, int parameterCount, BlockCallTargetUnsplatN/*!*/ clrMethod) {
            return Create(context, parameterCount, BlockSignatureAttributes.HasUnsplatParameter, clrMethod);
        }

        public static Proc/*!*/ Create(RubyContext/*!*/ context, int parameterCount, BlockSignatureAttributes signatureAttributes, Delegate/*!*/ clrMethod) {
            // scope is used to get to the execution context:
            return new Proc(ProcKind.Block, null, context.EmptyScope,
                BlockDispatcher.Create(parameterCount, signatureAttributes, null, 0).SetMethod(clrMethod)
            );
        }

        /// <summary>
        /// Creates a proc that invokes a method of given name.
        /// <code>
        /// Proc.new do |*args| 
        ///   raise ArgumentException if args.size == 0
        ///   obj.methodName(args.delete_at(0), args) 
        /// end
        /// </code>
        /// </summary>
        public static Proc/*!*/ CreateMethodInvoker(RubyScope/*!*/ scope, string/*!*/ methodName) {
            ContractUtils.RequiresNotNull(scope, "scope");

            // TODO: 
            // This should pass a proc parameter (use BlockDispatcherUnsplatProcN).
            // MRI 1.9.2 doesn't do so though (see http://redmine.ruby-lang.org/issues/show/3792).

            var site = CallSite<Func<CallSite, object, object, object, object>>.Create(
                RubyCallAction.Make(
                    scope.RubyContext, methodName,
                    new RubyCallSignature(0, RubyCallFlags.HasScope | RubyCallFlags.HasSplattedArgument)
                )
            );

            var block = new BlockCallTargetUnsplatN((blockParam, self, args, unsplat) => {
                Debug.Assert(args.Length == 0);
                if (unsplat.Count == 0) {
                    throw RubyExceptions.CreateArgumentError("no receiver given");
                }
                object target = unsplat[0];
                unsplat.RemoveAt(0);
                return site.Target(site, scope, target, unsplat);
            });

            var procDispatcher = new BlockDispatcherUnsplatN(0,
                BlockDispatcher.MakeAttributes(BlockSignatureAttributes.HasUnsplatParameter, -1),
                null, 0
            );

            procDispatcher.SetMethod(block);
            return new Proc(ProcKind.Proc, scope.SelfObject, scope, procDispatcher);
        }

        #endregion

        public override string/*!*/ ToString() {
            return String.Format("{0}: {1}:{2}", _kind, SourcePath ?? "(unknown)", SourceLine);
        }
    }
}
