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
using System.Reflection;
using System.Reflection.Emit;
using System.Dynamic.Utils;
using System.Text;
using System.Threading;

namespace System.Linq.Expressions {
    //CONFORMING
    /// <summary>
    /// This captures a block of code that should correspond to a .NET method
    /// body. It takes input through parameters and is expected to be fully
    /// bound. This code can then be generated in a variety of ways. The
    /// variables can be kept as .NET locals or hoisted into an object bound to
    /// the delegate. This is the primary unit used for passing around
    /// Expression Trees in LINQ and the DLR.
    /// </summary>
    public abstract class LambdaExpression : Expression {
        private readonly string _name;
        private readonly Expression _body;
        private readonly ReadOnlyCollection<ParameterExpression> _parameters;
        private readonly Type _delegateType;

        internal LambdaExpression(
            Type delegateType,
            string name,
            Expression body,
            ReadOnlyCollection<ParameterExpression> parameters
        ) {

            Debug.Assert(delegateType != null);

            _name = name;
            _body = body;
            _parameters = parameters;
            _delegateType = delegateType;
        }

        protected override Type GetExpressionType() {
            return _delegateType;
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Lambda;
        }

        public ReadOnlyCollection<ParameterExpression> Parameters {
            get { return _parameters; }
        }

        public string Name {
            get { return _name; }
        }

        public Expression Body {
            get { return _body; }
        }

        public Type ReturnType {
            get { return Type.GetMethod("Invoke").ReturnType; }
        }

        public Delegate Compile() {
            return LambdaCompiler.CompileLambda(this, false);
        }

        public Delegate Compile(bool emitDebugSymbols) {
            return LambdaCompiler.CompileLambda(this, emitDebugSymbols);
        }

        public void CompileToMethod(MethodBuilder method, bool emitDebugSymbols) {
            ContractUtils.RequiresNotNull(method, "method");
            var type = method.DeclaringType as TypeBuilder;
            ContractUtils.Requires(type != null, "method", Strings.MethodBuilderDoesNotHaveTypeBuilder);
            if (emitDebugSymbols) {
                var module = method.Module as ModuleBuilder;
                ContractUtils.Requires(module != null, "method", Strings.MethodBuilderDoesNotHaveModuleBuilder);
            }
            LambdaCompiler.CompileLambda(this, method, emitDebugSymbols);
        }

        internal abstract LambdaExpression Accept(StackSpiller spiller);
    }

    //CONFORMING
    public sealed class Expression<TDelegate> : LambdaExpression {
        internal Expression(
            string name,
            Expression body,
            ReadOnlyCollection<ParameterExpression> parameters
        )
            : base(typeof(TDelegate), name, body, parameters) {
        }

        public new TDelegate Compile() {
            return LambdaCompiler.CompileLambda<TDelegate>(this, false);
        }

        public new TDelegate Compile(bool emitDebugSymbols) {
            return LambdaCompiler.CompileLambda<TDelegate>(this, emitDebugSymbols);
        }

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitLambda(this);
        }

        internal override LambdaExpression Accept(StackSpiller spiller) {
            return spiller.Rewrite(this);
        }
    }


    public partial class Expression {
        //internal lambda factory that creates an instance of Expression<delegateType>
        internal static LambdaExpression Lambda(
                ExpressionType nodeType,
                Type delegateType,
                string name,
                Expression body,
                ReadOnlyCollection<ParameterExpression> parameters
        ) {
            if (nodeType == ExpressionType.Lambda) {
                // got or create a delegate to the public Expression.Lambda<T> method and call that will be used for
                // creating instances of this delegate type
                Func<Expression, string, IEnumerable<ParameterExpression>, LambdaExpression> func;

                if (_exprCtors == null) {
                    EnsureLambdaFastPathInitialized();
                }

                lock (_exprCtors) {
                    if (!_exprCtors.TryGetValue(delegateType, out func)) {
                        _exprCtors[delegateType] = func = (Func<Expression, string, IEnumerable<ParameterExpression>, LambdaExpression>)
                            Delegate.CreateDelegate(
                                typeof(Func<Expression, string, IEnumerable<ParameterExpression>, LambdaExpression>),
                                _lambdaCtorMethod.MakeGenericMethod(delegateType)
                            );
                    }
                }

                return func(body, name, parameters);
            }

            return SlowMakeLambda(nodeType, delegateType, name, body, parameters);
        }

        private static void EnsureLambdaFastPathInitialized() {
            Interlocked.CompareExchange(
                ref _exprCtors,
                new CacheDict<Type, Func<Expression, string, IEnumerable<ParameterExpression>, LambdaExpression>>(200),
                null
            );

            EnsureLambdaCtor();
        }
        
        private static void EnsureLambdaCtor() {
            MethodInfo[] methods = (MethodInfo[])typeof(Expression).GetMember("Lambda", MemberTypes.Method, BindingFlags.Public | BindingFlags.Static);
            foreach (MethodInfo mi in methods) {
                if (!mi.IsGenericMethod) {
                    continue;
                }

                ParameterInfo[] pis = mi.GetParameters();
                if (pis.Length == 3) {
                    if (pis[0].ParameterType == typeof(Expression) &&
                        pis[1].ParameterType == typeof(string) &&
                        pis[2].ParameterType == typeof(IEnumerable<ParameterExpression>)) {
                        _lambdaCtorMethod = mi;
                        break;
                    }
                }
            }
            Debug.Assert(_lambdaCtorMethod != null);
        }

        private static LambdaExpression SlowMakeLambda(ExpressionType nodeType, Type delegateType, string name, Expression body, ReadOnlyCollection<ParameterExpression> parameters) {
            Type ot = typeof(Expression<>);
            Type ct = ot.MakeGenericType(new Type[] { delegateType });
            Type[] ctorTypes = new Type[] {
                typeof(ExpressionType),     // nodeType,
                typeof(string),             // name,
                typeof(Expression),         // body,
                typeof(ReadOnlyCollection<ParameterExpression>) // parameters) 
            }; 
            ConstructorInfo ctor = ct.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, ctorTypes, null);
            return (LambdaExpression)ctor.Invoke(new object[] { nodeType, name, body, parameters });
        }

        //CONFORMING
        public static Expression<TDelegate> Lambda<TDelegate>(Expression body, params ParameterExpression[] parameters) {
            return Lambda<TDelegate>(body, (IEnumerable<ParameterExpression>)parameters);
        }

        //CONFORMING
        public static Expression<TDelegate> Lambda<TDelegate>(Expression body, IEnumerable<ParameterExpression> parameters) {
            return Lambda<TDelegate>(body, "lambda_method", parameters);
        }

        //CONFORMING
        public static Expression<TDelegate> Lambda<TDelegate>(Expression body, String name, IEnumerable<ParameterExpression> parameters) {
            ReadOnlyCollection<ParameterExpression> parameterList = parameters.ToReadOnly();
            ValidateLambdaArgs(typeof(TDelegate), ref body, parameterList);
            return new Expression<TDelegate>(name, body, parameterList);
        }


        public static LambdaExpression Lambda(Expression body, params ParameterExpression[] parameters) {
            return Lambda(body, (IEnumerable<ParameterExpression>)parameters);
        }

        public static LambdaExpression Lambda(Expression body, IEnumerable<ParameterExpression> parameters) {
            return Lambda(body, "lambda_method", parameters);
        }

        //CONFORMING
        public static LambdaExpression Lambda(Type delegateType, Expression body, params ParameterExpression[] parameters) {
            return Lambda(delegateType, body, "lambda_method", parameters);
        }

        //CONFORMING
        public static LambdaExpression Lambda(Type delegateType, Expression body, IEnumerable<ParameterExpression> parameters) {
            return Lambda(delegateType, body, "lambda_method", parameters);
        }

        //CONFORMING
        public static LambdaExpression Lambda(Expression body, string name, IEnumerable<ParameterExpression> parameters) {
            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.RequiresNotNull(body, "body");

            ReadOnlyCollection<ParameterExpression> parameterList = parameters.ToReadOnly();

            bool binder = body.Type == typeof(void);

            int paramCount = parameterList.Count;
            Type[] typeArgs = new Type[paramCount + (binder ? 0 : 1)];
            for (int i = 0; i < paramCount; i++) {
                ContractUtils.RequiresNotNull(parameterList[i], "parameter");
                typeArgs[i] = parameterList[i].Type;
            }

            Type delegateType;
            if (binder)
                delegateType = GetActionType(typeArgs);
            else {
                typeArgs[paramCount] = body.Type;
                delegateType = GetFuncType(typeArgs);
            }

            return Lambda(ExpressionType.Lambda, delegateType, name, body, parameterList);
        }

        //CONFORMING
        public static LambdaExpression Lambda(Type delegateType, Expression body, string name, IEnumerable<ParameterExpression> parameters) {
            ReadOnlyCollection<ParameterExpression> paramList = parameters.ToReadOnly();
            ValidateLambdaArgs(delegateType, ref body, paramList);

            return Lambda(ExpressionType.Lambda, delegateType, name, body, paramList);
        }

        //CONFORMING
        private static void ValidateLambdaArgs(Type delegateType, ref Expression body, ReadOnlyCollection<ParameterExpression> parameters) {
            ContractUtils.RequiresNotNull(delegateType, "delegateType");
            RequiresCanRead(body, "body");

            if (!typeof(Delegate).IsAssignableFrom(delegateType) || delegateType == typeof(Delegate)) {
                throw Error.LambdaTypeMustBeDerivedFromSystemDelegate();
            }

            MethodInfo mi;
            lock (_LambdaDelegateCache) {
                if (!_LambdaDelegateCache.TryGetValue(delegateType, out mi)) {
                    _LambdaDelegateCache[delegateType] = mi = delegateType.GetMethod("Invoke");
                }
            }

            ParameterInfo[] pis = mi.GetParametersCached();

            if (pis.Length > 0) {
                if (pis.Length != parameters.Count) {
                    throw Error.IncorrectNumberOfLambdaDeclarationParameters();
                }
                for (int i = 0, n = pis.Length; i < n; i++) {
                    ParameterExpression pex = parameters[i];
                    ParameterInfo pi = pis[i];
                    RequiresCanRead(pex, "parameters");
                    Type pType = pi.ParameterType;
                    if (pex.IsByRef) {
                        if (!pType.IsByRef) {
                            //We cannot pass a parameter of T& to a delegate that takes T or any non-ByRef type.
                            throw Error.ParameterExpressionNotValidAsDelegate(pex.Type.MakeByRefType(), pType);
                        }
                        pType = pType.GetElementType();
                    }
                    if (!TypeUtils.AreReferenceAssignable(pex.Type, pType)) {
                        throw Error.ParameterExpressionNotValidAsDelegate(pex.Type, pType);
                    }
                }
            } else if (parameters.Count > 0) {
                throw Error.IncorrectNumberOfLambdaDeclarationParameters();
            }
            if (mi.ReturnType != typeof(void) && !TypeUtils.AreReferenceAssignable(mi.ReturnType, body.Type)) {
                if (TypeUtils.IsSameOrSubclass(typeof(Expression), mi.ReturnType) && mi.ReturnType.IsAssignableFrom(body.GetType())) {
                    body = Expression.Quote(body);
                } else {
                    throw Error.ExpressionTypeDoesNotMatchReturn(body.Type, mi.ReturnType);
                }
            }
        }

        //CONFORMING
        public static Type GetFuncType(params Type[] typeArgs) {
            ContractUtils.RequiresNotNull(typeArgs, "typeArgs");
            Type result = DelegateHelpers.GetFuncType(typeArgs);
            if (result == null) {
                throw Error.IncorrectNumberOfTypeArgsForFunc();
            }
            return result;
        }

        //CONFORMING
        public static Type GetActionType(params Type[] typeArgs) {
            ContractUtils.RequiresNotNull(typeArgs, "typeArgs");
            Type result = DelegateHelpers.GetActionType(typeArgs);
            if (result == null) {
                throw Error.IncorrectNumberOfTypeArgsForAction();
            }
            return result;
        }

        /// <summary>
        /// Gets a Func or Action corresponding to the given type arguments. If
        /// no Func or Action is large enough, it will generate a custom
        /// delegate type.
        /// 
        /// As with Func, the last argument is the return type. It can be set
        /// to System.Void to produce a an Action.
        /// </summary>
        /// <param name="typeArgs">The type arguments of the delegate.</param>
        /// <returns>The delegate type.</returns>
        public static Type GetDelegateType(params Type[] typeArgs) {
            ContractUtils.RequiresNotEmpty(typeArgs, "typeArgs");
            return DelegateHelpers.MakeDelegateType(typeArgs);
        }
    }
}
