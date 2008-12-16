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
using System.Dynamic.Utils;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Dynamic {
    public class DynamicMetaObject {
        private readonly Expression _expression;
        private readonly BindingRestrictions _restrictions;
        private readonly object _value;
        private readonly bool _hasValue;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly DynamicMetaObject[] EmptyMetaObjects = new DynamicMetaObject[0];

        public DynamicMetaObject(Expression expression, BindingRestrictions restrictions) {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.RequiresNotNull(restrictions, "restrictions");

            _expression = expression;
            _restrictions = restrictions;
        }

        public DynamicMetaObject(Expression expression, BindingRestrictions restrictions, object value)
            : this(expression, restrictions) {
            _value = value;
            _hasValue = true;
        }

        public Expression Expression {
            get {
                return _expression;
            }
        }

        public BindingRestrictions Restrictions {
            get {
                return _restrictions;
            }
        }

        public object Value {
            get {
                return _value;
            }
        }

        public bool HasValue {
            get {
                return _hasValue;
            }
        }

        public Type RuntimeType {
            get {
                if (_hasValue) {
                    Type ct = Expression.Type;
                    // valuetype at compile tyme, type cannot change.
                    if (ct.IsValueType) {
                        return ct;
                    }
                    if (_value != null) {
                        return _value.GetType();
                    } else {
                        return typeof(DynamicNull);
                    }
                } else {
                    return null;
                }
            }
        }

        public Type LimitType {
            get {
                return RuntimeType ?? Expression.Type;
            }
        }

        public bool IsDynamicObject {
            get {
                return _value is IDynamicObject;
            }
        }

        public virtual DynamicMetaObject BindConvert(ConvertBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackConvert(this);
        }

        public virtual DynamicMetaObject BindGetMember(GetMemberBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackGetMember(this);
        }

        public virtual DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackSetMember(this, value);
        }

        public virtual DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackDeleteMember(this);
        }

        public virtual DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackGetIndex(this, indexes);
        }

        public virtual DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackSetIndex(this, indexes, value);
        }

        public virtual DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackDeleteIndex(this, indexes);
        }

        public virtual DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackInvokeMember(this, args);
        }

        public virtual DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackInvoke(this, args);
        }

        public virtual DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackCreateInstance(this, args);
        }

        public virtual DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackUnaryOperation(this);
        }

        public virtual DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackBinaryOperation(this, arg);
        }

        /// <summary>
        /// Binds an operation a.b (op)= c
        /// </summary>
        /// <param name="binder">Binder implementing the language semantics.</param>
        /// <param name="value">Meta Object representing the right-side argument.</param>
        /// <returns>MetaObject representing the result of the binding.</returns>
        public virtual DynamicMetaObject BindBinaryOperationOnMember(BinaryOperationOnMemberBinder binder, DynamicMetaObject value) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackBinaryOperationOnMember(this, value);
        }


        /// <summary>
        /// Binds an operation a[i,j,k] (op)= c
        /// </summary>
        /// <param name="binder">Binder implementing the language semantics.</param>
        /// <param name="indexes">The array of MetaObjects representing the indexes for the index operation.</param>
        /// <param name="value">The MetaObject representing the right-hand value of the operation.</param>
        /// <returns>MetaObject representing the result of the binding.</returns>
        public virtual DynamicMetaObject BindBinaryOperationOnIndex(BinaryOperationOnIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackBinaryOperationOnIndex(this, indexes, value);
        }

        /// <summary>
        /// Binds the unary operation performed on a result of index operation on the object.
        /// </summary>
        /// <param name="binder">The binder implementing the language semantics.</param>
        /// <param name="indexes">The array of MetaObject representing the indexes for the index operation.</param>
        /// <returns>The MetaObject representing the result of the binding.</returns>
        public virtual DynamicMetaObject BindUnaryOperationOnIndex(UnaryOperationOnIndexBinder binder, DynamicMetaObject[] indexes) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackUnaryOperationOnIndex(this, indexes);
        }

        /// <summary>
        /// Binds the unary operation performed on a result of get member operation on the object.
        /// </summary>
        /// <param name="binder">The binder implementing the language semantics.</param>
        /// <returns>The MetaObject representing the result of the binding.</returns>
        public virtual DynamicMetaObject BindUnaryOperationOnMember(UnaryOperationOnMemberBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackUnaryOperationOnMember(this);
        }

        /// <summary>
        /// Returns the enumeration of all dynamic member names.
        /// </summary>
        /// <returns>The list of dynamic members.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual IEnumerable<string> GetDynamicMemberNames() {
            return new string[0];
        }

        /// <summary>
        /// Returns the enumeration of key-value pairs of all dynamic data members. Data members include members
        /// such as properties, fields, but not necessarily methods. The key value pair includes the member name
        /// and the value.
        /// </summary>
        /// <returns>The list of key-value pairs representing data member name and its value.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public virtual IEnumerable<KeyValuePair<string, object>> GetDynamicDataMembers() {
            return new KeyValuePair<string, object>[0];
        }

        // Internal helpers

        internal static Type[] GetTypes(DynamicMetaObject[] objects) {
            Type[] res = new Type[objects.Length];
            for (int i = 0; i < objects.Length; i++) {
                res[i] = objects[i].RuntimeType ?? objects[i].Expression.Type;
            }

            return res;
        }

        public static Expression[] GetExpressions(DynamicMetaObject[] objects) {
            ContractUtils.RequiresNotNull(objects, "objects");

            Expression[] res = new Expression[objects.Length];
            for (int i = 0; i < objects.Length; i++) {
                DynamicMetaObject mo = objects[i];
                ContractUtils.RequiresNotNull(mo, "objects");
                Expression expr = mo.Expression;
                ContractUtils.RequiresNotNull(expr, "objects");
                res[i] = expr;
            }

            return res;
        }

        public static DynamicMetaObject ObjectToMetaObject(object argValue, Expression parameterExpression) {
            IDynamicObject ido = argValue as IDynamicObject;
            if (ido != null) {
                return ido.GetMetaObject(parameterExpression);
            } else {
                return new DynamicMetaObject(parameterExpression, BindingRestrictions.Empty, argValue);
            }
        }

        public static DynamicMetaObject CreateThrow(DynamicMetaObject target, DynamicMetaObject[] args, Type exception, params object[] exceptionArgs) {
            return CreateThrow(
                target,
                args,
                exception,
                exceptionArgs != null ? exceptionArgs.Map<object, Expression>((arg) => Expression.Constant(arg)) : null
            );
        }

        public static DynamicMetaObject CreateThrow(DynamicMetaObject target, DynamicMetaObject[] args, Type exception, params Expression[] exceptionArgs) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNull(exception, "exception");

            Type[] argTypes = exceptionArgs != null ? exceptionArgs.Map((arg) => arg.Type) : Type.EmptyTypes;
            ConstructorInfo constructor = exception.GetConstructor(argTypes);

            if (constructor == null) {
                throw new ArgumentException(Strings.TypeDoesNotHaveConstructorForTheSignature);
            }

            return new DynamicMetaObject(
                Expression.Throw(
                    Expression.New(
                        exception.GetConstructor(argTypes),
                        exceptionArgs
                    )
                ),
                target.Restrictions.Merge(BindingRestrictions.Combine(args))
            );
        }
    }
}
