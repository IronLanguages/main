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
using IronRuby.Compiler;
using IronRuby.Compiler.Generation;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Runtime.Calls {
    using Ast = Expression;

    public enum RubyCallFlags {
        None = 0,
        HasScope = 1,
        HasSplattedArgument = 2,

        // an additional argument following splat arguments (e.g. target[args, *splat]=rhs)
        HasRhsArgument = 4,

        /// <summary>
        /// The site must propagate return from the block as implemented in <see cref="RubyOps.MethodPropagateReturn"/>.
        /// This means that if the method that uses the site itself takes a block no action is needed, just pass the return value thru.
        /// If it doesn't take a block but is yielding to a user block (a rare case) it needs to throw <see cref="MethodUnwinder"/> exception
        /// if the yielded block returns.
        /// </summary>
        HasBlock = 8,

        // Used for private visibility check. By default method call sites have explicit self, so private methods are not visible.
        HasImplicitSelf = 16,

        // Interop calls can only see Ruby-public members.
        IsInteropCall = 32,

        // CallAction only: If the resolved method is a Ruby method call it otherwise invoke #base# method on target's type.
        // Used in method overrides defined in types emitted for Ruby classes that derive from CLR type.
        IsVirtualCall = 64,

        // CallAction only: Skip the immediate class of the target in method resolution. Lexically unbound super calls.
        IsSuperCall = 128,

        // SuperCallAction only: the super site dynamically loads its parameters from scope.
        HasImplicitArguments = 128,
    }
        
    /// <summary>
    /// RubyScope/RubyContext, (self), (argument){ArgumentCount}, (splatted-argument)?, (block)?
    /// </summary>
    public struct RubyCallSignature : IEquatable<RubyCallSignature> {
        private const int FlagsCount = 8;

        private const int ResolveOnlyArgumentCount = (int)(UInt32.MaxValue >> FlagsCount);
        private const int MaxArgumentCount = ResolveOnlyArgumentCount - 1;
        private const uint FlagsMask = (1 << FlagsCount) - 1;

        private readonly uint _countAndFlags;

        public bool HasImplicitSelf { get { return (_countAndFlags & (uint)RubyCallFlags.HasImplicitSelf) != 0; } }
        public bool HasScope { get { return (_countAndFlags & (uint)RubyCallFlags.HasScope) != 0; } }
        public bool HasBlock { get { return (_countAndFlags & (uint)RubyCallFlags.HasBlock) != 0; } }
        public bool HasSplattedArgument { get { return (_countAndFlags & (uint)RubyCallFlags.HasSplattedArgument) != 0; } }
        public bool HasRhsArgument { get { return (_countAndFlags & (uint)RubyCallFlags.HasRhsArgument) != 0; } }
        public bool IsInteropCall { get { return (_countAndFlags & (uint)RubyCallFlags.IsInteropCall) != 0; } }
        public bool IsVirtualCall { get { return (_countAndFlags & (uint)RubyCallFlags.IsVirtualCall) != 0; } }
        public bool HasImplicitArguments { get { return (_countAndFlags & (uint)RubyCallFlags.HasImplicitArguments) != 0; } }
        public bool IsSuperCall { get { return (_countAndFlags & (uint)RubyCallFlags.IsSuperCall) != 0; } }

        // defined? ignores arguments hence we can use one argument number (max) to represent resolve only sites:
        public bool ResolveOnly { get { return ArgumentCount == ResolveOnlyArgumentCount; } }

        public int ArgumentCount { get { return (int)(_countAndFlags >> FlagsCount); } }
        internal RubyCallFlags Flags { get { return (RubyCallFlags)(_countAndFlags & FlagsMask); } }
        
        // total call-site arguments w/o RubyContext/RubyScope
        public int TotalArgumentCount {
            get {
                return 1 + // instance (self)
                    ArgumentCount +
                    (HasSplattedArgument ? 1 : 0) +
                    (HasBlock ? 1 : 0) +
                    (HasRhsArgument ? 1 : 0);
            }
        }

        /// <summary>
        /// Used by defined? operator applied on methods.
        /// </summary>
        private RubyCallSignature(RubyCallFlags flags) {
            Debug.Assert(((int)flags >> FlagsCount) == 0);
            _countAndFlags = ((uint)ResolveOnlyArgumentCount << FlagsCount) | (uint)flags;
        }

        public RubyCallSignature(int argumentCount, RubyCallFlags flags) {
            Debug.Assert(argumentCount >= 0 && argumentCount <= MaxArgumentCount);
            Debug.Assert(((int)flags >> FlagsCount) == 0);

            _countAndFlags = ((uint)argumentCount << FlagsCount) | (uint)flags;
        }

        [Emitted, Obsolete("Do not use from code"), CLSCompliant(false)]
        public RubyCallSignature(uint countAndFlags) {
            _countAndFlags = countAndFlags;
        }

        internal static bool TryCreate(CallInfo callInfo, out RubyCallSignature callSignature) {
            callSignature = RubyCallSignature.Interop(callInfo.ArgumentCount);
            return callInfo.ArgumentNames.Count != 0;
        }

        public static RubyCallSignature WithImplicitSelf(int argumentCount) {
            return new RubyCallSignature(argumentCount, RubyCallFlags.HasImplicitSelf);
        }
        
        public static RubyCallSignature Simple(int argumentCount) {
            return new RubyCallSignature(argumentCount, RubyCallFlags.None);
        }

        public static RubyCallSignature Interop(int argumentCount) {
            return new RubyCallSignature(argumentCount, RubyCallFlags.IsInteropCall);
        }

        public static RubyCallSignature WithBlock(int argumentCount) {
            return new RubyCallSignature(argumentCount, RubyCallFlags.HasBlock);
        }

        public static RubyCallSignature WithSplat(int argumentCount) {
            return new RubyCallSignature(argumentCount, RubyCallFlags.HasSplattedArgument);
        }

        public static RubyCallSignature WithSplatAndBlock(int argumentCount) {
            return new RubyCallSignature(argumentCount, RubyCallFlags.HasBlock | RubyCallFlags.HasSplattedArgument);
        }

        public static RubyCallSignature WithScope(int argumentCount) {
            return new RubyCallSignature(argumentCount, RubyCallFlags.HasScope);
        }

        public static RubyCallSignature WithScopeAndBlock(int argumentCount) {
            return new RubyCallSignature(argumentCount, RubyCallFlags.HasScope | RubyCallFlags.HasBlock);
        }

        public static RubyCallSignature WithScopeAndSplat(int argumentCount) {
            return new RubyCallSignature(argumentCount, RubyCallFlags.HasScope | RubyCallFlags.HasSplattedArgument);
        }

        public static RubyCallSignature WithScopeAndSplatAndBlock(int argumentCount) {
            return new RubyCallSignature(argumentCount, RubyCallFlags.HasScope | RubyCallFlags.HasBlock | RubyCallFlags.HasSplattedArgument);
        }

        public static RubyCallSignature IsDefined(bool hasImplicitSelf) {
            return new RubyCallSignature(RubyCallFlags.HasScope | (hasImplicitSelf ? RubyCallFlags.HasImplicitSelf : RubyCallFlags.None));
        }

        internal Expression/*!*/ CreateExpression() {
            return Ast.New(Methods.RubyCallSignatureCtor, AstUtils.Constant(_countAndFlags));
        }

        public bool Equals(RubyCallSignature other) {
            return _countAndFlags == other._countAndFlags;
        }

        public override string/*!*/ ToString() {
            return "(" +
                (HasImplicitSelf ? "." : "") +
                (HasImplicitArguments ? "^" : "") +
                (IsVirtualCall ? "V" : "") +
                (HasScope ? "S" : "C") +
                (ResolveOnly ? "?" : "," + ArgumentCount.ToString()) + 
                (HasSplattedArgument ? "*" : "") + 
                (HasBlock ? "&" : "") + 
                (HasRhsArgument ? "=" : "") +
            ")";
        }
    }
}
