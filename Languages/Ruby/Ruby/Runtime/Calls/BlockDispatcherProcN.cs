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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Utils;
using System.Diagnostics;
using IronRuby.Builtins;

namespace IronRuby.Runtime.Calls {
    using BlockCallTargetProcN = Func<BlockParam, object, object[], Proc, object>;

    // L(m, n, &)
    internal sealed class BlockDispatcherProcN : BlockDispatcherN<BlockCallTargetProcN> {
        internal BlockDispatcherProcN(int parameterCount, BlockSignatureAttributes attributesAndArity, string sourcePath, int sourceLine)
            : base(parameterCount, attributesAndArity, sourcePath, sourceLine) {
            Debug.Assert(!HasUnsplatParameter);
            Debug.Assert(HasProcParameter);
        }

        // R(0, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg) {
            return _block(param, self, new object[_parameterCount], procArg);
        }

        // R(1, -)
        public override object InvokeNoAutoSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1) {
            return _block(param, self, MakeArray(arg1), procArg);
        }

        // R(1, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1) {
            if (_parameterCount == 1) {
                return _block(param, self, MakeArray(arg1), procArg);
            } else {
                IList list = arg1 as IList ?? Protocols.ImplicitTrySplat(param.RubyContext, arg1) ?? new object[] { arg1 };
                return _block(param, self, CopyArgumentsFromSplattee(new object[_parameterCount], 0, list), procArg);
            }
        }

        // R(2, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2) {
            return _block(param, self, MakeArray(arg1, arg2), procArg);
        }

        // R(3, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3) {
            return _block(param, self, MakeArray(arg1, arg2, arg3), procArg);
        }

        // R(4, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4) {
            return _block(param, self, MakeArray(arg1, arg2, arg3, arg4), procArg);
        }

        // R(N, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object[]/*!*/ args) {
            Debug.Assert(args.Length > 4);

            // we need at least _parameterCount items in the parameter array:
            if (args.Length < _parameterCount) {
                Array.Resize(ref args, _parameterCount);
            }

            return _block(param, self, args, procArg);
        }

        // R(0, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, IList/*!*/ splattee) {
            if (splattee.Count == 1) {
                return Invoke(param, self, procArg, splattee[0]);
            } else {
                return _block(param, self, CopyArgumentsFromSplattee(new object[_parameterCount], 0, splattee), procArg);
            }
        }

        // R(1, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, IList/*!*/ splattee) {
            if (splattee.Count == 0) {
                return Invoke(param, self, procArg, arg1);
            } else {
                return _block(param, self, CopyArgumentsFromSplattee(MakeArray(arg1), 1, splattee), procArg);
            }
        }

        // R(2, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, IList/*!*/ splattee) {
            return _block(param, self, CopyArgumentsFromSplattee(MakeArray(arg1, arg2), 2, splattee), procArg);
        }

        // R(3, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3, IList/*!*/ splattee) {
            return _block(param, self, CopyArgumentsFromSplattee(MakeArray(arg1, arg2, arg3), 3, splattee), procArg);
        }

        // R(4, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4, IList/*!*/ splattee) {
            return _block(param, self, CopyArgumentsFromSplattee(MakeArray(arg1, arg2, arg3, arg4), 4, splattee), procArg);
        }

        // R(N, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object[]/*!*/ args, IList/*!*/ splattee) {
            Debug.Assert(args.Length > MaxBlockArity);
            int i, j;
            CreateArgumentsFromSplattee(_parameterCount, out i, out j, ref args, splattee);
            return _block(param, self, args, procArg);
        }

        // R(N, *, =)
        public override object InvokeSplatRhs(BlockParam/*!*/ param, object self, Proc procArg, object[]/*!*/ args, IList/*!*/ splattee, object rhs) {
            return _block(param, self, CreateArgumentsFromSplatteeAndRhs(_parameterCount, args, splattee, rhs), procArg);
        }
    }
}
