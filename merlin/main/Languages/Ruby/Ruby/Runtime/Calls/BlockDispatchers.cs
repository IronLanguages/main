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

namespace IronRuby.Runtime.Calls {
    // L(0, -)
    internal sealed class BlockDispatcher0 : BlockDispatcher {
        private readonly BlockCallTarget0/*!*/ _block;

        public override Delegate/*!*/ Method { get { return _block; } }
        public override int ParameterCount { get { return 0; } }

        public BlockDispatcher0(BlockCallTarget0/*!*/ block, BlockSignatureAttributes attributes)
            : base(attributes) {
            Assert.NotNull(block);
            Debug.Assert(!HasUnsplatParameter);

            _block = block;
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
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object splattee) {
            return _block(param, self);
        }

        // R(1, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object splattee) {
            return _block(param, self);
        }

        // R(2, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object splattee) {
            return _block(param, self);
        }

        // R(3, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object splattee) {
            return _block(param, self);
        }

        // R(4, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object arg4, object splattee) {
            return _block(param, self);
        }

        // R(N, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object[]/*!*/ args, object splattee) {
            return _block(param, self);
        }

        // R(N, *, =)
        public override object InvokeSplatRhs(BlockParam/*!*/ param, object self, object[]/*!*/ args, object splattee, object rhs) {
            return _block(param, self);
        }
    }

    // L(1, -)
    internal sealed class BlockDispatcher1 : BlockDispatcher {
        private readonly BlockCallTarget1/*!*/ _block;

        public override Delegate/*!*/ Method { get { return _block; } }
        public override int ParameterCount { get { return 1; } }

        public BlockDispatcher1(BlockCallTarget1/*!*/ block, BlockSignatureAttributes attributes)
            : base(attributes) {
            Assert.NotNull(block);
            Debug.Assert(!HasUnsplatParameter);
            _block = block;
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
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object splattee) {
            var list = splattee as List<object>;
            if (list != null) {
                switch (list.Count) {
                    case 0:
                        if (!HasSingleCompoundParameter) {
                            param.MultipleValuesForBlockParameterWarning(list.Count);
                        }
                        splattee = null; 
                        break;

                    case 1: 
                        splattee = list[0]; 
                        break;

                    default:
                        if (!HasSingleCompoundParameter) {
                            param.MultipleValuesForBlockParameterWarning(list.Count);
                        }
                        splattee = new RubyArray(list);
                        break;
                }
            }

            return _block(param, self, splattee);
        }
        
        // R(1, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object splattee) {
            var list = splattee as List<object>;
            if (list == null) {
                if (!HasSingleCompoundParameter) {
                    param.MultipleValuesForBlockParameterWarning(2);
                }

                arg1 = RubyOps.MakeArray2(arg1, splattee);
            } else if (list.Count > 0) {
                var array = RubyOps.MakeArray1(arg1);
                array.AddRange(list);
                arg1 = array;

                if (!HasSingleCompoundParameter) {
                    param.MultipleValuesForBlockParameterWarning(array.Count);
                }
            }

            return _block(param, self, arg1);
        }

        private object InvokeSplatInternal(BlockParam/*!*/ param, object self, RubyArray/*!*/ array, object splattee) {
            Debug.Assert(array.Count >= 2);

            RubyOps.SplatAppend(array, splattee);

            if (!HasSingleCompoundParameter) {
                param.MultipleValuesForBlockParameterWarning(array.Count);
            }

            return _block(param, self, array);
        }

        // R(2, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object splattee) {
            return InvokeSplatInternal(param, self, RubyOps.MakeArray2(arg1, arg2), splattee);
        }

        // R(3, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object splattee) {
            return InvokeSplatInternal(param, self, RubyOps.MakeArray3(arg1, arg2, arg3), splattee);
        }

        // R(4, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object arg4, object splattee) {
            return InvokeSplatInternal(param, self, RubyOps.MakeArray4(arg1, arg2, arg3, arg4), splattee);
        }
        
        // R(N, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object[]/*!*/ args, object splattee) {
            Debug.Assert(args.Length > MaxBlockArity);
            return InvokeSplatInternal(param, self, RubyOps.MakeArrayN(args), splattee);
        }

        // R(N, *, =)
        public override object InvokeSplatRhs(BlockParam/*!*/ param, object self, object[]/*!*/ args, object splattee, object rhs) {
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
    internal sealed class BlockDispatcher2 : BlockDispatcher {
        private readonly BlockCallTarget2/*!*/ _block;

        public override Delegate/*!*/ Method { get { return _block; } }
        public override int ParameterCount { get { return 2; } }

        public BlockDispatcher2(BlockCallTarget2/*!*/ block, BlockSignatureAttributes attributes)
            : base(attributes) {
            Assert.NotNull(block);
            Debug.Assert(!HasUnsplatParameter);
            _block = block;
        }

        // R(0, -)
        public override object Invoke(BlockParam/*!*/ param, object self) {
            return _block(param, self, null, null);
        }

        // R(1, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1) {
            return InvokeSplatInternal(param, self, arg1);
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
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object splattee) {
            return InvokeSplatInternal(param, self, splattee);
        }

        private object InvokeSplatInternal(BlockParam/*!*/ param, object self, object splattee) {
            var list = splattee as List<object>;
            if (list != null) {
                switch (list.Count) {
                    case 0: return _block(param, self, null, null);
                    case 1: return _block(param, self, list[0], null);
                    default: return _block(param, self, list[0], list[1]);
                }
            }

            return _block(param, self, splattee, null);
        }

        // R(1, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object splattee) {
            var list = splattee as List<object>;
            if (list != null) {
                switch (list.Count) {
                    case 0: return _block(param, self, arg1, null);
                    default: return _block(param, self, arg1, list[0]);
                }
            }

            return _block(param, self, arg1, splattee);
        }

        // R(2, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object splattee) {
            return _block(param, self, arg1, arg2);
        }

        // R(3, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object splattee) {
            return _block(param, self, arg1, arg2);
        }

        // R(4, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object arg4, object splattee) {
            return _block(param, self, arg1, arg2);
        }

        // R(N, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object[]/*!*/ args, object splattee) {
            Debug.Assert(args.Length > MaxBlockArity);
            return _block(param, self, args[0], args[1]);
        }

        // R(N, *, =)
        public override object InvokeSplatRhs(BlockParam/*!*/ param, object self, object[]/*!*/ args, object splattee, object rhs) {
            args = CreateArgumentsFromSplatteeAndRhs(2, args, splattee, rhs);
            return _block(param, self, args[0], args[1]);
        }
    }

    // L(3, -)
    internal sealed class BlockDispatcher3 : BlockDispatcher {
        private readonly BlockCallTarget3/*!*/ _block;

        public override Delegate/*!*/ Method { get { return _block; } }
        public override int ParameterCount { get { return 3; } }

        public BlockDispatcher3(BlockCallTarget3/*!*/ block, BlockSignatureAttributes attributes)
            : base(attributes) {
            Assert.NotNull(block);
            Debug.Assert(!HasUnsplatParameter);
            _block = block;
        }

        // R(0, -)
        public override object Invoke(BlockParam/*!*/ param, object self) {
            return _block(param, self, null, null, null);
        }

        // R(1, -)
        public override object Invoke(BlockParam/*!*/ param, object self, object arg1) {
            return InvokeSplatInternal(param, self, arg1);
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
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object splattee) {
            return InvokeSplatInternal(param, self, splattee);
        }

        private object InvokeSplatInternal(BlockParam/*!*/ param, object self, object splattee) {
            var list = splattee as List<object>;
            if (list != null) {
                switch (list.Count) {
                    case 0: return _block(param, self, null, null, null);
                    case 1: return _block(param, self, list[0], null, null);
                    case 2: return _block(param, self, list[0], list[1], null);
                    default: return _block(param, self, list[0], list[1], list[2]);
                }
            }

            return _block(param, self, splattee, null, null);
        }

        // R(1, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object splattee) {
            var list = splattee as List<object>;
            if (list != null) {
                switch (list.Count) {
                    case 0: return _block(param, self, arg1, null, null);
                    case 1: return _block(param, self, arg1, list[0], null);
                    default: return _block(param, self, arg1, list[0], list[1]);
                }
            }

            return _block(param, self, arg1, splattee, null);
        }

        // R(2, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object splattee) {
            var list = splattee as List<object>;
            if (list != null) {
                switch (list.Count) {
                    case 0: return _block(param, self, arg1, arg2, null);
                    default: return _block(param, self, arg1, arg2, list[0]);
                }
            }

            return _block(param, self, arg1, arg2, splattee);
        }

        // R(3, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object splattee) {
            return _block(param, self, arg1, arg2, arg3);
        }

        // R(4, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object arg4, object splattee) {
            return _block(param, self, arg1, arg2, arg3);
        }

        // R(N, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object[]/*!*/ args, object splattee) {
            Debug.Assert(args.Length > MaxBlockArity);
            return _block(param, self, args[0], args[1], args[2]);
        }

        // R(N, *, =)
        public override object InvokeSplatRhs(BlockParam/*!*/ param, object self, object[]/*!*/ args, object splattee, object rhs) {
            args = CreateArgumentsFromSplatteeAndRhs(3, args, splattee, rhs);
            return _block(param, self, args[0], args[1], args[2]);
        }
    }

    // L(4, -)
    internal sealed class BlockDispatcher4 : BlockDispatcher {
        private readonly BlockCallTarget4/*!*/ _block;

        public override Delegate/*!*/ Method { get { return _block; } }
        public override int ParameterCount { get { return 4; } }

        public BlockDispatcher4(BlockCallTarget4/*!*/ block, BlockSignatureAttributes attributes)
            : base(attributes) {
            Assert.NotNull(block);
            Debug.Assert(!HasUnsplatParameter);
            _block = block;
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
            return InvokeSplatInternal(param, self, arg1);
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
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object splattee) {
            return InvokeSplatInternal(param, self, splattee);
        }

        private object InvokeSplatInternal(BlockParam/*!*/ param, object self, object splattee) {
            var list = splattee as List<object>;
            if (list != null) {
                switch (list.Count) {
                    case 0: return _block(param, self, null, null, null, null);
                    case 1: return _block(param, self, list[0], null, null, null);
                    case 2: return _block(param, self, list[0], list[1], null, null);
                    case 3: return _block(param, self, list[0], list[1], list[2], null);
                    default: return _block(param, self, list[0], list[1], list[2], list[3]);
                }
            }

            return _block(param, self, splattee, null, null, null);
        }

        // R(1, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object splattee) {
            var list = splattee as List<object>;
            if (list != null) {
                switch (list.Count) {
                    case 0: return _block(param, self, arg1, null, null, null);
                    case 1: return _block(param, self, arg1, list[0], null, null);
                    case 2: return _block(param, self, arg1, list[0], list[1], null);
                    default: return _block(param, self, arg1, list[0], list[1], list[2]);
                }
            }

            return _block(param, self, arg1, splattee, null, null);
        }

        // R(2, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object splattee) {
            var list = splattee as List<object>;
            if (list != null) {
                switch (list.Count) {
                    case 0: return _block(param, self, arg1, arg2, null, null);
                    case 1: return _block(param, self, arg1, arg2, list[0], null);
                    default: return _block(param, self, arg1, arg2, list[0], list[1]);
                }
            }

            return _block(param, self, arg1, arg2, splattee, null);
        }

        // R(3, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object splattee) {
            var list = splattee as List<object>;
            if (list != null) {
                switch (list.Count) {
                    case 0: return _block(param, self, arg1, arg2, arg3, null);
                    default: return _block(param, self, arg1, arg2, arg3, list[0]);
                }
            }

            return _block(param, self, arg1, arg2, arg3, splattee);
        }

        // R(4, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object arg1, object arg2, object arg3, object arg4, object splattee) {
            return _block(param, self, arg1, arg2, arg3, arg4);
        }

        // R(N, *)
        public override object InvokeSplat(BlockParam/*!*/ param, object self, object[]/*!*/ args, object splattee) {
            Debug.Assert(args.Length > MaxBlockArity);
            return _block(param, self, args[0], args[1], args[2], args[3]);
        }

        // R(N, *, =)
        public override object InvokeSplatRhs(BlockParam/*!*/ param, object self, object[]/*!*/ args, object splattee, object rhs) {
            args = CreateArgumentsFromSplatteeAndRhs(4, args, splattee, rhs);
            return _block(param, self, args[0], args[1], args[2], args[3]);
        }
    }
}
