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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Dynamic.Utils;
using System.Text;

namespace System.Linq.Expressions {
    //CONFORMING
    public class MethodCallExpression : Expression, IArgumentProvider {
        private readonly MethodInfo _method;

        internal MethodCallExpression(MethodInfo method) {

            _method = method;
        }

        internal static MethodCallExpression Make(
            MethodInfo method,
            Expression instance,
            ReadOnlyCollection<Expression> arguments) {
            if (instance == null) {
                return new MethodCallExpressionN(method, arguments);
            } else {
                return new InstanceMethodCallExpressionN(method, instance, arguments);
            }
        }

        internal virtual Expression GetInstance() {
            return null;
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Call;
        }

        protected override Type GetExpressionType() {
            return _method.ReturnType;
        }

        public MethodInfo Method {
            get { return _method; }
        }

        public Expression Object {
            get { return GetInstance(); }
        }

        public ReadOnlyCollection<Expression> Arguments {
            get { return GetOrMakeArguments(); }
        }

        internal virtual ReadOnlyCollection<Expression> GetOrMakeArguments() {
            throw ContractUtils.Unreachable;
        }

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitMethodCall(this);
        }

        /// <summary>
        /// Returns a new MethodCallExpression replacing the existing instance/args with the
        /// newly provided instance and args.    Arguments can be null to use the existing
        /// arguments.
        /// 
        /// This helper is provided to allow re-writing of nodes to not depend on the specific optimized
        /// subclass of MethodCallExpression which is being used. 
        /// </summary>
        internal virtual MethodCallExpression Rewrite(Expression instance, IList<Expression> args) {
            throw ContractUtils.Unreachable;
        }

        #region IArgumentProvider Members

        Expression IArgumentProvider.GetArgument(int index) {
            throw ContractUtils.Unreachable;
        }

        int IArgumentProvider.ArgumentCount {
            get { throw ContractUtils.Unreachable; }
        }

        #endregion
    }

    #region Specialized Subclasses

    internal class MethodCallExpressionN : MethodCallExpression, IArgumentProvider {
        private IList<Expression> _arguments;

        public MethodCallExpressionN(MethodInfo method, IList<Expression> args)
            : base(method) {
            _arguments = args;
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

        internal override MethodCallExpression Rewrite(Expression instance, IList<Expression> args) {
            Debug.Assert(instance == null);
            Debug.Assert(args == null || args.Count == _arguments.Count);

            return new MethodCallExpressionN(Method, args ?? _arguments);
        }
    }

    internal class InstanceMethodCallExpressionN : MethodCallExpression, IArgumentProvider {
        private IList<Expression> _arguments;
        private readonly Expression _instance;

        public InstanceMethodCallExpressionN(MethodInfo method, Expression instance, IList<Expression> args)
            : base(method) {
            _instance = instance;
            _arguments = args;
        }

        Expression IArgumentProvider.GetArgument(int index) {
            return _arguments[index];
        }

        int IArgumentProvider.ArgumentCount {
            get {
                return _arguments.Count;
            }
        }

        internal override Expression GetInstance() {
            return _instance;
        }

        internal override ReadOnlyCollection<Expression> GetOrMakeArguments() {
            return ReturnReadOnly(ref _arguments);
        }

        internal override MethodCallExpression Rewrite(Expression instance, IList<Expression> args) {
            Debug.Assert(instance != null);
            Debug.Assert(args == null || args.Count == _arguments.Count);

            return new InstanceMethodCallExpressionN(Method, instance, args ?? _arguments);
        }
    }

    internal class MethodCallExpression1 : MethodCallExpression, IArgumentProvider {
        private object _arg0;       // storage for the 1st argument or a ROC.  See IArgumentProvider

        public MethodCallExpression1(MethodInfo method, Expression arg0)
            : base(method) {
            _arg0 = arg0;
        }

        Expression IArgumentProvider.GetArgument(int index) {
            switch (index) {
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

        internal override MethodCallExpression Rewrite(Expression instance, IList<Expression> args) {
            Debug.Assert(instance == null);
            Debug.Assert(args == null || args.Count == 1);

            if (args != null) {
                return new MethodCallExpression1(Method, args[0]);
            }

            return new MethodCallExpression1(Method, ReturnObject<Expression>(_arg0));
        }
    }

    internal class MethodCallExpression2 : MethodCallExpression, IArgumentProvider {
        private object _arg0;               // storage for the 1st argument or a ROC.  See IArgumentProvider
        private readonly Expression _arg1;  // storage for the 2nd arg

        public MethodCallExpression2(MethodInfo method, Expression arg0, Expression arg1)
            : base(method) {
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

        internal override MethodCallExpression Rewrite(Expression instance, IList<Expression> args) {
            Debug.Assert(instance == null);
            Debug.Assert(args == null || args.Count == 2);

            if (args != null) {
                return new MethodCallExpression2(Method, args[0], args[1]);
            }
            return new MethodCallExpression2(Method, ReturnObject<Expression>(_arg0), _arg1);
        }
    }

    internal class MethodCallExpression3 : MethodCallExpression, IArgumentProvider {
        private object _arg0;           // storage for the 1st argument or a ROC.  See IArgumentProvider
        private readonly Expression _arg1, _arg2; // storage for the 2nd - 3rd args.

        public MethodCallExpression3(MethodInfo method, Expression arg0, Expression arg1, Expression arg2)
            : base(method) {
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

        internal override MethodCallExpression Rewrite(Expression instance, IList<Expression> args) {
            Debug.Assert(instance == null);
            Debug.Assert(args == null || args.Count == 3);

            if (args != null) {
                return new MethodCallExpression3(Method, args[0], args[1], args[2]);
            }
            return new MethodCallExpression3(Method, ReturnObject<Expression>(_arg0), _arg1, _arg2);
        }
    }

    internal class MethodCallExpression4 : MethodCallExpression, IArgumentProvider {
        private object _arg0;               // storage for the 1st argument or a ROC.  See IArgumentProvider
        private readonly Expression _arg1, _arg2, _arg3;  // storage for the 2nd - 4th args.

        public MethodCallExpression4(MethodInfo method, Expression arg0, Expression arg1, Expression arg2, Expression arg3)
            : base(method) {
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

        internal override MethodCallExpression Rewrite(Expression instance, IList<Expression> args) {
            Debug.Assert(instance == null);
            Debug.Assert(args == null || args.Count == 4);

            if (args != null) {
                return new MethodCallExpression4(Method, args[0], args[1], args[2], args[3]);
            }
            return new MethodCallExpression4(Method, ReturnObject<Expression>(_arg0), _arg1, _arg2, _arg3);
        }
    }

    internal class MethodCallExpression5 : MethodCallExpression, IArgumentProvider {
        private object _arg0;           // storage for the 1st argument or a ROC.  See IArgumentProvider
        private readonly Expression _arg1, _arg2, _arg3, _arg4;   // storage for the 2nd - 5th args.

        public MethodCallExpression5(MethodInfo method, Expression arg0, Expression arg1, Expression arg2, Expression arg3, Expression arg4)
            : base(method) {
            _arg0 = arg0;
            _arg1 = arg1;
            _arg2 = arg2;
            _arg3 = arg3;
            _arg4 = arg4;
        }

        Expression IArgumentProvider.GetArgument(int index) {
            switch (index) {
                case 0: return ReturnObject<Expression>(_arg0);
                case 1: return _arg1;
                case 2: return _arg2;
                case 3: return _arg3;
                case 4: return _arg4;
                default: throw new InvalidOperationException();
            }
        }

        int IArgumentProvider.ArgumentCount {
            get {
                return 5;
            }
        }

        internal override ReadOnlyCollection<Expression> GetOrMakeArguments() {
            return ReturnReadOnly(this, ref _arg0);
        }

        internal override MethodCallExpression Rewrite(Expression instance, IList<Expression> args) {
            Debug.Assert(instance == null);
            Debug.Assert(args == null || args.Count == 5);

            if (args != null) {
                return new MethodCallExpression5(Method, args[0], args[1], args[2], args[3], args[4]);
            }

            return new MethodCallExpression5(Method, ReturnObject<Expression>(_arg0), _arg1, _arg2, _arg3, _arg4);
        }
    }

    internal class InstanceMethodCallExpression2 : MethodCallExpression, IArgumentProvider {
        private readonly Expression _instance;
        private object _arg0;                // storage for the 1st argument or a ROC.  See IArgumentProvider
        private readonly Expression _arg1;   // storage for the 2nd argument

        public InstanceMethodCallExpression2(MethodInfo method, Expression instance, Expression arg0, Expression arg1)
            : base(method) {
            Debug.Assert(instance != null);

            _instance = instance;
            _arg0 = arg0;
            _arg1 = arg1;
        }

        Expression IArgumentProvider.GetArgument(int index) {
            switch(index) {
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

        internal override Expression GetInstance() {
            return _instance;
        }

        internal override ReadOnlyCollection<Expression> GetOrMakeArguments() {
            return ReturnReadOnly(this, ref _arg0);
        }

        internal override MethodCallExpression Rewrite(Expression instance, IList<Expression> args) {
            Debug.Assert(instance != null);
            Debug.Assert(args.Count == 2);

            if (args != null) {
                return new InstanceMethodCallExpression2(Method, instance, args[0], args[1]);
            }
            return new InstanceMethodCallExpression2(Method, instance, ReturnObject<Expression>(_arg0), _arg1);
        }
    }

    internal class InstanceMethodCallExpression3 : MethodCallExpression, IArgumentProvider {
        private readonly Expression _instance;
        private object _arg0;                       // storage for the 1st argument or a ROC.  See IArgumentProvider
        private readonly Expression _arg1, _arg2;   // storage for the 2nd - 3rd argument

        public InstanceMethodCallExpression3(MethodInfo method, Expression instance, Expression arg0, Expression arg1, Expression arg2)
            : base(method) {
            Debug.Assert(instance != null);

            _instance = instance;
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

        internal override Expression GetInstance() {
            return _instance;
        }

        internal override ReadOnlyCollection<Expression> GetOrMakeArguments() {
            return ReturnReadOnly(this, ref _arg0);
        }

        internal override MethodCallExpression Rewrite(Expression instance, IList<Expression> args) {
            Debug.Assert(instance != null);
            Debug.Assert(args.Count == 3);

            if (args != null) {
                return new InstanceMethodCallExpression3(Method, instance, args[0], args[1], args[2]);
            }
            return new InstanceMethodCallExpression3(Method, instance, ReturnObject<Expression>(_arg0), _arg1, _arg2);
        }
    }

    #endregion

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {

        #region Call

        public static MethodCallExpression Call(MethodInfo method, Expression arg0) {            
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.RequiresNotNull(arg0, "arg0");            

            ParameterInfo[] pis = ValidateMethodAndGetParameters(null, method);

            ValidateArgumentCount(method, ExpressionType.Call, 1, pis);

            arg0 = ValidateOneArgument(method, ExpressionType.Call, arg0, pis[0]);            

            return new MethodCallExpression1(method, arg0); 
        }
        
        public static MethodCallExpression Call(MethodInfo method, Expression arg0, Expression arg1) {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.RequiresNotNull(arg0, "arg0");
            ContractUtils.RequiresNotNull(arg1, "arg1");

            ParameterInfo[] pis = ValidateMethodAndGetParameters(null, method);

            ValidateArgumentCount(method, ExpressionType.Call, 2, pis);

            arg0 = ValidateOneArgument(method, ExpressionType.Call, arg0, pis[0]);
            arg1 = ValidateOneArgument(method, ExpressionType.Call, arg1, pis[1]);

            return new MethodCallExpression2(method, arg0, arg1);
        }

        public static MethodCallExpression Call(MethodInfo method, Expression arg0, Expression arg1, Expression arg2) {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.RequiresNotNull(arg0, "arg0");
            ContractUtils.RequiresNotNull(arg1, "arg1");
            ContractUtils.RequiresNotNull(arg2, "arg2");

            ParameterInfo[] pis = ValidateMethodAndGetParameters(null, method);

            ValidateArgumentCount(method, ExpressionType.Call, 3, pis);

            arg0 = ValidateOneArgument(method, ExpressionType.Call, arg0, pis[0]);
            arg1 = ValidateOneArgument(method, ExpressionType.Call, arg1, pis[1]);
            arg2 = ValidateOneArgument(method, ExpressionType.Call, arg2, pis[2]);

            return new MethodCallExpression3(method, arg0, arg1, arg2);
        }

        public static MethodCallExpression Call(MethodInfo method, Expression arg0, Expression arg1, Expression arg2, Expression arg3) {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.RequiresNotNull(arg0, "arg0");
            ContractUtils.RequiresNotNull(arg1, "arg1");
            ContractUtils.RequiresNotNull(arg2, "arg2");
            ContractUtils.RequiresNotNull(arg3, "arg3");

            ParameterInfo[] pis = ValidateMethodAndGetParameters(null, method);

            ValidateArgumentCount(method, ExpressionType.Call, 4, pis);

            arg0 = ValidateOneArgument(method, ExpressionType.Call, arg0, pis[0]);
            arg1 = ValidateOneArgument(method, ExpressionType.Call, arg1, pis[1]);
            arg2 = ValidateOneArgument(method, ExpressionType.Call, arg2, pis[2]);
            arg3 = ValidateOneArgument(method, ExpressionType.Call, arg3, pis[3]);

            return new MethodCallExpression4(method, arg0, arg1, arg2, arg3);
        }

        public static MethodCallExpression Call(MethodInfo method, Expression arg0, Expression arg1, Expression arg2, Expression arg3, Expression arg4) {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.RequiresNotNull(arg0, "arg0");
            ContractUtils.RequiresNotNull(arg1, "arg1");
            ContractUtils.RequiresNotNull(arg2, "arg2");
            ContractUtils.RequiresNotNull(arg3, "arg3");
            ContractUtils.RequiresNotNull(arg4, "arg4");

            ParameterInfo[] pis = ValidateMethodAndGetParameters(null, method);

            ValidateArgumentCount(method, ExpressionType.Call, 5, pis);

            arg0 = ValidateOneArgument(method, ExpressionType.Call, arg0, pis[0]);
            arg1 = ValidateOneArgument(method, ExpressionType.Call, arg1, pis[1]);
            arg2 = ValidateOneArgument(method, ExpressionType.Call, arg2, pis[2]);
            arg3 = ValidateOneArgument(method, ExpressionType.Call, arg3, pis[3]);
            arg4 = ValidateOneArgument(method, ExpressionType.Call, arg4, pis[4]);

            return new MethodCallExpression5(method, arg0, arg1, arg2, arg3, arg4);
        }

        //CONFORMING
        public static MethodCallExpression Call(MethodInfo method, params Expression[] arguments) {
            return Call(null, method, arguments);
        }

        public static MethodCallExpression Call(MethodInfo method, IEnumerable<Expression> arguments) {
            return Call(null, method, arguments);
        }

        //CONFORMING
        public static MethodCallExpression Call(Expression instance, MethodInfo method) {
            return Call(instance, method, EmptyReadOnlyCollection<Expression>.Instance);
        }

        //CONFORMING
        public static MethodCallExpression Call(Expression instance, MethodInfo method, params Expression[] arguments) {
            return Call(instance, method, (IEnumerable<Expression>)arguments);
        }

        public static MethodCallExpression Call(Expression instance, MethodInfo method, Expression arg0, Expression arg1) {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.RequiresNotNull(arg0, "arg0");
            ContractUtils.RequiresNotNull(arg1, "arg1");

            ParameterInfo[] pis = ValidateMethodAndGetParameters(instance, method);

            ValidateArgumentCount(method, ExpressionType.Call, 2, pis);

            arg0 = ValidateOneArgument(method, ExpressionType.Call, arg0, pis[0]);
            arg1 = ValidateOneArgument(method, ExpressionType.Call, arg1, pis[1]);

            if (instance != null) {
                return new InstanceMethodCallExpression2(method, instance, arg0, arg1);
            }

            return new MethodCallExpression2(method, arg0, arg1);
        }

        public static MethodCallExpression Call(Expression instance, MethodInfo method, Expression arg0, Expression arg1, Expression arg2) {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.RequiresNotNull(arg0, "arg0");
            ContractUtils.RequiresNotNull(arg1, "arg1");
            ContractUtils.RequiresNotNull(arg2, "arg2");

            ParameterInfo[] pis = ValidateMethodAndGetParameters(instance, method);

            ValidateArgumentCount(method, ExpressionType.Call, 3, pis);

            arg0 = ValidateOneArgument(method, ExpressionType.Call, arg0, pis[0]);
            arg1 = ValidateOneArgument(method, ExpressionType.Call, arg1, pis[1]);
            arg2 = ValidateOneArgument(method, ExpressionType.Call, arg2, pis[2]);

            if (instance != null) {
                return new InstanceMethodCallExpression3(method, instance, arg0, arg1, arg2);
            }
            return new MethodCallExpression3(method, arg0, arg1, arg2);
        }

        //CONFORMING
        public static MethodCallExpression Call(Expression instance, string methodName, Type[] typeArguments, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(instance, "instance");
            ContractUtils.RequiresNotNull(methodName, "methodName");
            if (arguments == null) arguments = new Expression[] { };

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            return Expression.Call(instance, FindMethod(instance.Type, methodName, typeArguments, arguments, flags), arguments);
        }

        //CONFORMING
        public static MethodCallExpression Call(Type type, string methodName, Type[] typeArguments, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(methodName, "methodName");

            if (arguments == null) arguments = new Expression[] { };
            BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            return Expression.Call(null, FindMethod(type, methodName, typeArguments, arguments, flags), arguments);
        }

        //CONFORMING
        public static MethodCallExpression Call(Expression instance, MethodInfo method, IEnumerable<Expression> arguments) {
            ContractUtils.RequiresNotNull(method, "method");

            ReadOnlyCollection<Expression> argList = arguments.ToReadOnly();

            ValidateMethodInfo(method);
            ValidateStaticOrInstanceMethod(instance, method);
            ValidateArgumentTypes(method, ExpressionType.Call, ref argList);

            if (instance == null) {
                return new MethodCallExpressionN(method, argList);
            } else {
                return new InstanceMethodCallExpressionN(method, instance, argList);
            }
        }

        private static ParameterInfo[] ValidateMethodAndGetParameters(Expression instance, MethodInfo method) {
            ValidateMethodInfo(method);
            ValidateStaticOrInstanceMethod(instance, method);

            return GetParametersForValidation(method, ExpressionType.Call);
        }

        private static void ValidateStaticOrInstanceMethod(Expression instance, MethodInfo method) {
            if (method.IsStatic) {
                ContractUtils.Requires(instance == null, "instance", Strings.OnlyStaticMethodsHaveNullInstance);
            } else {
                ContractUtils.Requires(instance != null, "method", Strings.OnlyStaticMethodsHaveNullInstance);
                RequiresCanRead(instance, "instance");
                ValidateCallInstanceType(instance.Type, method);
            }
        }

        //CONFORMING
        private static void ValidateCallInstanceType(Type instanceType, MethodInfo method) {
            if (!TypeUtils.IsValidInstanceType(method, instanceType)) {
                throw Error.MethodNotDefinedForType(method, instanceType);
            }
        }

        //CONFORMING
        private static void ValidateArgumentTypes(MethodBase method, ExpressionType nodeKind, ref ReadOnlyCollection<Expression> arguments) {
            Debug.Assert(nodeKind == ExpressionType.Invoke || nodeKind == ExpressionType.Call || nodeKind == ExpressionType.Dynamic || nodeKind == ExpressionType.New);

            ParameterInfo[] pis = GetParametersForValidation(method, nodeKind);

            ValidateArgumentCount(method, nodeKind, arguments.Count, pis);

            Expression[] newArgs = null;
            for (int i = 0, n = pis.Length; i < n; i++) {
                Expression arg = arguments[i];
                ParameterInfo pi = pis[i];
                arg = ValidateOneArgument(method, nodeKind, arg, pi);

                if (newArgs == null && arg != arguments[i]) {
                    newArgs = new Expression[arguments.Count];
                    for (int j = 0; j < i; j++) {
                        newArgs[j] = arguments[j];
                    }
                }
                if (newArgs != null) {
                    newArgs[i] = arg;
                }
            }
            if (newArgs != null) {
                arguments = new ReadOnlyCollection<Expression>(newArgs);
            }
        }

        private static ParameterInfo[] GetParametersForValidation(MethodBase method, ExpressionType nodeKind) {
            ParameterInfo[] pis = method.GetParametersCached();

            if (nodeKind == ExpressionType.Dynamic) {
                pis = pis.RemoveFirst(); // ignore CallSite argument
            }
            return pis;
        }

        private static void ValidateArgumentCount(MethodBase method, ExpressionType nodeKind, int count, ParameterInfo[] pis) {
            if (pis.Length != count) {
                // Throw the right error for the node we were given
                switch (nodeKind) {
                    case ExpressionType.New:
                        throw Error.IncorrectNumberOfConstructorArguments();
                    case ExpressionType.Invoke:
                        throw Error.IncorrectNumberOfLambdaArguments();
                    case ExpressionType.Dynamic:
                    case ExpressionType.Call:
                        throw Error.IncorrectNumberOfMethodCallArguments(method);
                    default:
                        throw ContractUtils.Unreachable;
                }
            }
        }

        private static Expression ValidateOneArgument(MethodBase method, ExpressionType nodeKind, Expression arg, ParameterInfo pi) {
            RequiresCanRead(arg, "arguments");
            Type pType = pi.ParameterType;
            if (pType.IsByRef) {
                pType = pType.GetElementType();
            }
            TypeUtils.ValidateType(pType);
            if (!TypeUtils.AreReferenceAssignable(pType, arg.Type)) {
                if (TypeUtils.IsSameOrSubclass(typeof(Expression), pType) && pType.IsAssignableFrom(arg.GetType())) {
                    arg = Expression.Quote(arg);
                } else {
                    // Throw the right error for the node we were given
                    switch (nodeKind) {
                        case ExpressionType.New:
                            throw Error.ExpressionTypeDoesNotMatchConstructorParameter(arg.Type, pType);
                        case ExpressionType.Invoke:
                            throw Error.ExpressionTypeDoesNotMatchParameter(arg.Type, pType);
                        case ExpressionType.Dynamic:
                        case ExpressionType.Call:
                            throw Error.ExpressionTypeDoesNotMatchMethodParameter(arg.Type, pType, method);
                        default:
                            throw ContractUtils.Unreachable;
                    }
                }
            }
            return arg;
        }

        //CONFORMING
        private static MethodInfo FindMethod(Type type, string methodName, Type[] typeArgs, Expression[] args, BindingFlags flags) {
            MemberInfo[] members = type.FindMembers(MemberTypes.Method, flags, Type.FilterNameIgnoreCase, methodName);
            if (members == null || members.Length == 0)
                throw Error.MethodDoesNotExistOnType(methodName, type);

            MethodInfo method;

            var methodInfos = members.Map(t => (MethodInfo)t);
            int count = FindBestMethod(methodInfos, typeArgs, args, out method);

            if (count == 0)
                throw Error.MethodWithArgsDoesNotExistOnType(methodName, type);
            if (count > 1)
                throw Error.MethodWithMoreThanOneMatch(methodName, type);
            return method;
        }

        //CONFORMING
        private static int FindBestMethod(IEnumerable<MethodInfo> methods, Type[] typeArgs, Expression[] args, out MethodInfo method) {
            int count = 0;
            method = null;
            foreach (MethodInfo mi in methods) {
                MethodInfo moo = ApplyTypeArgs(mi, typeArgs);
                if (moo != null && IsCompatible(moo, args)) {
                    // favor public over non-public methods
                    if (method == null || (!method.IsPublic && moo.IsPublic)) {
                        method = moo;
                        count = 1;
                    }
                        // only count it as additional method if they both public or both non-public
                    else if (method.IsPublic == moo.IsPublic) {
                        count++;
                    }
                }
            }
            return count;
        }

        //CONFORMING
        private static bool IsCompatible(MethodBase m, Expression[] args) {
            ParameterInfo[] parms = m.GetParametersCached();
            if (parms.Length != args.Length)
                return false;
            for (int i = 0; i < args.Length; i++) {
                Expression arg = args[i];
                ContractUtils.RequiresNotNull(arg, "argument");
                Type argType = arg.Type;
                Type pType = parms[i].ParameterType;
                if (pType.IsByRef) {
                    pType = pType.GetElementType();
                }
                if (!TypeUtils.AreReferenceAssignable(pType, argType) &&
                    !(TypeUtils.IsSameOrSubclass(typeof(Expression), pType) && pType.IsAssignableFrom(arg.GetType()))) {
                    return false;
                }
            }
            return true;
        }

        //CONFORMING
        private static MethodInfo ApplyTypeArgs(MethodInfo m, Type[] typeArgs) {
            if (typeArgs == null || typeArgs.Length == 0) {
                if (!m.IsGenericMethodDefinition)
                    return m;
            } else {
                if (m.IsGenericMethodDefinition && m.GetGenericArguments().Length == typeArgs.Length)
                    return m.MakeGenericMethod(typeArgs);
            }
            return null;
        }


        #endregion

        #region ArrayIndex

        //CONFORMING
        public static MethodCallExpression ArrayIndex(Expression array, params Expression[] indexes) {
            return ArrayIndex(array, (IEnumerable<Expression>)indexes);
        }

        //CONFORMING
        public static MethodCallExpression ArrayIndex(Expression array, IEnumerable<Expression> indexes) {
            RequiresCanRead(array, "array");
            ContractUtils.RequiresNotNull(indexes, "indexes");

            Type arrayType = array.Type;
            if (!arrayType.IsArray)
                throw Error.ArgumentMustBeArray();

            ReadOnlyCollection<Expression> indexList = indexes.ToReadOnly();
            if (arrayType.GetArrayRank() != indexList.Count)
                throw Error.IncorrectNumberOfIndexes();

            foreach (Expression e in indexList) {
                RequiresCanRead(e, "indexes");
                if (e.Type != typeof(int)) {
                    throw Error.ArgumentMustBeArrayIndexType();
                }
            }

            MethodInfo mi = array.Type.GetMethod("Get", BindingFlags.Public | BindingFlags.Instance);
            return Call(array, mi, indexList);
        }

        #endregion

    }
}
