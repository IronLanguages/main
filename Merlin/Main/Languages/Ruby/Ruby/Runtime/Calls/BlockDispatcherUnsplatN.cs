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
using IronRuby.Builtins;
using System.Collections;

namespace IronRuby.Runtime.Calls {
    using BlockCallTargetUnsplatN = Func<BlockParam, object, object[], RubyArray, object>;

    // L(n, *)
    internal sealed class BlockDispatcherUnsplatN : BlockDispatcher {
        private readonly BlockCallTargetUnsplatN/*!*/ _block;
        private readonly int _parameterCount; // doesn't include the * parameter

        public override Delegate/*!*/ Method { get { return _block; } }
        public override int ParameterCount { get { return _parameterCount; } }

        internal BlockDispatcherUnsplatN(BlockCallTargetUnsplatN/*!*/ block, int parameterCount, BlockSignatureAttributes attributesAndArity) 
            : base(attributesAndArity) {
            Assert.NotNull(block);
            Debug.Assert(HasUnsplatParameter);

            _parameterCount = parameterCount;
            _block = block;
        }

        // R(0, -)
        public override object Invoke(BlockParam/*!*/ param, object self) {
            return InvokeInternal(param, self, ArrayUtils.EmptyObjects);
        }

        // R(1, -)
        public override object InvokeNoAutoSplat(BlockParam/*!*/ param, object self, object arg1) {
            return InvokeInternal(param, self, new object[] { arg1 }); // TODO: optimize
        }
        
        // R(1, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1) {
            if (_parameterCount > 0) {
                return InvokeSplatInternal(param, self, ArrayUtils.EmptyObjects, arg1); // TODO: optimize
            } else {
                return InvokeInternal(param, self, new object[] { arg1 }); // TODO: optimize
            }
        }

        // R(2, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1, object arg2) {
            return InvokeInternal(param, self, new object[] { arg1, arg2 });// TODO: optimize
        }

        // R(3, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3) {
            return InvokeInternal(param, self, new object[] { arg1, arg2, arg3 });// TODO: optimize
        }

        // R(4, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object arg4) {
            return InvokeInternal(param, self, new object[] { arg1, arg2, arg3, arg4 });// TODO: optimize
        }

        // R(N, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object[]/*!*/ args) {
            return InvokeInternal(param, self, args);
        }

        private object InvokeInternal(BlockParam/*!*/ param, object self, object[]/*!*/ args) {
            // TODO
            if (args.Length < _parameterCount) {
                Array.Resize(ref args, _parameterCount);
                return _block(param, self, args, RubyOps.MakeArray0());
            } else if (args.Length == _parameterCount) {
                return _block(param, self, args, RubyOps.MakeArray0());
            } else {
                var actualArgs = new object[_parameterCount];

                for (int i = 0; i < actualArgs.Length; i++) {
                    actualArgs[i] = args[i];
                }
                
                var array = new RubyArray(args.Length - _parameterCount); 
                for (int i = _parameterCount; i < args.Length; i++) {
                    array.Add(args[i]);
                }

                return _block(param, self, actualArgs, array);
            }
        }

        // R(0, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object splattee) {
            return InvokeSplatInternal(param, self, ArrayUtils.EmptyObjects, splattee);
        }

        // R(1, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object splattee) {
            return InvokeSplatInternal(param, self, new object[] { arg1 }, splattee);
        }

        // R(2, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object splattee) {
            return InvokeSplatInternal(param, self, new object[] { arg1, arg2 }, splattee);
        }

        // R(3, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object splattee) {
            return InvokeSplatInternal(param, self, new object[] { arg1, arg2, arg3 }, splattee);
        }

        // R(4, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object arg4, object splattee) {
            return InvokeSplatInternal(param, self, new object[] { arg1, arg2, arg3, arg4 }, splattee);
        }

        // R(N, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object[]/*!*/ args, object splattee) {
            return InvokeSplatInternal(param, self, args, splattee);
        }

        // R(N, *, =)
        public override object InvokeSplatRhs(BlockParam/*!*/ param, object self, object[]/*!*/ args, object splattee, object rhs) {
            var list = splattee as IList;
            if (list != null) {
                var l = new RubyArray(list.Count + 1);
                l.AddRange(list);
                l.Add(rhs);
                list = l;
            } else {
                list = RubyOps.MakeArray2(splattee, rhs);
            }

            return InvokeSplatInternal(param, self, args, list, list);
        }

        private object InvokeSplatInternal(BlockParam/*!*/ param, object self, object[]/*!*/ args, object splattee) {
            return InvokeSplatInternal(param, self, args, splattee, splattee as IList);
        }

        private object InvokeSplatInternal(BlockParam/*!*/ param, object self, object[]/*!*/ args, object splattee, IList list) {
            int argsLength = args.Length;

            int nextArg, nextItem;
            CreateArgumentsFromSplattee(_parameterCount, out nextArg, out nextItem, ref args, splattee);

            var array = new RubyArray();

            // remaining args:
            while (nextArg < argsLength) {
                array.Add(args[nextArg++]);
            }

            // remaining items:
            if (list != null) {
                while (nextItem < list.Count) {
                    array.Add(list[nextItem++]);
                }
            } else if (nextItem < 1) {
                // splattee hasn't been added yet:
                array.Add(splattee);
            }

            return _block(param, self, args, array);
        }
    }
}
