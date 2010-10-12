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
    using BlockCallTargetN = Func<BlockParam, object, object[], object>;

    // L(n > 4, -)
    internal sealed class BlockDispatcherN : BlockDispatcherN<BlockCallTargetN> {
        internal BlockDispatcherN(int parameterCount, BlockSignatureAttributes attributesAndArity, string sourcePath, int sourceLine)
            : base(parameterCount, attributesAndArity, sourcePath, sourceLine) {
            Debug.Assert(parameterCount > BlockDispatcher.MaxBlockArity);
            Debug.Assert(!HasUnsplatParameter);
        }

        // R(0, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg) {
            // TODO: warning except for L == 1 nested l-value
            return _block(param, self, new object[_parameterCount]);
        }

        // R(1, -)
        public override object InvokeNoAutoSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1) {
            return _block(param, self, MakeArray(arg1));
        }
        
        // R(1, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1) {
            IList list = arg1 as IList ?? Protocols.ImplicitTrySplat(param.RubyContext, arg1) ?? new object[] { arg1 };                
            return _block(param, self, CopyArgumentsFromSplattee(new object[_parameterCount], 0, list));
        }

        // R(2, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2) {
            return _block(param, self, MakeArray(arg1, arg2));
        }

        // R(3, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3) {
            return _block(param, self, MakeArray(arg1, arg2, arg3));
        }

        // R(4, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4) {
            return _block(param, self, MakeArray(arg1, arg2, arg3, arg4));
        }

        // R(N, -)
        public override object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object[]/*!*/ args) {
            Debug.Assert(args.Length > 4);

            // we need at least _parameterCount items in the parameter array:
            if (args.Length < _parameterCount) {
                Array.Resize(ref args, _parameterCount);
            }

            return _block(param, self, args);
        }

        // R(0, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, IList/*!*/ splattee) {
            if (splattee.Count == 1) {
                return Invoke(param, self, procArg, splattee[0]);
            } else {
                return _block(param, self, CopyArgumentsFromSplattee(new object[_parameterCount], 0, splattee));
            }
        }

        // R(1, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, IList/*!*/ splattee) {
            if (splattee.Count == 0) {
                return Invoke(param, self, procArg, arg1);
            } else {
                return _block(param, self, CopyArgumentsFromSplattee(MakeArray(arg1), 1, splattee));
            }
        }

        // R(2, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, IList/*!*/ splattee) {
            return _block(param, self, CopyArgumentsFromSplattee(MakeArray(arg1, arg2), 2, splattee));
        }

        // R(3, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3, IList/*!*/ splattee) {
            return _block(param, self, CopyArgumentsFromSplattee(MakeArray(arg1, arg2, arg3), 3, splattee));
        }

        // R(4, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4, IList/*!*/ splattee) {
            return _block(param, self, CopyArgumentsFromSplattee(MakeArray(arg1, arg2, arg3, arg4), 4, splattee));
        }

        // R(N, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object[]/*!*/ args, IList/*!*/ splattee) {
            Debug.Assert(args.Length > MaxBlockArity);
            int i, j;
            CreateArgumentsFromSplattee(_parameterCount, out i, out j, ref args, splattee);
            return _block(param, self, args);
        }

        // R(N, *, =)
        public override object InvokeSplatRhs(BlockParam/*!*/ param, object self, Proc procArg, object[]/*!*/ args, IList/*!*/ splattee, object rhs) {
            return _block(param, self, CreateArgumentsFromSplatteeAndRhs(_parameterCount, args, splattee, rhs));
        }
    }
}
