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
using System.Dynamic;
using System.Linq.Expressions;
using IronRuby.Compiler;
using IronRuby.Compiler.Generation;
using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Runtime.Calls {
    public enum RubyCallFlags {
        None = 0,
        HasScope = 1,
        HasSplattedArgument = 2,

        // an additional argument following splat arguments (e.g. target[args, *splat]=rhs)
        HasRhsArgument = 4,
        HasBlock = 8,

        // Used for private visibility check. By default method call sites have explicit self, so private methods are not visible.
        HasImplicitSelf = 16,

        // Interop calls can only see Ruby-public members.
        IsInteropCall = 32,

        // If the resolved method is a Ruby method call it otherwise invoke #base# method on target's type.
        // Used in method overrides defined in types emitted for Ruby classes that derive from CLR type.
        IsVirtualCall = 64,
    }
        
    /// <summary>
    /// RubyScope/RubyContext, (self), (argument){ArgumentCount}, (splatted-argument)?, (block)?
    /// </summary>
    public struct RubyCallSignature : IEquatable<RubyCallSignature> {
        private const int FlagsCount = 7;
        private const int MaxArgumentCount = (int)(UInt32.MaxValue >> FlagsCount);
        private const RubyCallFlags FlagsMask = (RubyCallFlags)(1 << FlagsCount) - 1;

        private readonly uint _countAndFlags;

        public bool HasImplicitSelf { get { return ((RubyCallFlags)_countAndFlags & RubyCallFlags.HasImplicitSelf) != 0; } }
        public bool HasScope { get { return ((RubyCallFlags)_countAndFlags & RubyCallFlags.HasScope) != 0; } }
        public bool HasBlock { get { return ((RubyCallFlags)_countAndFlags & RubyCallFlags.HasBlock) != 0; } }
        public bool HasSplattedArgument { get { return ((RubyCallFlags)_countAndFlags & RubyCallFlags.HasSplattedArgument) != 0; } }
        public bool HasRhsArgument { get { return ((RubyCallFlags)_countAndFlags & RubyCallFlags.HasRhsArgument) != 0; } }
        public bool IsInteropCall { get { return ((RubyCallFlags)_countAndFlags & RubyCallFlags.IsInteropCall) != 0; } }
        public bool IsVirtualCall { get { return ((RubyCallFlags)_countAndFlags & RubyCallFlags.IsVirtualCall) != 0; } }

        public int ArgumentCount { get { return (int)_countAndFlags >> FlagsCount; } }
        internal RubyCallFlags Flags { get { return (RubyCallFlags)_countAndFlags & FlagsMask; } }
        
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

        public RubyCallSignature(int argumentCount, RubyCallFlags flags) {
            Debug.Assert(argumentCount >= 0 && argumentCount < MaxArgumentCount);
            Debug.Assert(((int)flags >> FlagsCount) == 0);

            _countAndFlags = ((uint)argumentCount << FlagsCount) | (uint)flags;
        }

        public RubyCallSignature(bool hasScope, bool hasImplicitSelf, int argumentCount, bool hasSplattedArgument, bool hasBlock, bool hasRhsArgument) {
            Debug.Assert(argumentCount >= 0 && argumentCount < MaxArgumentCount);

            var flags = RubyCallFlags.None;
            if (hasImplicitSelf) flags |= RubyCallFlags.HasImplicitSelf;
            if (hasScope) flags |= RubyCallFlags.HasScope;
            if (hasSplattedArgument) flags |= RubyCallFlags.HasSplattedArgument;
            if (hasBlock) flags |= RubyCallFlags.HasBlock;
            if (hasRhsArgument) flags |= RubyCallFlags.HasRhsArgument;

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

        internal Expression/*!*/ CreateExpression() {
            return Ast.New(Methods.RubyCallSignatureCtor, AstUtils.Constant(_countAndFlags));
        }

        public bool Equals(RubyCallSignature other) {
            return _countAndFlags == other._countAndFlags;
        }

        public override string/*!*/ ToString() {
            return "(" +
                (HasImplicitSelf ? "." : "") +
                (IsVirtualCall ? "V" : "") +
                (HasScope ? "S," : "C,") +
                ArgumentCount.ToString() + 
                (HasSplattedArgument ? "*" : "") + 
                (HasBlock ? "&" : "") + 
                (HasRhsArgument ? "=" : "") + 
            ")";
        }
    }
}
