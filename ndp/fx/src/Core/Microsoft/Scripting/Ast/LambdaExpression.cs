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
using System.Dynamic.Utils;
using System.Linq.Expressions.Compiler;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace System.Linq.Expressions {
    /// <summary>
    /// Creates a <see cref="LambdaExpression"/> node.
    /// This captures a block of code that is similar to a .NET method body.
    /// </summary>
    /// <remarks>
    /// Lambda expressions take input through parameters and are expected to be fully bound. 
    /// </remarks>
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

        /// <summary>
        /// Gets the static type of the expression that this <see cref="Expression" /> represents. (Inherited from <see cref="Expression"/>.)
        /// </summary>
        /// <returns>The <see cref="Type"/> that represents the static type of the expression.</returns>
        protected override Type GetExpressionType() {
            return _delegateType;
        }

        /// <summary>
        /// Returns the node type of this <see cref="Expression" />. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="ExpressionType"/> that represents this expression.</returns>
        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Lambda;
        }

        /// <summary>
        /// Gets the parameters of the lambda expression. 
        /// </summary>
        public ReadOnlyCollection<ParameterExpression> Parameters {
            get { return _parameters; }
        }

        /// <summary>
        /// Gets the name of the lambda expression. 
        /// </summary>
        /// <remarks>Used for debugging purposes.</remarks>
        public string Name {
            get { return _name; }
        }

        /// <summary>
        /// Gets the body of the lambda expression. 
        /// </summary>
        public Expression Body {
            get { return _body; }
        }

        /// <summary>
        /// Gets the return type of the lambda expression. 
        /// </summary>
        public Type ReturnType {
            get { return Type.GetMethod("Invoke").ReturnType; }
        }

        /// <summary>
        /// Produces a delegate that represents the lambda expression.
        /// </summary>
        /// <returns>A delegate containing the compiled version of the lambda.</returns>
        public Delegate Compile() {
            return LambdaCompiler.Compile(this);
        }

        /// <summary>
        /// Compiles the lambda into a method definition.
        /// </summary>
        /// <param name="method">A <see cref="MethodBuilder"/> which will be used to hold the lambda's IL.</param>
        /// <param name="emitDebugSymbols">A parameter that indicates if debugging information should be emitted.</param>
        public void CompileToMethod(MethodBuilder method, bool emitDebugSymbols) {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.Requires(method.IsStatic, "method");

            var type = method.DeclaringType as TypeBuilder;
            ContractUtils.Requires(type != null, "method", Strings.MethodBuilderDoesNotHaveTypeBuilder);
            
            if (emitDebugSymbols) {
                var module = method.Module as ModuleBuilder;
                ContractUtils.Requires(module != null, "method", Strings.MethodBuilderDoesNotHaveModuleBuilder);
            }

            LambdaCompiler.Compile(this, method, emitDebugSymbols);
        }

        internal abstract LambdaExpression Accept(StackSpiller spiller);
    }

    /// <summary>
    /// Defines a <see cref="Expression{TDelegate}"/> node.
    /// This captures a block of code that is similar to a .NET method body.
    /// </summary>
    /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
    /// <remarks>
    /// Lambda expressions take input through parameters and are expected to be fully bound. 
    /// </remarks>
    public sealed class Expression<TDelegate> : LambdaExpression {
        internal Expression(
            string name,
            Expression body,
            ReadOnlyCollection<ParameterExpression> parameters
        )
            : base(typeof(TDelegate), name, body, parameters) {
        }

        /// <summary>
        /// Produces a delegate that represents the lambda expression.
        /// </summary>
        /// <returns>A delegate containing the compiled version of the lambda.</returns>
        public new TDelegate Compile() {
            return (TDelegate)(object)LambdaCompiler.Compile(this);
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

        /// <summary>
        /// Creates an <see cref="Expression{TDelegate}"/> where the delegate type is known at compile time. 
        /// </summary>
        /// <typeparam name="TDelegate">The delegate type. </typeparam>
        /// <param name="body">An <see cref="Expression"/> to set the <see cref="P:Body"/> property equal to. </param>
        /// <param name="parameters">An array that contains <see cref="ParameterExpression"/> objects to use to populate the <see cref="P:Parameters"/> collection. </param>
        /// <returns>An <see cref="Expression{TDelegate}"/> that has the <see cref="P:NodeType"/> property equal to <see cref="P:Lambda"/> and the <see cref="P:Body"/> and <see cref="P:Parameters"/> properties set to the specified values.</returns>
        public static Expression<TDelegate> Lambda<TDelegate>(Expression body, params ParameterExpression[] parameters) {
            return Lambda<TDelegate>(body, (IEnumerable<ParameterExpression>)parameters);
        }

        /// <summary>
        /// Creates an <see cref="Expression{TDelegate}"/> where the delegate type is known at compile time. 
        /// </summary>
        /// <typeparam name="TDelegate">The delegate type. </typeparam>
        /// <param name="body">An <see cref="Expression"/> to set the <see cref="P:Body"/> property equal to. </param>
        /// <param name="parameters">An <see cref="IEnumerable{T}"/> that contains <see cref="ParameterExpression"/> objects to use to populate the <see cref="P:Parameters"/> collection. </param>
        /// <returns>An <see cref="Expression{TDelegate}"/> that has the <see cref="P:NodeType"/> property equal to <see cref="P:Lambda"/> and the <see cref="P:Body"/> and <see cref="P:Parameters"/> properties set to the specified values.</returns>
        public static Expression<TDelegate> Lambda<TDelegate>(Expression body, IEnumerable<ParameterExpression> parameters) {
            return Lambda<TDelegate>(body, "lambda_method", parameters);
        }

        /// <summary>
        /// Creates an <see cref="Expression{TDelegate}"/> where the delegate type is known at compile time. 
        /// </summary>
        /// <typeparam name="TDelegate">The delegate type. </typeparam>
        /// <param name="body">An <see cref="Expression"/> to set the <see cref="P:Body"/> property equal to. </param>
        /// <param name="parameters">An <see cref="IEnumerable{T}"/> that contains <see cref="ParameterExpression"/> objects to use to populate the <see cref="P:Parameters"/> collection. </param>
        /// <param name="name">The name of the lambda. Used for generating debugging info.</param>
        /// <returns>An <see cref="Expression{TDelegate}"/> that has the <see cref="P:NodeType"/> property equal to <see cref="P:Lambda"/> and the <see cref="P:Body"/> and <see cref="P:Parameters"/> properties set to the specified values.</returns>
        public static Expression<TDelegate> Lambda<TDelegate>(Expression body, String name, IEnumerable<ParameterExpression> parameters) {
            var parameterList = parameters.ToReadOnly();
            ValidateLambdaArgs(typeof(TDelegate), ref body, parameterList);
            return new Expression<TDelegate>(name, body, parameterList);
        }

        /// <summary>
        /// Creates a LambdaExpression by first constructing a delegate type. 
        /// </summary>
        /// <param name="body">An <see cref="Expression"/> to set the <see cref="P:Body"/> property equal to. </param>
        /// <param name="parameters">An array that contains <see cref="ParameterExpression"/> objects to use to populate the <see cref="P:Parameters"/> collection. </param>
        /// <returns>A <see cref="LambdaExpression"/> that has the <see cref="P:NodeType"/> property equal to Lambda and the <see cref="P:Body"/> and <see cref="P:Parameters"/> properties set to the specified values.</returns>
        public static LambdaExpression Lambda(Expression body, params ParameterExpression[] parameters) {
            return Lambda(body, (IEnumerable<ParameterExpression>)parameters);
        }

        /// <summary>
        /// Creates a LambdaExpression by first constructing a delegate type. 
        /// </summary>
        /// <param name="body">An <see cref="Expression"/> to set the <see cref="P:Body"/> property equal to. </param>
        /// <param name="parameters">An <see cref="IEnumerable{T}"/> that contains <see cref="ParameterExpression"/> objects to use to populate the <see cref="P:Parameters"/> collection. </param>
        /// <returns>A <see cref="LambdaExpression"/> that has the <see cref="P:NodeType"/> property equal to Lambda and the <see cref="P:Body"/> and <see cref="P:Parameters"/> properties set to the specified values.</returns>
        public static LambdaExpression Lambda(Expression body, IEnumerable<ParameterExpression> parameters) {
            return Lambda(body, "lambda_method", parameters);
        }

        /// <summary>
        /// Creates a LambdaExpression by first constructing a delegate type. 
        /// </summary>
        /// <param name="body">An <see cref="Expression"/> to set the <see cref="P:Body"/> property equal to. </param>
        /// <param name="parameters">An array that contains <see cref="ParameterExpression"/> objects to use to populate the <see cref="P:Parameters"/> collection. </param>
        /// <param name="delegateType">A <see cref="Type"/> representing the delegate signature for the lambda.</param>
        /// <returns>A <see cref="LambdaExpression"/> that has the <see cref="P:NodeType"/> property equal to Lambda and the <see cref="P:Body"/> and <see cref="P:Parameters"/> properties set to the specified values.</returns>
        public static LambdaExpression Lambda(Type delegateType, Expression body, params ParameterExpression[] parameters) {
            return Lambda(delegateType, body, "lambda_method", parameters);
        }

        /// <summary>
        /// Creates a LambdaExpression by first constructing a delegate type. 
        /// </summary>
        /// <param name="body">An <see cref="Expression"/> to set the <see cref="P:Body"/> property equal to. </param>
        /// <param name="parameters">An <see cref="IEnumerable{T}"/> that contains <see cref="ParameterExpression"/> objects to use to populate the <see cref="P:Parameters"/> collection. </param>
        /// <param name="delegateType">A <see cref="Type"/> representing the delegate signature for the lambda.</param>
        /// <returns>A <see cref="LambdaExpression"/> that has the <see cref="P:NodeType"/> property equal to Lambda and the <see cref="P:Body"/> and <see cref="P:Parameters"/> properties set to the specified values.</returns>
        public static LambdaExpression Lambda(Type delegateType, Expression body, IEnumerable<ParameterExpression> parameters) {
            return Lambda(delegateType, body, "lambda_method", parameters);
        }

        /// <summary>
        /// Creates a LambdaExpression by first constructing a delegate type. 
        /// </summary>
        /// <param name="body">An <see cref="Expression"/> to set the <see cref="P:Body"/> property equal to. </param>
        /// <param name="parameters">An <see cref="IEnumerable{T}"/> that contains <see cref="ParameterExpression"/> objects to use to populate the <see cref="P:Parameters"/> collection. </param>
        /// <param name="name">The name for the lambda. Used for emitting debug information.</param>
        /// <returns>A <see cref="LambdaExpression"/> that has the <see cref="P:NodeType"/> property equal to Lambda and the <see cref="P:Body"/> and <see cref="P:Parameters"/> properties set to the specified values.</returns>
        public static LambdaExpression Lambda(Expression body, string name, IEnumerable<ParameterExpression> parameters) {
            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.RequiresNotNull(body, "body");

            var parameterList = parameters.ToReadOnly();

            int paramCount = parameterList.Count;
            Type[] typeArgs = new Type[paramCount + 1];
            for (int i = 0; i < paramCount; i++) {
                ContractUtils.RequiresNotNull(parameterList[i], "parameter");
                Type pType = parameterList[i].Type;
                typeArgs[i] = parameterList[i].IsByRef ? pType.MakeByRefType() : pType;
            }
            typeArgs[paramCount] = body.Type;

            Type delegateType = DelegateHelpers.MakeDelegateType(typeArgs);

            return Lambda(ExpressionType.Lambda, delegateType, name, body, parameterList);
        }

        /// <summary>
        /// Creates a LambdaExpression by first constructing a delegate type. 
        /// </summary>
        /// <param name="body">An <see cref="Expression"/> to set the <see cref="P:Body"/> property equal to. </param>
        /// <param name="parameters">An <see cref="IEnumerable{T}"/> that contains <see cref="ParameterExpression"/> objects to use to populate the <see cref="P:Parameters"/> collection. </param>
        /// <param name="name">The name for the lambda. Used for emitting debug information.</param>
        /// <param name="delegateType">A <see cref="Type"/> representing the delegate signature for the lambda.</param>
        /// <returns>A <see cref="LambdaExpression"/> that has the <see cref="P:NodeType"/> property equal to Lambda and the <see cref="P:Body"/> and <see cref="P:Parameters"/> properties set to the specified values.</returns>
        public static LambdaExpression Lambda(Type delegateType, Expression body, string name, IEnumerable<ParameterExpression> parameters) {
            var paramList = parameters.ToReadOnly();
            ValidateLambdaArgs(delegateType, ref body, paramList);

            return Lambda(ExpressionType.Lambda, delegateType, name, body, paramList);
        }

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
                var set = new Set<ParameterExpression>(pis.Length);
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
                    if (set.Contains(pex)) {
                        throw Error.DuplicateVariable(pex);
                    }
                    set.Add(pex);
                }
            } else if (parameters.Count > 0) {
                throw Error.IncorrectNumberOfLambdaDeclarationParameters();
            }
            if (mi.ReturnType != typeof(void) && !TypeUtils.AreReferenceAssignable(mi.ReturnType, body.Type)) {
                if (TypeUtils.IsSameOrSubclass(typeof(LambdaExpression), mi.ReturnType) && mi.ReturnType.IsAssignableFrom(body.GetType())) {
                    body = Expression.Quote(body);
                } else {
                    throw Error.ExpressionTypeDoesNotMatchReturn(body.Type, mi.ReturnType);
                }
            }
        }

        /// <summary>
        /// Creates a Type object that represents a generic System.Func delegate type that has specific type arguments. 
        /// </summary>
        /// <param name="typeArgs">An array of one to five Type objects that specify the type arguments for the System.Func delegate type.</param>
        /// <returns>The type of a System.Func delegate that has the specified type arguments.</returns>
        public static Type GetFuncType(params Type[] typeArgs) {
            ContractUtils.RequiresNotNull(typeArgs, "typeArgs");
            ContractUtils.RequiresNotNullItems(typeArgs, "typeArgs");
            Type result = DelegateHelpers.GetFuncType(typeArgs);
            if (result == null) {
                throw Error.IncorrectNumberOfTypeArgsForFunc();
            }
            return result;
        }

        /// <summary>
        /// Creates a Type object that represents a generic System.Action delegate type that has specific type arguments. 
        /// </summary>
        /// <param name="typeArgs">An array of zero to four Type objects that specify the type arguments for the System.Action delegate type.</param>
        /// <returns>The type of a System.Action delegate that has the specified type arguments.</returns>
        public static Type GetActionType(params Type[] typeArgs) {
            ContractUtils.RequiresNotNull(typeArgs, "typeArgs");
            ContractUtils.RequiresNotNullItems(typeArgs, "typeArgs");
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
        /// </summary>
        /// <param name="typeArgs">The type arguments of the delegate.</param>
        /// <returns>The delegate type.</returns>
        /// <remarks>
        /// As with Func, the last argument is the return type. It can be set
        /// to System.Void to produce an Action.</remarks>
        public static Type GetDelegateType(params Type[] typeArgs) {
            ContractUtils.RequiresNotEmpty(typeArgs, "typeArgs");
            ContractUtils.RequiresNotNullItems(typeArgs, "typeArgs");
            return DelegateHelpers.MakeDelegateType(typeArgs);
        }
    }
}
