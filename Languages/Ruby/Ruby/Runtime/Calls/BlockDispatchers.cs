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

using System;
using Microsoft.Scripting.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using IronRuby.Builtins;
using System.Collections;

namespace IronRuby.Runtime.Calls {
    using BlockCallTarget0 = Func<BlockParam, object, object>;
    using BlockCallTarget1 = Func<BlockParam, object, object, object>;
    using BlockCallTarget2 = Func<BlockParam, object, object, object, object>;
    using BlockCallTarget3 = Func<BlockParam, object, object, object, object, object>;
    using BlockCallTarget4 = Func<BlockParam, object, object, object, object, object, object>;
    using BlockCallTargetN = Func<BlockParam, object, object[], object>;

    // L(0, -)
    internal sealed class BlockDispatcher0 : BlockDispatcher<BlockCallTarget0> {
        public override int ParameterCount { get { return 0; } }

        public BlockDispatcher0(BlockSignatureAttributes attributesAndArity, string sourcePath, int sourceLine)
            : base(attributesAndArity, sourcePath, sourceLine) {
            Debug.Assert(!HasUnsplatParameter);
        }

        // R(0, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg) {
            return _block(param, self);
        }

        // R(1, -)
        public override object InvokeNoAutoSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1) {
            return _block(param, self);
        }

        // R(1, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1) {
            return _block(param, self);
        }

        // R(2, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2) {
            return _block(param, self);
        }

        // R(3, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3) {
            return _block(param, self);
        }

        // R(4, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4) {
            return _block(param, self);
        }

        // R(N, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object[]/*!*/ args) {
            Debug.Assert(args.Length > MaxBlockArity);
            return _block(param, self);
        }

        // R(0, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, IList/*!*/ splattee) {
            return _block(param, self);
        }

        // R(1, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, IList/*!*/ splattee) {
            return _block(param, self);
        }

        // R(2, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, IList/*!*/ splattee) {
            return _block(param, self);
        }

        // R(3, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3, IList/*!*/ splattee) {
            return _block(param, self);
        }

        // R(4, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4, IList/*!*/ splattee) {
            return _block(param, self);
        }

        // R(N, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object[]/*!*/ args, IList/*!*/ splattee) {
            return _block(param, self);
        }

        // R(N, *, =)
        public override object InvokeSplatRhs(BlockParam/*!*/ param, object self, Proc procArg, object[]/*!*/ args, IList/*!*/ splattee, object rhs) {
            return _block(param, self);
        }
    }

    // L(1, -)
    internal sealed class BlockDispatcher1 : BlockDispatcher<BlockCallTarget1> {
        public override int ParameterCount { get { return 1; } }

        public BlockDispatcher1(BlockSignatureAttributes attributesAndArity, string sourcePath, int sourceLine)
            : base(attributesAndArity, sourcePath, sourceLine) {
            Debug.Assert(!HasUnsplatParameter);
        }

        // R(0, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg) {
            return _block(param, self, null);
        }

        // R(1, -)
        public override object InvokeNoAutoSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1) {
            return _block(param, self, arg1);
        }

        // R(1, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1) {
            return _block(param, self, arg1);
        }

        // R(2, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2) {
            return _block(param, self, arg1);
        }

        // R(3, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3) {
            return _block(param, self, arg1);
        }

        // R(4, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4) {
            return _block(param, self, arg1);
        }

        // R(N, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object[]/*!*/ args) {
            Debug.Assert(args.Length > MaxBlockArity);
            return _block(param, self, args[0]);
        }

        // R(0, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, IList/*!*/ splattee) {
            return _block(param, self, (splattee.Count > 0) ? splattee[0] : null);
        }
        
        // R(1, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, IList/*!*/ splattee) {
            return _block(param, self, arg1);
        }

        // R(2, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, IList/*!*/ splattee) {
            return _block(param, self, arg1);
        }

        // R(3, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3, IList/*!*/ splattee) {
            return _block(param, self, arg1);
        }

        // R(4, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4, IList/*!*/ splattee) {
            return _block(param, self, arg1);
        }
        
        // R(N, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object[]/*!*/ args, IList/*!*/ splattee) {
            Debug.Assert(args.Length > MaxBlockArity);
            return _block(param, self, args[0]);
        }

        // R(N, *, =)
        public override object InvokeSplatRhs(BlockParam/*!*/ param, object self, Proc procArg, object[]/*!*/ args, IList/*!*/ splattee, object rhs) {
            return _block(param, self, args.Length > 0 ? args[0] : splattee.Count > 0 ? splattee[0] : rhs);
        }
    }

    // L(2, -)
    internal sealed class BlockDispatcher2 : BlockDispatcher<BlockCallTarget2> {
        public override int ParameterCount { get { return 2; } }

        public BlockDispatcher2(BlockSignatureAttributes attributesAndArity, string sourcePath, int sourceLine)
            : base(attributesAndArity, sourcePath, sourceLine) {
            Debug.Assert(!HasUnsplatParameter);
        }

        // R(0, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg) {
            return _block(param, self, null, null);
        }

        // R(1, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1) {
            IList splattee = arg1 as IList ?? Protocols.ImplicitTrySplat(param.RubyContext, arg1);
            if (splattee != null) {
                switch (splattee.Count) {
                    case 0: return _block(param, self, null, null);
                    case 1: return _block(param, self, splattee[0], null);
                    default: return _block(param, self, splattee[0], splattee[1]);
                }
            } else {
                return _block(param, self, arg1, null);
            }
        }

        // R(1, -)
        public override object InvokeNoAutoSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1) {
            return _block(param, self, arg1, null);
        }

        // R(2, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2) {
            return _block(param, self, arg1, arg2);
        }

        // R(3, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3) {
            return _block(param, self, arg1, arg2);
        }

        // R(4, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4) {
            return _block(param, self, arg1, arg2);
        }
        
        // R(N, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object[]/*!*/ args) {
            Debug.Assert(args.Length > MaxBlockArity);
            return _block(param, self, args[0], args[1]);
        }

        // R(0, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, IList/*!*/ splattee) {
            switch (splattee.Count) {
                case 0: return Invoke(param, self, procArg);
                case 1: return Invoke(param, self, procArg, splattee[0]);
                default: return Invoke(param, self, procArg, splattee[0], splattee[1]);
            }
        }

        // R(1, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, IList/*!*/ splattee) {
            switch (splattee.Count) {
                case 0: return Invoke(param, self, procArg, arg1);
                default: return Invoke(param, self, procArg, arg1, splattee[0]);
            }
        }

        // R(2, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, IList/*!*/ splattee) {
            return _block(param, self, arg1, arg2);
        }

        // R(3, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3, IList/*!*/ splattee) {
            return _block(param, self, arg1, arg2);
        }

        // R(4, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4, IList/*!*/ splattee) {
            return _block(param, self, arg1, arg2);
        }

        // R(N, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object[]/*!*/ args, IList/*!*/ splattee) {
            Debug.Assert(args.Length > MaxBlockArity);
            return _block(param, self, args[0], args[1]);
        }

        // R(N, *, =)
        public override object InvokeSplatRhs(BlockParam/*!*/ param, object self, Proc procArg, object[]/*!*/ args, IList/*!*/ splattee, object rhs) {
            args = CreateArgumentsFromSplatteeAndRhs(2, args, splattee, rhs);
            return _block(param, self, args[0], args[1]);
        }
    }

    // L(3, -)
    internal sealed class BlockDispatcher3 : BlockDispatcher<BlockCallTarget3> {
        public override int ParameterCount { get { return 3; } }

        public BlockDispatcher3(BlockSignatureAttributes attributesAndArity, string sourcePath, int sourceLine)
            : base(attributesAndArity, sourcePath, sourceLine) {
            Debug.Assert(!HasUnsplatParameter);
        }

        // R(0, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg) {
            return _block(param, self, null, null, null);
        }

        // R(1, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1) {
            IList splattee = arg1 as IList ?? Protocols.ImplicitTrySplat(param.RubyContext, arg1);
            if (splattee != null) {
                switch (splattee.Count) {
                    case 0: return _block(param, self, null, null, null);
                    case 1: return _block(param, self, splattee[0], null, null);
                    case 2: return _block(param, self, splattee[0], splattee[1], null);
                    default: return _block(param, self, splattee[0], splattee[1], splattee[2]);
                }
            } else {
                return _block(param, self, arg1, null, null);
            }
        }

        // R(1, -)
        public override object InvokeNoAutoSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1) {
            return _block(param, self, arg1, null, null);
        }

        // R(2, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2) {
            return _block(param, self, arg1, arg2, null);
        }

        // R(3, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3) {
            return _block(param, self, arg1, arg2, arg3);
        }

        // R(4, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4) {
            return _block(param, self, arg1, arg2, arg3);
        }

        // R(N, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object[]/*!*/ args) {
            Debug.Assert(args.Length > MaxBlockArity);
            return _block(param, self, args[0], args[1], args[2]);
        }

        // R(0, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, IList/*!*/ splattee) {
            switch (splattee.Count) {
                case 0: return Invoke(param, self, procArg);
                case 1: return Invoke(param, self, procArg, splattee[0]);
                case 2: return Invoke(param, self, procArg, splattee[0], splattee[1]);
                default: return Invoke(param, self, procArg, splattee[0], splattee[1], splattee[2]);
            }
        }

        // R(1, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, IList/*!*/ splattee) {
            switch (splattee.Count) {
                case 0: return Invoke(param, self, procArg, arg1);
                case 1: return Invoke(param, self, procArg, arg1, splattee[0]);
                default: return Invoke(param, self, procArg, arg1, splattee[0], splattee[1]);
            }
        }

        // R(2, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, IList/*!*/ splattee) {
            switch (splattee.Count) {
                case 0: return Invoke(param, self, procArg, arg1, arg2);
                default: return Invoke(param, self, procArg, arg1, arg2, splattee[0]);
            }
        }

        // R(3, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3, IList/*!*/ splattee) {
            return _block(param, self, arg1, arg2, arg3);
        }

        // R(4, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4, IList/*!*/ splattee) {
            return _block(param, self, arg1, arg2, arg3);
        }

        // R(N, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object[]/*!*/ args, IList/*!*/ splattee) {
            Debug.Assert(args.Length > MaxBlockArity);
            return _block(param, self, args[0], args[1], args[2]);
        }

        // R(N, *, =)
        public override object InvokeSplatRhs(BlockParam/*!*/ param, object self, Proc procArg, object[]/*!*/ args, IList/*!*/ splattee, object rhs) {
            args = CreateArgumentsFromSplatteeAndRhs(3, args, splattee, rhs);
            return _block(param, self, args[0], args[1], args[2]);
        }
    }

    // L(4, -)
    internal sealed class BlockDispatcher4 : BlockDispatcher<BlockCallTarget4> {
        public override int ParameterCount { get { return 4; } }

        public BlockDispatcher4(BlockSignatureAttributes attributesAndArity, string sourcePath, int sourceLine)
            : base(attributesAndArity, sourcePath, sourceLine) {
            Debug.Assert(!HasUnsplatParameter);
        }

        // R(0, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg) {
            return _block(param, self, null, null, null, null);
        }
        
        // R(1, -)
        public override object InvokeNoAutoSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1) {
            return _block(param, self, arg1, null, null, null);
        }

        // R(1, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1) {
            IList splattee = arg1 as IList ?? Protocols.ImplicitTrySplat(param.RubyContext, arg1);
            if (splattee != null) {
                switch (splattee.Count) {
                    case 0: return _block(param, self, null, null, null, null);
                    case 1: return _block(param, self, splattee[0], null, null, null);
                    case 2: return _block(param, self, splattee[0], splattee[1], null, null);
                    case 3: return _block(param, self, splattee[0], splattee[1], splattee[2], null);
                    default: return _block(param, self, splattee[0], splattee[1], splattee[2], splattee[3]);
                }
            } else {
                return _block(param, self, arg1, null, null, null);
            }
        }

        // R(2, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2) {
            return _block(param, self, arg1, arg2, null, null);
        }

        // R(3, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3) {
            return _block(param, self, arg1, arg2, arg3, null);
        }

        // R(4, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4) {
            return _block(param, self, arg1, arg2, arg3, arg4);
        }

        // R(N, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object[]/*!*/ args) {
            Debug.Assert(args.Length > MaxBlockArity);
            return _block(param, self, args[0], args[1], args[2], args[3]);
        }

        // R(0, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, IList/*!*/ splattee) {
            switch (splattee.Count) {
                case 0: return Invoke(param, self, procArg);
                case 1: return Invoke(param, self, procArg, splattee[0]);
                case 2: return Invoke(param, self, procArg, splattee[0], splattee[1]);
                case 3: return Invoke(param, self, procArg, splattee[0], splattee[1], splattee[2]);
                default: return Invoke(param, self, procArg, splattee[0], splattee[1], splattee[2], splattee[3]);
            }
        }

        // R(1, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, IList/*!*/ splattee) {
            switch (splattee.Count) {
                case 0: return Invoke(param, self, procArg, arg1);
                case 1: return Invoke(param, self, procArg, arg1, splattee[0]);
                case 2: return Invoke(param, self, procArg, arg1, splattee[0], splattee[1]);
                default: return Invoke(param, self, procArg, arg1, splattee[0], splattee[1], splattee[2]);
            }
        }

        // R(2, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, IList/*!*/ splattee) {
            switch (splattee.Count) {
                case 0: return Invoke(param, self, procArg, arg1, arg2);
                case 1: return Invoke(param, self, procArg, arg1, arg2, splattee[0]);
                default: return Invoke(param, self, procArg, arg1, arg2, splattee[0], splattee[1]);
            }
        }

        // R(3, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3, IList/*!*/ splattee) {
            switch (splattee.Count) {
                case 0: return Invoke(param, self, procArg, arg1, arg2, arg3);
                default: return Invoke(param, self, procArg, arg1, arg2, arg3, splattee[0]);
            }
        }

        // R(4, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4, IList/*!*/ splattee) {
            return _block(param, self, arg1, arg2, arg3, arg4);
        }

        // R(N, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object[]/*!*/ args, IList/*!*/ splattee) {
            Debug.Assert(args.Length > MaxBlockArity);
            return _block(param, self, args[0], args[1], args[2], args[3]);
        }

        // R(N, *, =)
        public override object InvokeSplatRhs(BlockParam/*!*/ param, object self, Proc procArg, object[]/*!*/ args, IList/*!*/ splattee, object rhs) {
            args = CreateArgumentsFromSplatteeAndRhs(4, args, splattee, rhs);
            return _block(param, self, args[0], args[1], args[2], args[3]);
        }
    }
}
