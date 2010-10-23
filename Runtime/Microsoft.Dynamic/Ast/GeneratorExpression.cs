/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// A parameterless generator, that is of type IEnumerable, IEnumerable{T},
    /// IEnumerator, or IEnumerator{T}. Its body can contain a series of
    /// YieldExpressions. Each call into MoveNext on the enumerator reenters
    /// the generator, and executes until it reaches a YieldReturn or YieldBreak
    /// expression
    /// </summary>
    public sealed class GeneratorExpression : Expression {
        private readonly LabelTarget _target;
        private readonly Expression _body;
        private Expression _reduced;
        private readonly Type _type;
        private readonly string _name;
        private readonly bool _rewriteAssignments;

        internal GeneratorExpression(string name, Type type, LabelTarget label, Expression body, bool rewriteAssignments) {
            _target = label;
            _body = body;
            _type = type;
            _name = name;
            _rewriteAssignments = rewriteAssignments;
        }

        public override bool CanReduce {
            get { return true; }
        }

        public sealed override Type Type {
            get { return _type; }
        }

        public sealed override ExpressionType NodeType {
            get { return ExpressionType.Extension; }
        }

        public string Name {
            get { return _name; }
        }

        /// <summary>
        /// The label used by YieldBreak and YieldReturn expressions to yield
        /// from this generator
        /// </summary>
        public LabelTarget Target {
            get { return _target; }
        }

        /// <summary>
        /// The body of the generator, which can contain YieldBreak and
        /// YieldReturn expressions
        /// </summary>
        public Expression Body {
            get { return _body; }
        }

        /// <summary>
        /// Indicates whether the lhs instances are preserved when assignments
        /// are made to expressions containing yields.
        /// </summary>
        public bool RewriteAssignments {
            get { return _rewriteAssignments; }
        }

        public override Expression Reduce() {
            if (_reduced == null) {
                _reduced = new GeneratorRewriter(this).Reduce();
            }
            return _reduced;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor) {
            Expression b = visitor.Visit(_body);
            if (b == _body) {
                return this;
            }
            return Utils.Generator(_name, _target, b, Type);
        }

        internal bool IsEnumerable {
            get { return Utils.IsEnumerableType(Type); }
        }
    }

    public partial class Utils {        
        /// <summary>
        /// Creates a generator with type IEnumerable{T}, where T is the label.Type
        /// </summary>
        public static GeneratorExpression Generator(LabelTarget label, Expression body) {
            ContractUtils.RequiresNotNull(label, "label");
            ContractUtils.RequiresNotNull(body, "body");
            ContractUtils.Requires(label.Type != typeof(void), "label", "label must have a non-void type");

            return new GeneratorExpression("generator", typeof(IEnumerable<>).MakeGenericType(label.Type), label, body, true);
        }

        public static GeneratorExpression Generator(LabelTarget label, Expression body, Type type) {
            return Generator("generator", label, body, type);
        }

        public static GeneratorExpression Generator(string name, LabelTarget label, Expression body, Type type) {
            return Generator(name, label, body, type, true);
        }

        public static GeneratorExpression Generator(string name, LabelTarget label, Expression body, Type type, bool rewriteAssignments) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(body, "body");
            ContractUtils.RequiresNotNull(label, "label");
            ContractUtils.Requires(label.Type != typeof(void), "label", "label must have a non-void type");
            ContractUtils.Requires(body.Type == typeof(void), "body", "generator body must have a void type");

            // Generator type must be one of: IEnumerable, IEnumerator,
            // IEnumerable<T>, or IEnumerator<T>, where T is label.Ttpe
            if (type.IsGenericType) {
                Type genType = type.GetGenericTypeDefinition();
                if (genType != typeof(IEnumerable<>) && genType != typeof(IEnumerator<>)
                    || type.GetGenericArguments()[0] != label.Type) {
                    throw GeneratorTypeMustBeEnumerableOfT(label.Type);
                }
            } else if (type != typeof(IEnumerable) && type != typeof(IEnumerator)) {
                throw GeneratorTypeMustBeEnumerableOfT(label.Type);
            }

            ContractUtils.RequiresNotNull(body, "body");

            return new GeneratorExpression(name, type, label, body, rewriteAssignments);
        }

        private static ArgumentException GeneratorTypeMustBeEnumerableOfT(Type type) {
            return new ArgumentException(string.Format("Generator must be of type IEnumerable<T>, IEnumerator<T>, IEnumerable, or IEnumerator, where T is '{0}'", type));
        }

        internal static bool IsEnumerableType(Type type) {
            return type == typeof(IEnumerable) ||
                type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        #region Generator lambda factories

        public static Expression<T> GeneratorLambda<T>(LabelTarget label, Expression body, params ParameterExpression[] parameters) {
            return (Expression<T>)GeneratorLambda(typeof(T), label, body, null, parameters);
        }

        public static Expression<T> GeneratorLambda<T>(LabelTarget label, Expression body, string name, params ParameterExpression[] parameters) {
            return (Expression<T>)GeneratorLambda(typeof(T), label, body, name, parameters);
        }

        public static Expression<T> GeneratorLambda<T>(LabelTarget label, Expression body, string name, IEnumerable<ParameterExpression> parameters) {
            return (Expression<T>)GeneratorLambda(typeof(T), label, body, name, parameters);
        }

        public static LambdaExpression GeneratorLambda(Type delegateType, LabelTarget label, Expression body, params ParameterExpression[] parameters) {
            return GeneratorLambda(delegateType, label, body, null, parameters);
        }

        public static LambdaExpression GeneratorLambda(Type delegateType, LabelTarget label, Expression body, string name, params ParameterExpression[] parameters) {
            return GeneratorLambda(delegateType, label, body, name, (IEnumerable<ParameterExpression>)parameters);
        }

        public static LambdaExpression GeneratorLambda(Type delegateType, LabelTarget label, Expression body, string name, IEnumerable<ParameterExpression> parameters) {
            return GeneratorLambda(delegateType, label, body, name, true, (IEnumerable<ParameterExpression>)parameters);
        }

        // Creates a GeneratorLambda as a lambda containing a parameterless
        // generator. In the case where we return an IEnumerator, it's a very
        // simple, constant-time construction. However, if the result is
        // IEnumerable, it will perform a full tree walk to ensure that each
        // call to GetEnumerator() returns an IEnumerator with the same
        // values for the parameters.
        public static LambdaExpression GeneratorLambda(
            Type delegateType,
            LabelTarget label,
            Expression body,
            string name,
            bool rewriteAssignments,
            IEnumerable<ParameterExpression> parameters)
        {
            ContractUtils.RequiresNotNull(delegateType, "delegateType");
            ContractUtils.Requires(delegateType.IsSubclassOf(typeof(MulticastDelegate)), "Lambda type parameter must be derived from System.Delegate");
            Type generatorType = delegateType.GetMethod("Invoke").GetReturnType();

            var paramList = parameters.ToReadOnly();
            if (IsEnumerableType(generatorType)) {
                // rewrite body
                body = TransformEnumerable(body, paramList);
            }

            return Expression.Lambda(
                 delegateType,
                 Utils.Generator(name, label, body, generatorType, rewriteAssignments),
                 name,
                 paramList
             );
        }

        // Creates a GeneratorLambda as a lambda containing a parameterless
        // generator. Because we want parameters to be captured by value and
        // not as variables, we have to do a transformation more like this:
        ///
        //    static IEnumerable<int> Foo(int count) {
        //        count *= 2;
        //        for (int i = 0; i < count; i++) {
        //            yield return i;
        //        }
        //    }
        //
        // Becomes:
        //
        //    static IEnumerable<int> Foo(int count) {
        //        return generator {
        //            int __count = count;
        //            __count *= 2;
        //            for (int i = 0; i < __count; i++) {
        //                yield return i;
        //            }
        //        }
        //    }
        //
        // This involves a full rewrite, unfortunately.
        private static Expression TransformEnumerable(Expression body, ReadOnlyCollection<ParameterExpression> paramList) {
            if (paramList.Count == 0) {
                return body;
            }
            int count = paramList.Count;
            var vars = new ParameterExpression[count];
            var map = new Dictionary<ParameterExpression, ParameterExpression>(count);
            var block = new Expression[count + 1];
            for (int i = 0; i < count; i++) {
                ParameterExpression param = paramList[i];
                vars[i] = Expression.Variable(param.Type, param.Name);
                map.Add(param, vars[i]);
                block[i] = Expression.Assign(vars[i], param);
            }
            block[count] = new LambdaParameterRewriter(map).Visit(body);
            return Expression.Block(
                new ReadOnlyCollection<ParameterExpression>(vars), 
                new ReadOnlyCollection<Expression>(block)
            );
        }

        #endregion
    }
}
