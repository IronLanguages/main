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
using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Reflection;
using IronRuby.Builtins;
using System.Collections.ObjectModel;
using System.Collections;
using System.Diagnostics;

namespace IronRuby.Runtime.Calls {
    using Ast = Expression;
    using AstFactory = IronRuby.Compiler.Ast.AstFactory;

    using BlockCallTarget0 = Func<BlockParam, object, object>;
    using BlockCallTarget1 = Func<BlockParam, object, object, object>;
    using BlockCallTarget2 = Func<BlockParam, object, object, object, object>;
    using BlockCallTarget3 = Func<BlockParam, object, object, object, object, object>;
    using BlockCallTarget4 = Func<BlockParam, object, object, object, object, object, object>;
    using BlockCallTargetN = Func<BlockParam, object, object[], object>;
    using BlockCallTargetProcN = Func<BlockParam, object, object[], Proc, object>;
    using BlockCallTargetUnsplatN = Func<BlockParam, object, object[], RubyArray, object>;
    using BlockCallTargetUnsplatProcN = Func<BlockParam, object, object[], RubyArray, Proc, object>;

    [Flags]
    public enum BlockSignatureAttributes {
        None = 0,

        // {|..., &b|}
        HasProcParameter = 1,

        // {|...,*,...|}
        HasUnsplatParameter = 2,        // TODO: 1.9: arity < 0 iff the block has unsplat => we can remove signature attributes enum

        // bits 31..3 store arity (might be different from formal parameter count)
    }

    internal abstract class BlockDispatcher<T> : BlockDispatcher where T : class {
        protected T _block;

        public BlockDispatcher(BlockSignatureAttributes attributesAndArity, string sourcePath, int sourceLine)
            : base(attributesAndArity, sourcePath, sourceLine) {
        }

        public override Delegate/*!*/ Method {
            get { return (Delegate)(object)_block; }
        }

        internal override BlockDispatcher/*!*/ SetMethod(object/*!*/ method) {
            // Note: this might potentially be executed by multiple threads. So we can assert that _block == null here.
            // It's ok if the delegate is overwritten multiple times since all the target methods are equivalent.
            _block = (T)method;
            return this;
        }
    }

    internal abstract class BlockDispatcherN<T> : BlockDispatcher<T> where T : class {
        protected readonly int _parameterCount;

        public override int ParameterCount { get { return _parameterCount; } }

        public BlockDispatcherN(int parameterCount, BlockSignatureAttributes attributesAndArity, string sourcePath, int sourceLine) 
            : base(attributesAndArity, sourcePath, sourceLine) {
            _parameterCount = parameterCount;
        }

        protected object[]/*!*/ MakeArray(object arg1) {
            var array = new object[_parameterCount];
            array[0] = arg1;
            return array;
        }

        protected object[]/*!*/ MakeArray(object arg1, object arg2) {
            var array = new object[_parameterCount];
            array[0] = arg1;
            array[1] = arg2;
            return array;
        }

        protected object[]/*!*/ MakeArray(object arg1, object arg2, object arg3) {
            var array = new object[_parameterCount];
            array[0] = arg1;
            array[1] = arg2;
            array[2] = arg3;
            return array;
        }

        protected object[]/*!*/ MakeArray(object arg1, object arg2, object arg3, object arg4) {
            var array = new object[_parameterCount];
            array[0] = arg1;
            array[1] = arg2;
            array[2] = arg3;
            array[3] = arg4;
            return array;
        }
    }

    public abstract class BlockDispatcher {
        // position of the block definition (opening brace):
        private readonly string _sourcePath;
        private readonly int _sourceLine;

        private readonly BlockSignatureAttributes _attributesAndArity;

        public bool HasUnsplatParameter {
            get { return (_attributesAndArity & BlockSignatureAttributes.HasUnsplatParameter) != 0; }
        }

        public bool HasProcParameter {
            get { return (_attributesAndArity & BlockSignatureAttributes.HasProcParameter) != 0; }
        }

        public int Arity {
            get { return ((int)_attributesAndArity >> 2); }
        }

        public static BlockSignatureAttributes MakeAttributes(BlockSignatureAttributes attributes, int arity) {
            return attributes | (BlockSignatureAttributes)(arity << 2);
        }

        // Doesn't include unsplat parameter. 
        // Includes anonymous parameter.
        public abstract int ParameterCount { get; }
        public abstract Delegate/*!*/ Method { get; }
        internal abstract BlockDispatcher/*!*/ SetMethod(object/*!*/ method);

        public string SourcePath { get { return _sourcePath; } }
        public int SourceLine { get { return _sourceLine; } }

        public abstract object Invoke(BlockParam/*!*/ param, object self, Proc procArg);
        public abstract object InvokeNoAutoSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1);
        public abstract object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1);
        public abstract object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2);
        public abstract object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3);
        public abstract object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4);
        public abstract object Invoke(BlockParam/*!*/ param, object self, Proc procArg, object[]/*!*/ args);

        public abstract object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, IList/*!*/ splattee);
        public abstract object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, IList/*!*/ splattee);
        public abstract object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, IList/*!*/ splattee);
        public abstract object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3, IList/*!*/ splattee);
        public abstract object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4, IList/*!*/ splattee);
        public abstract object InvokeSplat(BlockParam/*!*/ param, object self, Proc procArg, object[]/*!*/ args, IList/*!*/ splattee);

        public abstract object InvokeSplatRhs(BlockParam/*!*/ param, object self, Proc procArg, object[]/*!*/ args, IList/*!*/ splattee, object rhs);

        internal const int MaxBlockArity = 4;
        internal const int HiddenParameterCount = 2;

        internal BlockDispatcher(BlockSignatureAttributes attributesAndArity, string sourcePath, int sourceLine) {
            _attributesAndArity = attributesAndArity;
            _sourcePath = sourcePath;
            _sourceLine = sourceLine;
        }

        internal static BlockDispatcher/*!*/ Create(int parameterCount, BlockSignatureAttributes attributesAndArity, string sourcePath, int sourceLine) {
            if ((attributesAndArity & BlockSignatureAttributes.HasUnsplatParameter) == 0) {
                if ((attributesAndArity & BlockSignatureAttributes.HasProcParameter) == 0) {
                    switch (parameterCount) {
                        case 0: return new BlockDispatcher0(attributesAndArity, sourcePath, sourceLine);
                        case 1: return new BlockDispatcher1(attributesAndArity, sourcePath, sourceLine);
                        case 2: return new BlockDispatcher2(attributesAndArity, sourcePath, sourceLine);
                        case 3: return new BlockDispatcher3(attributesAndArity, sourcePath, sourceLine);
                        case 4: return new BlockDispatcher4(attributesAndArity, sourcePath, sourceLine);
                        default: return new BlockDispatcherN(parameterCount, attributesAndArity, sourcePath, sourceLine);
                    }
                } else {
                    return new BlockDispatcherProcN(parameterCount, attributesAndArity, sourcePath, sourceLine);
                }
            } else {
                if ((attributesAndArity & BlockSignatureAttributes.HasProcParameter) == 0) {
                    return new BlockDispatcherUnsplatN(parameterCount, attributesAndArity, sourcePath, sourceLine);
                } else {
                    return new BlockDispatcherUnsplatProcN(parameterCount, attributesAndArity, sourcePath, sourceLine);
                }
            }
        }

        internal static LambdaExpression/*!*/ CreateLambda(Expression body, string name, ICollection<ParameterExpression> parameters,
            int parameterCount, BlockSignatureAttributes attributes) {

            if ((attributes & BlockSignatureAttributes.HasUnsplatParameter) == 0) {
                if ((attributes & BlockSignatureAttributes.HasProcParameter) == 0) {
                    switch (parameterCount) {
                        case 0: return Ast.Lambda<BlockCallTarget0>(body, name, parameters);
                        case 1: return Ast.Lambda<BlockCallTarget1>(body, name, parameters);
                        case 2: return Ast.Lambda<BlockCallTarget2>(body, name, parameters);
                        case 3: return Ast.Lambda<BlockCallTarget3>(body, name, parameters);
                        case 4: return Ast.Lambda<BlockCallTarget4>(body, name, parameters);
                        default: return Ast.Lambda<BlockCallTargetN>(body, name, parameters);
                    }
                } else {
                    return Ast.Lambda<BlockCallTargetProcN>(body, name, parameters);
                }
            } else {
                if ((attributes & BlockSignatureAttributes.HasProcParameter) == 0) {
                    return Ast.Lambda<BlockCallTargetUnsplatN>(body, name, parameters);
                } else {
                    return Ast.Lambda<BlockCallTargetUnsplatProcN>(body, name, parameters);
                }
            }
        }

        private static void CopyArgumentsFromSplattee(object[]/*!*/ args, int initializedArgCount, int parameterCount,
            out int nextArg, out int nextItem, IList/*!*/ splattee) {

            int i = Math.Min(initializedArgCount, parameterCount);
            int j = 0;
            while (i < parameterCount && j < splattee.Count) {
                args[i++] = splattee[j++];
            }
        
            nextArg = i;
            nextItem = j;
        }

        // Expects first "initializeArgCount" slots of "args" array initialized with actual argument values 
        // and fills the rest by splatting "splattee". The size of the array "args" is the number of formal parameters the block takes.
        internal static object[]/*!*/ CopyArgumentsFromSplattee(object[]/*!*/ args, int initializedArgCount, IList/*!*/ splattee) {
            int nextArg, nextItem;
            CopyArgumentsFromSplattee(args, initializedArgCount, args.Length, out nextArg, out nextItem, splattee);
            return args;
        }

        internal static void CreateArgumentsFromSplattee(int parameterCount, out int nextArg, out int nextItem, ref object[]/*!*/ args, IList/*!*/ splattee) {
            // the args array is passed to the block, we need at least space for all explicit parameters:
            int originalLength = args.Length;
            if (args.Length < parameterCount) {
                Array.Resize(ref args, parameterCount);
            }

            CopyArgumentsFromSplattee(args, originalLength, parameterCount, out nextArg, out nextItem, splattee);
        }

        internal static object[]/*!*/ CreateArgumentsFromSplatteeAndRhs(int parameterCount, object[]/*!*/ args, IList/*!*/ splattee, object rhs) {
            int nextArg, nextItem;

            // the args array is passed to the block, we need at least space for all explicit parameters:
            CreateArgumentsFromSplattee(parameterCount, out nextArg, out nextItem, ref args, splattee);

            if (nextArg < args.Length) {
                args[nextArg++] = rhs;
            }

            return args;
        }

#if OBSOLETE
        private Expression/*!*/ AddWarning(Expression/*!*/ codeContextExpression, Expression/*!*/ expression) {
            Assert.NotNull(codeContextExpression, expression);

            // do not report warning if the only parameter is a nested left value:
            if (FirstArgumentIsNestedLValue) {
                return expression;
            }

            return Methods.MultipleValuesForBlockParameterWarning", codeContextExpression, expression);
        }

        private void SetCallRuleArguments(
            Expression/*!*/ blockParameterExpression, // special arg #0
            Expression/*!*/ selfParameterExpression,  // special arg #1
            CallArguments/*!*/ args,                  // user args
            Expression/*!*/ codeContextExpression,
            MetaObjectBuilder/*!*/ rule, 
            ArgsBuilder/*!*/ actualArgs) {

            // mandatory args:
            actualArgs.Add(blockParameterExpression);
            actualArgs.Add(selfParameterExpression);

            int parameterIndex = 0;

            // mimics CompoundLeftValue.TransformWrite //

            // L(1,-)?
            bool leftOneNone = OptionalParamCount == 1 && !HasParamsArray;

            // L(0,*)?
            bool leftNoneSplat = OptionalParamCount == 0 && HasParamsArray;

            // R(0,*)?
            bool rightNoneSplat = !args.Signature.IsSimple && args.Length == 1 && args.GetArgumentKind(0) == ArgumentKind.List;

            // R(1,-)?
            bool rightOneNone = !args.Signature.IsSimple && args.Length == 1 && args.GetArgumentKind(0) == ArgumentKind.Simple
                || args.Signature.IsSimple && args.Length == 1;

            // R(1,*)?
            bool rightOneSplat = !args.Signature.IsSimple && args.Length == 2 &&
                args.GetArgumentKind(0) == ArgumentKind.Simple &&
                args.GetArgumentKind(1) == ArgumentKind.List;

            // R(0,-)?
            bool rightNoneNone = args.Length == 0;

            if (leftOneNone) {
                Expression rvalue;

                if (rightOneNone) {
                    // simple assignment
                    rvalue = args.Expressions[parameterIndex];
                } else if (rightOneSplat && TestEmptyList(rule, args.Values[parameterIndex + 1], args.Expressions[parameterIndex + 1])) {
                    // simple assignment if the splatted value is an empty array:
                    rvalue = args.Expressions[parameterIndex];
                } else if (rightNoneNone) {
                    // nil assignment
                    rvalue = AddWarning(codeContextExpression, AstUtils.Constant(null));
                } else if (rightNoneSplat) {
                    // Splat(RHS[*]):
                    rvalue = MakeArgumentSplatWithWarning(rule, args.Values[parameterIndex], args.Expressions[parameterIndex], codeContextExpression);
                } else {
                    // more than one argument -> pack to an array + warning

                    // MakeArray(RHS) + SplatAppend(RHS*):
                    List<Expression> arguments = new List<Expression>();
                    AddBlockArguments(rule, arguments, args, parameterIndex);
                    rvalue = AddWarning(codeContextExpression, ArgsBuilder.MakeArgsArray(arguments));
                }

                actualArgs.Add(rvalue);

            } else {

                // R(0,*) || R(1,-) && !L(0,*) ==> CompoundLeftValue.TransformWrite does Unsplat, MakeArray otherwise.
                // 
                // However, we are not constructing a materalized resulting array (contrary to CompoundLeftValue.TransformWrite).
                // The resulting array is comprised of slots on the stack (loaded to the formal parameters of the block #1, ..., #n).
                // Therefore, we effectively need to take items of imaginary Unsplat's result and put them into the actualArgs as arguments.
                //
                // Unsplat of x makes an array containing x if x is not an array, otherwise it returns x.
                // So, we just need to take elements of x and push them onto the stack.
                //

                List<Expression> arguments = new List<Expression>();

                if (rightNoneSplat) {
                    ArgsBuilder.SplatListToArguments(rule, arguments, args.Values[parameterIndex], args.Expressions[parameterIndex], false);
                } else if (rightOneNone && !leftNoneSplat) {
                    ArgsBuilder.SplatListToArguments(rule, arguments, args.Values[parameterIndex], args.Expressions[parameterIndex], true);
                } else {
                    AddBlockArguments(rule, arguments, args, parameterIndex);
                }

                actualArgs.AddRange(arguments);
            }

            actualArgs.AddForEachMissingArgument(delegate() { return AstUtils.Constant(null); });

            if (HasParamsArray) {
                actualArgs.AddParamsArray();
            }
        }

        private bool TestEmptyList(MetaObjectBuilder/*!*/ rule, object arg, Expression/*!*/ parameter) {
            int listLength;
            ParameterExpression listVariable;
            return ArgsBuilder.AddTestForListArg(rule, arg, parameter, out listLength, out listVariable) && listLength == 0;
        }

        private Expression/*!*/ MakeArgumentSplatWithWarning(MetaObjectBuilder/*!*/ rule, object arg, Expression/*!*/ parameter,
            Expression/*!*/ codeContextExpression) {

            int listLength;
            ParameterExpression listVariable;
            if (ArgsBuilder.AddTestForListArg(rule, arg, parameter, out listLength, out listVariable)) {
                if (listLength == 0) {
                    // return nil argument + Warning
                    return AddWarning(codeContextExpression, AstUtils.Constant(null));
                } else if (listLength == 1) {
                    // return the only item of the array:
                    return Ast.Call(
                        listVariable,
                        typeof(IList).GetMethod("get_Item"),
                        AstUtils.Constant(0)
                    );
                } else {
                    // return the array itself + Warning:
                    return AddWarning(codeContextExpression, parameter);
                }
            } else {
                // not an array, return the value:
                return parameter;
            }
        }

        private Expression/*!*/ MakeArgumentUnsplat(MetaObjectBuilder/*!*/ rule, object arg, Expression/*!*/ parameter) {
            int listLength;
            ParameterExpression listVariable;
            if (ArgsBuilder.AddTestForListArg(rule, arg, parameter, out listLength, out listVariable)) {
                // an array, return:
                return parameter;
            } else {
                // not an array, wrap:
                return AstFactory.OptimizedOpCall("MakeArray", parameter);
            }
        }

        private void AddBlockArguments(MetaObjectBuilder/*!*/ rule, List<Expression>/*!*/ actualArgs, CallArguments/*!*/ args, int parameterIndex) {

            while (parameterIndex < args.Length) {
                switch (args.GetArgumentKind(parameterIndex)) {
                    case ArgumentKind.Simple:
                        actualArgs.Add(args.Expressions[parameterIndex]);
                        break;

                    case ArgumentKind.List:
                        ArgsBuilder.SplatListToArguments(rule, actualArgs, args.Values[parameterIndex], args.Expressions[parameterIndex], false);
                        break;

                    case ArgumentKind.Instance:
                    case ArgumentKind.Block:
                    default:
                        throw new NotImplementedException();
                }

                parameterIndex++;
            }
        }
#endif
    }
}