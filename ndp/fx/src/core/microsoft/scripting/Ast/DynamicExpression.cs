/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions.Compiler;
using System.Runtime.CompilerServices;
using System.Dynamic.Utils;

namespace System.Linq.Expressions {
    /// <summary>
    /// A late-bound operation. The precise semantics is determined by the
    /// Binder. If the Binder is one of the standard dynamic operations
    /// supported by MetaObject, the run-time behavior can be infered from the
    /// StandardAction
    /// </summary>
    public class DynamicExpression : Expression, IArgumentProvider {
        private readonly CallSiteBinder _binder;
        private readonly Type _delegateType;

        internal DynamicExpression(Type delegateType, CallSiteBinder binder) {
            Debug.Assert(delegateType.GetMethod("Invoke").GetReturnType() == typeof(object) || GetType() != typeof(DynamicExpression));
            _delegateType = delegateType;
            _binder = binder;
        }

        internal static DynamicExpression Make(Type returnType, Type delegateType, CallSiteBinder binder, ReadOnlyCollection<Expression> arguments) {
            if (returnType == typeof(object)) {
                return new DynamicExpressionN(delegateType, binder, arguments);
            } else {
                return new TypedDynamicExpressionN(returnType, delegateType, binder, arguments);
            }
        }

        protected override Type GetExpressionType() {
            return typeof(object);
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Dynamic;
        }
        /// <summary>
        /// The CallSiteBinder, which determines the runtime behavior of the
        /// dynamic site
        /// </summary>
        public CallSiteBinder Binder {
            get { return _binder; }
        }

        /// <summary>
        /// The type of the CallSite's delegate
        /// </summary>
        public Type DelegateType {
            get { return _delegateType; }
        }

        /// <summary>
        /// Arguments to the dynamic operation
        /// </summary>
        public ReadOnlyCollection<Expression> Arguments {
            get { return GetOrMakeArguments(); }
        }

        internal virtual ReadOnlyCollection<Expression> GetOrMakeArguments() {
            throw Assert.Unreachable;
        }

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitDynamic(this);
        }

        /// <summary>
        /// Makes a copy of this node replacing the args with the provided values.  The 
        /// number of the args needs to match the number of the current block.
        /// 
        /// This helper is provided to allow re-writing of nodes to not depend on the specific optimized
        /// subclass of DynamicExpression which is being used. 
        /// </summary>
        internal virtual DynamicExpression Rewrite(Expression[] args) {
            throw Assert.Unreachable;
        }

        #region IArgumentProvider Members

        Expression IArgumentProvider.GetArgument(int index) {
            throw Assert.Unreachable;
        }

        int IArgumentProvider.ArgumentCount {
            get { throw Assert.Unreachable; }
        }

        #endregion
    }

    #region Specialized Subclasses

    internal class DynamicExpressionN : DynamicExpression, IArgumentProvider {
        private IList<Expression> _arguments;       // storage for the original IList or ROC.  See IArgumentProvider for more info.

        internal DynamicExpressionN(Type delegateType, CallSiteBinder binder, IList<Expression> arguments)
            : base(delegateType, binder) {            
            _arguments = arguments;
        }

        Expression IArgumentProvider.GetArgument(int index) {
            return _arguments[index];
        }

        int IArgumentProvider.ArgumentCount {
            get {
                return _arguments.Count;
            }
        }

        internal override ReadOnlyCollection<Expression> GetOrMakeArguments() {
            return ReturnReadOnly(ref _arguments);
        }

        internal override DynamicExpression Rewrite(Expression[] args) {
            Debug.Assert(args.Length == ((IArgumentProvider)this).ArgumentCount);

            return new DynamicExpressionN(DelegateType, Binder, args);
        }
    }

    internal class TypedDynamicExpressionN : DynamicExpressionN {
        private readonly Type _returnType;

        internal TypedDynamicExpressionN(Type returnType, Type delegateType, CallSiteBinder binder, IList<Expression> arguments)
            : base(delegateType, binder, arguments) {
            Debug.Assert(delegateType.GetMethod("Invoke").GetReturnType() == returnType);
            _returnType = returnType;
        }

        protected override Type GetExpressionType() {
            return _returnType;
        }

        internal override DynamicExpression Rewrite(Expression[] args) {
            Debug.Assert(args.Length == ((IArgumentProvider)this).ArgumentCount);

            return new TypedDynamicExpressionN(GetExpressionType(), DelegateType, Binder, args);
        }
    }

    internal class DynamicExpression1 : DynamicExpression, IArgumentProvider {
        private object _arg0;               // storage for the 1st argument or a ROC.  See IArgumentProvider for more info.

        internal DynamicExpression1(Type delegateType, CallSiteBinder binder, Expression arg0)
            : base(delegateType, binder) {
            _arg0 = arg0;
        }

        Expression IArgumentProvider.GetArgument(int index) {
            switch(index) {
                case 0: return ReturnObject<Expression>(_arg0);
                default: throw new InvalidOperationException();
            }
        }

        int IArgumentProvider.ArgumentCount {
            get {
                return 1;
            }
        }

        internal override ReadOnlyCollection<Expression> GetOrMakeArguments() {
            return ReturnReadOnly(this, ref _arg0);
        }

        internal override DynamicExpression Rewrite(Expression[] args) {
            Debug.Assert(args.Length == 1);

            return new DynamicExpression1(DelegateType, Binder, args[0]);
        }
    }

    internal sealed class TypedDynamicExpression1 : DynamicExpression1 {
        private readonly Type _retType;

        internal TypedDynamicExpression1(Type retType, Type delegateType, CallSiteBinder binder, Expression arg0)
            : base(delegateType, binder, arg0) {
            _retType = retType;
        }

        protected override Type GetExpressionType() {
            return _retType;
        }

        internal override DynamicExpression Rewrite(Expression[] args) {
            Debug.Assert(args.Length == 1);

            return new TypedDynamicExpression1(GetExpressionType(), DelegateType, Binder, args[0]);
        }
    }

    internal class DynamicExpression2 : DynamicExpression, IArgumentProvider {
        private object _arg0;                   // storage for the 1st argument or a ROC.  See IArgumentProvider for more info.
        private readonly Expression _arg1;      // storage for the 2nd argument

        internal DynamicExpression2(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1)
            : base(delegateType, binder) {
            _arg0 = arg0;
            _arg1 = arg1;
        }

        Expression IArgumentProvider.GetArgument(int index) {
            switch (index) {
                case 0: return ReturnObject<Expression>(_arg0);
                case 1: return _arg1;
                default: throw new InvalidOperationException();
            }
        }

        int IArgumentProvider.ArgumentCount {
            get {
                return 2;
            }
        }

        internal override ReadOnlyCollection<Expression> GetOrMakeArguments() {
            return ReturnReadOnly(this, ref _arg0);
        }

        internal override DynamicExpression Rewrite(Expression[] args) {
            Debug.Assert(args.Length == 2);

            return new DynamicExpression2(DelegateType, Binder, args[0], args[1]);
        }
    }

    internal sealed class TypedDynamicExpression2 : DynamicExpression2 {
        private readonly Type _retType;

        internal TypedDynamicExpression2(Type retType, Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1)
            : base(delegateType, binder, arg0, arg1) {
            _retType = retType;
        }

        protected override Type GetExpressionType() {
            return _retType;
        }

        internal override DynamicExpression Rewrite(Expression[] args) {
            Debug.Assert(args.Length == 2);

            return new TypedDynamicExpression2(GetExpressionType(), DelegateType, Binder, args[0], args[1]);
        }
    }

    internal class DynamicExpression3 : DynamicExpression, IArgumentProvider {
        private object _arg0;                       // storage for the 1st argument or a ROC.  See IArgumentProvider for more info.
        private readonly Expression _arg1, _arg2;   // storage for the 2nd & 3rd arguments

        internal DynamicExpression3(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2)
            : base(delegateType, binder) {
            _arg0 = arg0;
            _arg1 = arg1;
            _arg2 = arg2;
        }

        Expression IArgumentProvider.GetArgument(int index) {
            switch (index) {
                case 0: return ReturnObject<Expression>(_arg0);
                case 1: return _arg1;
                case 2: return _arg2;
                default: throw new InvalidOperationException();
            }
        }

        int IArgumentProvider.ArgumentCount {
            get {
                return 3;
            }
        }

        internal override ReadOnlyCollection<Expression> GetOrMakeArguments() {
            return ReturnReadOnly(this, ref _arg0);
        }

        internal override DynamicExpression Rewrite(Expression[] args) {
            Debug.Assert(args.Length == 3);

            return new DynamicExpression3(DelegateType, Binder, args[0], args[1], args[2]);
        }
    }

    internal sealed class TypedDynamicExpression3 : DynamicExpression3 {
        private readonly Type _retType;

        internal TypedDynamicExpression3(Type retType, Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2)
            : base(delegateType, binder, arg0, arg1, arg2) {
            _retType = retType;
        }

        protected override Type GetExpressionType() {
            return _retType;
        }


        internal override DynamicExpression Rewrite(Expression[] args) {
            Debug.Assert(args.Length == 3);

            return new TypedDynamicExpression3(GetExpressionType(), DelegateType, Binder, args[0], args[1], args[2]);
        }
    }

    internal class DynamicExpression4 : DynamicExpression, IArgumentProvider {
        private object _arg0;                               // storage for the 1st argument or a ROC.  See IArgumentProvider for more info.
        private readonly Expression _arg1, _arg2, _arg3;    // storage for the 2nd - 4th arguments

        internal DynamicExpression4(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2, Expression arg3)
            : base(delegateType, binder) {
            _arg0 = arg0;
            _arg1 = arg1;
            _arg2 = arg2;
            _arg3 = arg3;
        }

        Expression IArgumentProvider.GetArgument(int index) {
            switch (index) {
                case 0: return ReturnObject<Expression>(_arg0);
                case 1: return _arg1;
                case 2: return _arg2;
                case 3: return _arg3;
                default: throw new InvalidOperationException();
            }
        }

        int IArgumentProvider.ArgumentCount {
            get {
                return 4;
            }
        }

        internal override ReadOnlyCollection<Expression> GetOrMakeArguments() {
            return ReturnReadOnly(this, ref _arg0);
        }

        internal override DynamicExpression Rewrite(Expression[] args) {
            Debug.Assert(args.Length == 4);

            return new DynamicExpression4(DelegateType, Binder, args[0], args[1], args[2], args[3]);
        }
    }

    internal sealed class TypedDynamicExpression4 : DynamicExpression4 {
        private readonly Type _retType;

        internal TypedDynamicExpression4(Type retType, Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2, Expression arg3)
            : base(delegateType, binder, arg0, arg1, arg2, arg3) {
            _retType = retType;
        }

        protected override Type GetExpressionType() {
            return _retType;
        }

        internal override DynamicExpression Rewrite(Expression[] args) {
            Debug.Assert(args.Length == 4);

            return new TypedDynamicExpression4(GetExpressionType(), DelegateType, Binder, args[0], args[1], args[2], args[3]);
        }
    }

    #endregion

    public partial class Expression {

        public static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, params Expression[] arguments) {
            return MakeDynamic(delegateType, binder, (IEnumerable<Expression>)arguments);
        }

        public static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, IEnumerable<Expression> arguments) {
            ContractUtils.RequiresNotNull(delegateType, "delegateType");
            ContractUtils.RequiresNotNull(binder, "binder");
            ContractUtils.Requires(delegateType.IsSubclassOf(typeof(Delegate)), "delegateType", Strings.TypeMustBeDerivedFromSystemDelegate);

            var method = GetValidMethodForDynamic(delegateType);

            var args = arguments.ToReadOnly();
            ValidateArgumentTypes(method, ExpressionType.Dynamic, ref args);

            return DynamicExpression.Make(method.GetReturnType(), delegateType, binder, args);
        }

        private static System.Reflection.MethodInfo GetValidMethodForDynamic(Type delegateType) {
            var method = delegateType.GetMethod("Invoke");
            var pi = method.GetParametersCached();
            ContractUtils.Requires(pi.Length > 0 && pi[0].ParameterType == typeof(CallSite), "delegateType", Strings.FirstArgumentMustBeCallSite);
            return method;
        }

        public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, params Expression[] arguments) {
            return Dynamic(binder, returnType, (IEnumerable<Expression>)arguments);
        }

        public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0) {
            ContractUtils.RequiresNotNull(binder, "binder");
            ValidateDynamicArgument(arg0);

            DelegateHelpers.TypeInfo info = DelegateHelpers.GetNextTypeInfo(
                returnType,
                DelegateHelpers.GetNextTypeInfo(
                    arg0.Type, 
                    DelegateHelpers.NextTypeInfo(typeof(CallSite))
                )
            );

            Type delegateType = info.DelegateType;
            if (delegateType == null) {
                delegateType = info.MakeDelegateType(returnType, arg0);
            }

            if (returnType == typeof(object)) {
                return new DynamicExpression1(delegateType, binder, arg0);
            } else {
                return new TypedDynamicExpression1(returnType, delegateType, binder, arg0);
            }
        }

        public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1) {
            ContractUtils.RequiresNotNull(binder, "binder");
            ValidateDynamicArgument(arg0);
            ValidateDynamicArgument(arg1);

            DelegateHelpers.TypeInfo info = DelegateHelpers.GetNextTypeInfo(
                returnType,
                DelegateHelpers.GetNextTypeInfo(
                    arg1.Type,
                    DelegateHelpers.GetNextTypeInfo(
                        arg0.Type,
                        DelegateHelpers.NextTypeInfo(typeof(CallSite))
                    )
                )
            );

            Type delegateType = info.DelegateType;
            if (delegateType == null) {
                delegateType = info.MakeDelegateType(returnType, arg0, arg1);
            }

            if (returnType == typeof(object)) {
                return new DynamicExpression2(delegateType, binder, arg0, arg1);
            } else {
                return new TypedDynamicExpression2(returnType, delegateType, binder, arg0, arg1);
            }
        }

        public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1, Expression arg2) {
            ContractUtils.RequiresNotNull(binder, "binder");
            ValidateDynamicArgument(arg0);
            ValidateDynamicArgument(arg1);
            ValidateDynamicArgument(arg2);

            DelegateHelpers.TypeInfo info = DelegateHelpers.GetNextTypeInfo(
                returnType,
                DelegateHelpers.GetNextTypeInfo(
                    arg2.Type,
                    DelegateHelpers.GetNextTypeInfo(
                        arg1.Type,
                        DelegateHelpers.GetNextTypeInfo(
                            arg0.Type,
                            DelegateHelpers.NextTypeInfo(typeof(CallSite))
                        )
                    )
                )
            );

            Type delegateType = info.DelegateType;
            if (delegateType == null) {
                delegateType = info.MakeDelegateType(returnType, arg0, arg1, arg2);
            }

            if (returnType == typeof(object)) {
                return new DynamicExpression3(delegateType, binder, arg0, arg1, arg2);
            } else {
                return new TypedDynamicExpression3(returnType, delegateType, binder, arg0, arg1, arg2);
            }
        }

        public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1, Expression arg2, Expression arg3) {
            ContractUtils.RequiresNotNull(binder, "binder");
            ValidateDynamicArgument(arg0);
            ValidateDynamicArgument(arg1);
            ValidateDynamicArgument(arg2);
            ValidateDynamicArgument(arg3);

            DelegateHelpers.TypeInfo info = DelegateHelpers.GetNextTypeInfo(
                returnType,
                DelegateHelpers.GetNextTypeInfo(
                    arg3.Type,
                    DelegateHelpers.GetNextTypeInfo(
                        arg2.Type,
                        DelegateHelpers.GetNextTypeInfo(
                            arg1.Type,
                            DelegateHelpers.GetNextTypeInfo(
                                arg0.Type,
                                DelegateHelpers.NextTypeInfo(typeof(CallSite))
                            )
                        )
                    )
                )
            );

            Type delegateType = info.DelegateType;
            if (delegateType == null) {
                delegateType = info.MakeDelegateType(returnType, arg0, arg1, arg2, arg3);
            }

            if (returnType == typeof(object)) {
                return new DynamicExpression4(delegateType, binder, arg0, arg1, arg2, arg3);
            } else {
                return new TypedDynamicExpression4(returnType, delegateType, binder, arg0, arg1, arg2, arg3);
            }   
        }

        public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, IEnumerable<Expression> arguments) {
            ContractUtils.RequiresNotNull(arguments, "arguments");
            ContractUtils.RequiresNotNull(returnType, "returnType");

            var args = arguments.ToReadOnly();
            ContractUtils.RequiresNotEmpty(args, "args");
            return MakeDynamic(binder, returnType, args);
        }

        private static DynamicExpression MakeDynamic(CallSiteBinder binder, Type returnType, ReadOnlyCollection<Expression> args) {
            ContractUtils.RequiresNotNull(binder, "binder");

            for (int i = 0; i < args.Count; i++) {
                Expression arg = args[i];

                ValidateDynamicArgument(arg);
            }

            Type delegateType = DelegateHelpers.MakeCallSiteDelegate(args, returnType);

            // Since we made a delegate with argument types that exactly match,
            // we can skip delegate and argument validation
            if (returnType == typeof(object)) {
                switch (args.Count) {
                    case 1: return new DynamicExpression1(delegateType, binder, args[0]);
                    case 2: return new DynamicExpression2(delegateType, binder, args[0], args[1]);
                    case 3: return new DynamicExpression3(delegateType, binder, args[0], args[1], args[2]);
                    case 4: return new DynamicExpression4(delegateType, binder, args[0], args[1], args[2], args[3]);
                    default: return new DynamicExpressionN(delegateType, binder, args);
                }
            } else {
                switch (args.Count) {
                    case 1: return new TypedDynamicExpression1(returnType, delegateType, binder, args[0]);
                    case 2: return new TypedDynamicExpression2(returnType, delegateType, binder, args[0], args[1]);
                    case 3: return new TypedDynamicExpression3(returnType, delegateType, binder, args[0], args[1], args[2]);
                    case 4: return new TypedDynamicExpression4(returnType, delegateType, binder, args[0], args[1], args[2], args[3]);
                    default: return new TypedDynamicExpressionN(returnType, delegateType, binder, args);
                }
            }
        }

        private static void ValidateDynamicArgument(Expression arg) {
            RequiresCanRead(arg, "arguments");
            var type = arg.Type;
            ContractUtils.RequiresNotNull(type, "type");
            TypeUtils.ValidateType(type);
            ContractUtils.Requires(type != typeof(void), Strings.ArgumentTypeCannotBeVoid);
        }
    }
}
