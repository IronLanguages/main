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

using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Runtime.Calls;
using AstFactory = IronRuby.Compiler.Ast.AstFactory;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using IronRuby.Compiler.Generation;
using System.Dynamic;

namespace IronRuby.Runtime {
    using Ast = MSA.Expression;
    using System.Collections;
    
    public enum BlockReturnReason {
        Undefined = 0,
        Retry = 1,
        Return = 2,
        Break = 3,
    }

    public sealed class BlockReturnResult {
        internal static BlockReturnResult Retry = new BlockReturnResult();

        internal readonly object ReturnValue;
        internal readonly RuntimeFlowControl TargetFrame; // non-null for return, null for retry

        private BlockReturnResult() {
        }

        internal BlockReturnResult(RuntimeFlowControl/*!*/ targetFrame, object returnValue) {
            Assert.NotNull(targetFrame);
            TargetFrame = targetFrame;
            ReturnValue = returnValue;
        }

        public MethodUnwinder/*!*/ ToUnwinder() {
            return new MethodUnwinder(TargetFrame, ReturnValue);
        }
    }

    public enum BlockCallerKind {
        Yield,
        Call
    }

    internal sealed class MissingBlockParam {
        /// <remarks>
        /// LimitType must be MissingBlockParam (overload resolution, <see cref="RubyParameterBinder.PrepareParametersBinding"/>).
        /// Restriction should be empty: used only for !HasBlock call-sites => the site will never be reused for a call with a block.
        /// </remarks>
        internal sealed class Meta : DynamicMetaObject, IRestrictedMetaObject {
            internal static readonly DynamicMetaObject Instance = new Meta();

            private Meta()
                : base(AstUtils.Constant(null, typeof(MissingBlockParam)), BindingRestrictions.Empty) {
                Debug.Assert(LimitType == typeof(MissingBlockParam));
            }

            public DynamicMetaObject Restrict(Type/*!*/ type) {
                Debug.Assert(type == typeof(BlockParam) || type == typeof(MissingBlockParam));
                return this;
            }
        }

    }

    public sealed partial class BlockParam {
        // -- in --
        private readonly Proc/*!*/ _proc;
        private readonly BlockCallerKind _callerKind;

        // filled by module_eval: if not null than method definition uses the module
        private RubyModule _methodLookupModule;
        
        // Is the library method call taking this BlockParam a proc converter?
        // Used only for BlockParams that are passed to library method calls.
        // Friend: RubyOps
        internal readonly bool _isLibProcConverter;
        
        // -- out --
        private BlockReturnReason _returnReason;
        private RuntimeFlowControl _targetFrame;
        private ProcKind _sourceProcKind;

        internal BlockCallerKind CallerKind { get { return _callerKind; } }
        internal ProcKind SourceProcKind { get { return _sourceProcKind; } }
        internal BlockReturnReason ReturnReason { get { return _returnReason; } set { _returnReason = value; } }
        internal RuntimeFlowControl TargetFrame { get { return _targetFrame; } }
        internal RubyModule MethodLookupModule { get { return _methodLookupModule; } set { _methodLookupModule = value; } }
        internal bool IsLibProcConverter { get { return _isLibProcConverter; } }
        
        public Proc/*!*/ Proc { get { return _proc; } }

        [Emitted]
        public object Self { get { return _proc.Self; } }

        public RubyContext RubyContext {
            get { return _proc.LocalScope.RubyContext; }
        }

        public bool IsMethod {
            get { return _proc.Method != null; }
        }

        internal static PropertyInfo/*!*/ SelfProperty { get { return typeof(BlockParam).GetProperty("Self"); } }

        // friend: RubyOps
        internal BlockParam(Proc/*!*/ proc, BlockCallerKind callerKind, bool isLibProcConverter) {
            _callerKind = callerKind;
            _proc = proc;
            _isLibProcConverter = isLibProcConverter;
        }

        internal void SetFlowControl(BlockReturnReason reason, RuntimeFlowControl targetFrame, ProcKind sourceProcKind) {
            Debug.Assert((reason == BlockReturnReason.Break) == (targetFrame != null));

            _returnReason = reason;
            _targetFrame = targetFrame;
            _sourceProcKind = sourceProcKind;
        }

        internal object GetUnwinderResult(EvalUnwinder/*!*/ unwinder) {
            Debug.Assert(unwinder != null);
            SetFlowControl(unwinder.Reason, unwinder.TargetFrame, unwinder.SourceProcKind);
            return unwinder.ReturnValue;
        }

        #region Library Block Yield Helpers

        /// <summary>
        /// Must be called on the result of RubyOps.Yield. Implements post-yield control flow operation.
        /// </summary>
        /// <remarks>
        /// Used by library methods that take a block. The binder creates an instance of BlockParam holding on RFC if necessary.
        /// A library method that creates a block yet doesn't take one needs to manage RFC on its own.
        /// </remarks>
        internal bool BlockJumped(object returnValue) {
            // if this method is a proc converter then the current frame is Proc.Converter, otherwise it is not available:
            return RubyOps.MethodYieldRfc(_isLibProcConverter ? _proc.Converter : null, this, returnValue);
        }

        public bool Returning(object returnValue, out object result) {
            if (ReturnReason == BlockReturnReason.Return) {
                result = ((BlockReturnResult)returnValue).ReturnValue;
                return true;
            }
            result = null;
            return false;
        }

        /// <summary>
        /// Propagates control flow (break/return) from the yielded block to the enclosing block.
        /// </summary>
        public object PropagateFlow(BlockParam/*!*/ yieldedBlock, object returnValue) {
            if (yieldedBlock.ReturnReason == BlockReturnReason.Break) {
                return Break(returnValue);
            } else {
                _returnReason = yieldedBlock.ReturnReason;
                return returnValue;
            }
        }

        /// <summary>
        /// Breaks from the current block.
        /// </summary>
        public object Break(object returnValue) {
            Debug.Assert(_proc.Converter != null);

            // unwind to proc converter:
            SetFlowControl(BlockReturnReason.Break, _proc.Converter, _proc.Kind);
            return returnValue;
        }

        public bool Yield(out object blockResult) {
            return BlockJumped(blockResult = RubyOps.Yield0(null, Self, this));
        }

        public bool Yield(object arg1, out object blockResult) {
            return BlockJumped(blockResult = RubyOps.Yield1(arg1, null, Self, this));
        }

        public bool Yield(object arg1, object arg2, out object blockResult) {
            return BlockJumped(blockResult = RubyOps.Yield2(arg1, arg2, null, Self, this));
        }

        public bool Yield(object arg1, object arg2, object arg3, out object blockResult) {
            return BlockJumped(blockResult = RubyOps.Yield3(arg1, arg2, arg3, null, Self, this));
        }

        public bool Yield(object arg1, object arg2, object arg3, object arg4, out object blockResult) {
            return BlockJumped(blockResult = RubyOps.Yield4(arg1, arg2, arg3, arg4, null, Self, this));
        }

        public bool Yield(object[]/*!*/ args, out object blockResult) {
            return BlockJumped(blockResult = RubyOps.Yield(args, null, Self, this));
        }

        public bool YieldSplat(IList/*!*/ args, out object blockResult) {
            return BlockJumped(blockResult = RubyOps.YieldSplat0(args, null, Self, this));
        }

        #endregion

        #region Dynamic Operations

        /// <summary>
        /// "yields" to the proc.
        /// </summary>
        internal void BuildInvoke(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            Assert.NotNull(metaBuilder, args);
            Debug.Assert(!args.Signature.HasBlock);

            var convertedTarget = AstUtils.Convert(args.TargetExpression, typeof(BlockParam));

            // test for target type:
            metaBuilder.AddTypeRestriction(args.Target.GetType(), args.TargetExpression);

            metaBuilder.Result = AstFactory.YieldExpression(
                args.RubyContext,
                args.GetSimpleArgumentExpressions(),
                args.GetSplattedArgumentExpression(),
                args.GetRhsArgumentExpression(),
                args.GetBlockExpression(),                    
                convertedTarget,                              // block param
                Ast.Property(convertedTarget, SelfProperty)   // self
            );
        }

        #endregion

    }

    #region RubyOps

    public static partial class RubyOps {

        [Emitted]
        public static bool IsProcConverterTarget(BlockParam/*!*/ bfc, MethodUnwinder/*!*/ unwinder) {
            Debug.Assert(unwinder != null);
            return bfc.IsLibProcConverter && unwinder.TargetFrame == bfc.Proc.Converter;
        }
        
        [Emitted]
        public static BlockParam/*!*/ CreateBfcForYield(Proc proc) {
            if (proc != null) {
                return new BlockParam(proc, BlockCallerKind.Yield, false);
            } else {
                throw RubyExceptions.NoBlockGiven();
            }
        }

        [Emitted]
        public static BlockParam/*!*/ CreateBfcForProcCall(Proc/*!*/ proc) {
            Assert.NotNull(proc);
            return new BlockParam(proc, BlockCallerKind.Call, false);
        }
        
        [Emitted]
        public static BlockParam/*!*/ CreateBfcForLibraryMethod(Proc/*!*/ proc) {
            Assert.NotNull(proc);
            bool isProcConverter;

            if (proc.Kind == ProcKind.Block) {
                var rfc = new RuntimeFlowControl();
                rfc._activeFlowControlScope = rfc;
                proc.Converter = rfc;
                proc.Kind = ProcKind.Proc;
                isProcConverter = true;
            } else {
                isProcConverter = false;
            }

            return new BlockParam(proc, BlockCallerKind.Yield, isProcConverter);
        }

        [Emitted] 
        public static void LeaveProcConverter(BlockParam/*!*/ bfc) {
            Debug.Assert(bfc.Proc != null);
            if (bfc._isLibProcConverter) {
                Debug.Assert(bfc.Proc.Converter != null);
                bfc.Proc.Converter.LeaveMethod();
            }
        }
    }

    #endregion
}
