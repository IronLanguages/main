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
using System.Text;
using Microsoft.Scripting.Utils;
using System.Diagnostics;

namespace IronRuby.Runtime.Calls {
    using BlockCallTargetN = Func<BlockParam, object, object[], object>;

    // L(n > 4, -)
    internal sealed class BlockDispatcherN : BlockDispatcher {
        private readonly BlockCallTargetN/*!*/ _block;
        private readonly int _parameterCount;

        public override Delegate/*!*/ Method { get { return _block; } }
        public override int ParameterCount { get { return _parameterCount; } }

        internal BlockDispatcherN(BlockCallTargetN/*!*/ block, int parameterCount, BlockSignatureAttributes attributesAndArity)
            : base(attributesAndArity) {
            Assert.NotNull(block);
            Debug.Assert(parameterCount > BlockDispatcher.MaxBlockArity);
            Debug.Assert(!HasUnsplatParameter);

            _parameterCount = parameterCount;
            _block = block;
        }

        private object[]/*!*/ MakeArray(object arg1) {
            var array = new object[_parameterCount];
            array[0] = arg1;
            return array;
        }

        private object[]/*!*/ MakeArray(object arg1, object arg2) {
            var array = new object[_parameterCount];
            array[0] = arg1;
            array[1] = arg2;
            return array;
        }

        private object[]/*!*/ MakeArray(object arg1, object arg2, object arg3) {
            var array = new object[_parameterCount];
            array[0] = arg1;
            array[1] = arg2;
            array[2] = arg3;
            return array;
        }

        private object[]/*!*/ MakeArray(object arg1, object arg2, object arg3, object arg4) {
            var array = new object[_parameterCount];
            array[0] = arg1;
            array[1] = arg2;
            array[2] = arg3;
            array[3] = arg4;
            return array;
        }

        // R(0, -)
        public override object Invoke(BlockParam/*!*/ param, object self) {
            // TODO: warning except for L == 1 nested l-value
            return _block(param, self, new object[_parameterCount]);
        }

        // R(1, -)
        public override object InvokeNoAutoSplat(BlockParam/*!*/ param, object self, object arg1) {
            return _block(param, self, MakeArray(arg1));
        }
        
        // R(1, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1) {
            return _block(param, self, CopyArgumentsFromSplattee(new object[_parameterCount], 0, arg1));
        }

        // R(2, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1, object arg2) {
            return _block(param, self, MakeArray(arg1, arg2));
        }

        // R(3, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3) {
            return _block(param, self, MakeArray(arg1, arg2, arg3));
        }

        // R(4, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object arg4) {
            return _block(param, self, MakeArray(arg1, arg2, arg3, arg4));
        }

        // R(N, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object[]/*!*/ args) {
            Debug.Assert(args.Length > 4);

            // we need at least _parameterCount items in the parameter array:
            if (args.Length < _parameterCount) {
                Array.Resize(ref args, _parameterCount);
            }

            return _block(param, self, args);
        }

        // R(0, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object splattee) {
            return _block(param, self, CopyArgumentsFromSplattee(new object[_parameterCount], 0, splattee));
        }

        // R(1, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object splattee) {
            return _block(param, self, CopyArgumentsFromSplattee(MakeArray(arg1), 1, splattee));
        }

        // R(2, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object splattee) {
            return _block(param, self, CopyArgumentsFromSplattee(MakeArray(arg1, arg2), 2, splattee));
        }

        // R(3, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object splattee) {
            return _block(param, self, CopyArgumentsFromSplattee(MakeArray(arg1, arg2, arg3), 3, splattee));
        }

        // R(4, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object arg4, object splattee) {
            return _block(param, self, CopyArgumentsFromSplattee(MakeArray(arg1, arg2, arg3, arg4), 4, splattee));
        }

        // R(N, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object[]/*!*/ args, object splattee) {
            Debug.Assert(args.Length > MaxBlockArity);
            int i, j;
            CreateArgumentsFromSplattee(_parameterCount, out i, out j, ref args, splattee);
            return _block(param, self, args);
        }

        // R(N, *, =)
        public override object InvokeSplatRhs(BlockParam/*!*/ param, object self, object[]/*!*/ args, object splattee, object rhs) {
            return _block(param, self, CreateArgumentsFromSplatteeAndRhs(_parameterCount, args, splattee, rhs));
        }
    }
}
