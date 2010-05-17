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
            Debug.Assert(!HasSingleCompoundParameter);
        }

        // R(0, -)
        public override object Invoke(BlockParam/*!*/ param, object self) {
            return _block(param, self);
        }

        // R(1, -)
        public override object InvokeNoAutoSplat(BlockParam/*!*/ param, object self, object arg1) {
            return _block(param, self);
        }

        // R(1, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1) {
            return _block(param, self);
        }

        // R(2, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1, object arg2) {
            return _block(param, self);
        }

        // R(3, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3) {
            return _block(param, self);
        }

        // R(4, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object arg4) {
            return _block(param, self);
        }

        // R(N, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object[]/*!*/ args) {
            Debug.Assert(args.Length > MaxBlockArity);
            return _block(param, self);
        }

        // R(0, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, IList/*!*/ splattee) {
            return _block(param, self);
        }

        // R(1, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, IList/*!*/ splattee) {
            return _block(param, self);
        }

        // R(2, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, IList/*!*/ splattee) {
            return _block(param, self);
        }

        // R(3, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, IList/*!*/ splattee) {
            return _block(param, self);
        }

        // R(4, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object arg4, IList/*!*/ splattee) {
            return _block(param, self);
        }

        // R(N, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object[]/*!*/ args, IList/*!*/ splattee) {
            return _block(param, self);
        }

        // R(N, *, =)
        public override object InvokeSplatRhs(BlockParam/*!*/ param, object self, object[]/*!*/ args, IList/*!*/ splattee, object rhs) {
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
        public override object Invoke(BlockParam/*!*/ param, object self) {
            if (!HasSingleCompoundParameter) {
                param.MultipleValuesForBlockParameterWarning(0);
            }
            
            return _block(param, self, null);
        }

        // R(1, -)
        public override object InvokeNoAutoSplat(BlockParam/*!*/ param, object self, object arg1) {
            return _block(param, self, arg1);
        }

        // R(1, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1) {
            return _block(param, self, arg1);
        }

        // R(2, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1, object arg2) {
            if (!HasSingleCompoundParameter) {
                param.MultipleValuesForBlockParameterWarning(2);
            }

            return _block(param, self, RubyOps.MakeArray2(arg1, arg2));
        }

        // R(3, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3) {
            if (!HasSingleCompoundParameter) {
                param.MultipleValuesForBlockParameterWarning(3);
            }

            return _block(param, self, RubyOps.MakeArray3(arg1, arg2, arg3));
        }

        // R(4, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object arg4) {
            if (!HasSingleCompoundParameter) {
                param.MultipleValuesForBlockParameterWarning(4);
            }

            return _block(param, self, RubyOps.MakeArray4(arg1, arg2, arg3, arg4));
        }

        // R(N, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object[]/*!*/ args) {
            Debug.Assert(args.Length > MaxBlockArity);

            if (!HasSingleCompoundParameter) {
                param.MultipleValuesForBlockParameterWarning(args.Length);
            }

            return _block(param, self, RubyOps.MakeArrayN(args));
        }

        // R(0, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, IList/*!*/ splattee) {
            object item;
            switch (splattee.Count) {
                case 0:
                    if (!HasSingleCompoundParameter) {
                        param.MultipleValuesForBlockParameterWarning(splattee.Count);
                    }
                    item = null; 
                    break;

                case 1:
                    item = splattee[0]; 
                    break;

                default:
                    if (!HasSingleCompoundParameter) {
                        param.MultipleValuesForBlockParameterWarning(splattee.Count);
                    }
                    item = new RubyArray(splattee);
                    break;
            }

            return _block(param, self, item);
        }
        
        // R(1, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, IList/*!*/ splattee) {
            if (splattee.Count > 0) {
                var array = new RubyArray(1 + splattee.Count);
                array.Add(arg1);
                array.AddRange(splattee);
                arg1 = array;

                if (!HasSingleCompoundParameter) {
                    param.MultipleValuesForBlockParameterWarning(array.Count);
                }
            }

            return _block(param, self, arg1);
        }

        private object InvokeSplatInternal(BlockParam/*!*/ param, object self, RubyArray/*!*/ array, IList/*!*/ splattee) {
            Debug.Assert(array.Count >= 2);

            RubyOps.SplatAppend(array, splattee);

            if (!HasSingleCompoundParameter) {
                param.MultipleValuesForBlockParameterWarning(array.Count);
            }

            return _block(param, self, array);
        }

        // R(2, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, IList/*!*/ splattee) {
            return InvokeSplatInternal(param, self, RubyOps.MakeArray2(arg1, arg2), splattee);
        }

        // R(3, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, IList/*!*/ splattee) {
            return InvokeSplatInternal(param, self, RubyOps.MakeArray3(arg1, arg2, arg3), splattee);
        }

        // R(4, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object arg4, IList/*!*/ splattee) {
            return InvokeSplatInternal(param, self, RubyOps.MakeArray4(arg1, arg2, arg3, arg4), splattee);
        }
        
        // R(N, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object[]/*!*/ args, IList/*!*/ splattee) {
            Debug.Assert(args.Length > MaxBlockArity);
            return InvokeSplatInternal(param, self, RubyOps.MakeArrayN(args), splattee);
        }

        // R(N, *, =)
        public override object InvokeSplatRhs(BlockParam/*!*/ param, object self, object[]/*!*/ args, IList/*!*/ splattee, object rhs) {
            var array = new RubyArray(args);
            RubyOps.SplatAppend(array, splattee);
            array.Add(rhs);

            if (array.Count == 1) {
                return _block(param, self, rhs);
            }

            Debug.Assert(array.Count >= 2);

            if (!HasSingleCompoundParameter) {
                param.MultipleValuesForBlockParameterWarning(array.Count);
            }
            
            return _block(param, self, array);
        }
    }

    // L(2, -)
    internal sealed class BlockDispatcher2 : BlockDispatcher<BlockCallTarget2> {
        public override int ParameterCount { get { return 2; } }

        public BlockDispatcher2(BlockSignatureAttributes attributesAndArity, string sourcePath, int sourceLine)
            : base(attributesAndArity, sourcePath, sourceLine) {
            Debug.Assert(!HasUnsplatParameter);
            Debug.Assert(!HasSingleCompoundParameter);
        }

        // R(0, -)
        public override object Invoke(BlockParam/*!*/ param, object self) {
            return _block(param, self, null, null);
        }

        // R(1, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1) {
            // MRI calls to_ary, but not to_a (contrary to real *splatting)
            IList list = arg1 as IList ?? Protocols.ConvertToArraySplat(param.RubyContext, arg1);
            if (list != null) {
                return InvokeSplatInternal(param, self, list);
            } else {
                return _block(param, self, arg1, null);
            }
        }

        // R(1, -)
        public override object InvokeNoAutoSplat(BlockParam/*!*/ param, object self, object arg1) {
            return _block(param, self, arg1, null);
        }

        // R(2, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1, object arg2) {
            return _block(param, self, arg1, arg2);
        }

        // R(3, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3) {
            return _block(param, self, arg1, arg2);
        }

        // R(4, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object arg4) {
            return _block(param, self, arg1, arg2);
        }
        
        // R(N, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object[]/*!*/ args) {
            Debug.Assert(args.Length > MaxBlockArity);
            return _block(param, self, args[0], args[1]);
        }

        // R(0, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, IList/*!*/ splattee) {
            return InvokeSplatInternal(param, self, splattee);
        }

        private object InvokeSplatInternal(BlockParam/*!*/ param, object self, IList/*!*/ splattee) {
            switch (splattee.Count) {
                case 0: return _block(param, self, null, null);
                case 1: return _block(param, self, splattee[0], null);
                default: return _block(param, self, splattee[0], splattee[1]);
            }
        }

        // R(1, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, IList/*!*/ splattee) {
            switch (splattee.Count) {
                case 0: return _block(param, self, arg1, null);
                default: return _block(param, self, arg1, splattee[0]);
            }
        }

        // R(2, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, IList/*!*/ splattee) {
            return _block(param, self, arg1, arg2);
        }

        // R(3, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, IList/*!*/ splattee) {
            return _block(param, self, arg1, arg2);
        }

        // R(4, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object arg4, IList/*!*/ splattee) {
            return _block(param, self, arg1, arg2);
        }

        // R(N, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object[]/*!*/ args, IList/*!*/ splattee) {
            Debug.Assert(args.Length > MaxBlockArity);
            return _block(param, self, args[0], args[1]);
        }

        // R(N, *, =)
        public override object InvokeSplatRhs(BlockParam/*!*/ param, object self, object[]/*!*/ args, IList/*!*/ splattee, object rhs) {
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
            Debug.Assert(!HasSingleCompoundParameter);
        }

        // R(0, -)
        public override object Invoke(BlockParam/*!*/ param, object self) {
            return _block(param, self, null, null, null);
        }

        // R(1, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1) {
            // MRI calls to_ary, but not to_a (contrary to real *splatting)
            IList splattee = arg1 as IList ?? Protocols.ConvertToArraySplat(param.RubyContext, arg1);
            if (splattee != null) {
                return InvokeSplatInternal(param, self, splattee);
            } else {
                return _block(param, self, arg1, null, null);
            }
        }

        // R(1, -)
        public override object InvokeNoAutoSplat(BlockParam/*!*/ param, object self, object arg1) {
            return _block(param, self, arg1, null, null);
        }

        // R(2, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1, object arg2) {
            return _block(param, self, arg1, arg2, null);
        }

        // R(3, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3) {
            return _block(param, self, arg1, arg2, arg3);
        }

        // R(4, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object arg4) {
            return _block(param, self, arg1, arg2, arg3);
        }

        // R(N, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object[]/*!*/ args) {
            Debug.Assert(args.Length > MaxBlockArity);
            return _block(param, self, args[1], args[2], args[3]);
        }

        // R(0, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, IList/*!*/ splattee) {
            return InvokeSplatInternal(param, self, splattee);
        }

        private object InvokeSplatInternal(BlockParam/*!*/ param, object self, IList/*!*/ splattee) {
            switch (splattee.Count) {
                case 0: return _block(param, self, null, null, null);
                case 1: return _block(param, self, splattee[0], null, null);
                case 2: return _block(param, self, splattee[0], splattee[1], null);
                default: return _block(param, self, splattee[0], splattee[1], splattee[2]);
            }
        }

        // R(1, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, IList/*!*/ splattee) {
            switch (splattee.Count) {
                case 0: return _block(param, self, arg1, null, null);
                case 1: return _block(param, self, arg1, splattee[0], null);
                default: return _block(param, self, arg1, splattee[0], splattee[1]);
            }
        }

        // R(2, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, IList/*!*/ splattee) {
            switch (splattee.Count) {
                case 0: return _block(param, self, arg1, arg2, null);
                default: return _block(param, self, arg1, arg2, splattee[0]);
            }
        }

        // R(3, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, IList/*!*/ splattee) {
            return _block(param, self, arg1, arg2, arg3);
        }

        // R(4, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object arg4, IList/*!*/ splattee) {
            return _block(param, self, arg1, arg2, arg3);
        }

        // R(N, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object[]/*!*/ args, IList/*!*/ splattee) {
            Debug.Assert(args.Length > MaxBlockArity);
            return _block(param, self, args[0], args[1], args[2]);
        }

        // R(N, *, =)
        public override object InvokeSplatRhs(BlockParam/*!*/ param, object self, object[]/*!*/ args, IList/*!*/ splattee, object rhs) {
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
            Debug.Assert(!HasSingleCompoundParameter);
        }

        // R(0, -)
        public override object Invoke(BlockParam/*!*/ param, object self) {
            return _block(param, self, null, null, null, null);
        }
        
        // R(1, -)
        public override object InvokeNoAutoSplat(BlockParam/*!*/ param, object self, object arg1) {
            return _block(param, self, arg1, null, null, null);
        }

        // R(1, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1) {
            // MRI calls to_ary, but not to_a (contrary to real *splatting)
            IList list = arg1 as IList ?? Protocols.ConvertToArraySplat(param.RubyContext, arg1);
            if (list != null) {
                return InvokeSplatInternal(param, self, list);
            } else {
                return _block(param, self, arg1, null, null, null);
            }
        }

        // R(2, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1, object arg2) {
            return _block(param, self, arg1, arg2, null, null);
        }

        // R(3, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3) {
            return _block(param, self, arg1, arg2, arg3, null);
        }

        // R(4, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object arg4) {
            return _block(param, self, arg1, arg2, arg3, arg4);
        }

        // R(N, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object[]/*!*/ args) {
            Debug.Assert(args.Length > MaxBlockArity);
            return _block(param, self, args[1], args[2], args[3], args[4]);
        }

        // R(0, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, IList/*!*/ splattee) {
            return InvokeSplatInternal(param, self, splattee);
        }

        private object InvokeSplatInternal(BlockParam/*!*/ param, object self, IList/*!*/ splattee) {
            switch (splattee.Count) {
                case 0: return _block(param, self, null, null, null, null);
                case 1: return _block(param, self, splattee[0], null, null, null);
                case 2: return _block(param, self, splattee[0], splattee[1], null, null);
                case 3: return _block(param, self, splattee[0], splattee[1], splattee[2], null);
                default: return _block(param, self, splattee[0], splattee[1], splattee[2], splattee[3]);
            }
        }

        // R(1, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, IList/*!*/ splattee) {
            switch (splattee.Count) {
                case 0: return _block(param, self, arg1, null, null, null);
                case 1: return _block(param, self, arg1, splattee[0], null, null);
                case 2: return _block(param, self, arg1, splattee[0], splattee[1], null);
                default: return _block(param, self, arg1, splattee[0], splattee[1], splattee[2]);
            }
        }

        // R(2, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, IList/*!*/ splattee) {
            switch (splattee.Count) {
                case 0: return _block(param, self, arg1, arg2, null, null);
                case 1: return _block(param, self, arg1, arg2, splattee[0], null);
                default: return _block(param, self, arg1, arg2, splattee[0], splattee[1]);
            }
        }

        // R(3, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, IList/*!*/ splattee) {
            switch (splattee.Count) {
                case 0: return _block(param, self, arg1, arg2, arg3, null);
                default: return _block(param, self, arg1, arg2, arg3, splattee[0]);
            }
        }

        // R(4, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object arg4, IList/*!*/ splattee) {
            return _block(param, self, arg1, arg2, arg3, arg4);
        }

        // R(N, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object[]/*!*/ args, IList/*!*/ splattee) {
            Debug.Assert(args.Length > MaxBlockArity);
            return _block(param, self, args[0], args[1], args[2], args[3]);
        }

        // R(N, *, =)
        public override object InvokeSplatRhs(BlockParam/*!*/ param, object self, object[]/*!*/ args, IList/*!*/ splattee, object rhs) {
            args = CreateArgumentsFromSplatteeAndRhs(4, args, splattee, rhs);
            return _block(param, self, args[0], args[1], args[2], args[3]);
        }
    }
}
