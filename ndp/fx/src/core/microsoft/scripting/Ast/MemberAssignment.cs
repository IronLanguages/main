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

using System;
using System.Reflection;
using System.Text;
using System.Dynamic.Utils;

namespace System.Linq.Expressions {
    //CONFORMING
    public sealed class MemberAssignment : MemberBinding {
        Expression _expression;
        internal MemberAssignment(MemberInfo member, Expression expression)
            : base(MemberBindingType.Assignment, member) {
            _expression = expression;
        }
        public Expression Expression {
            get { return _expression; }
        }
    }


    public partial class Expression {
        //CONFORMING
        public static MemberAssignment Bind(MemberInfo member, Expression expression) {
            ContractUtils.RequiresNotNull(member, "member");
            RequiresCanRead(expression, "expression");
            Type memberType;
            ValidateSettableFieldOrPropertyMember(member, out memberType);
            if (!memberType.IsAssignableFrom(expression.Type)) {
                throw Error.ArgumentTypesMustMatch();
            }
            return new MemberAssignment(member, expression);
        }

        //CONFORMING
        public static MemberAssignment Bind(MethodInfo propertyAccessor, Expression expression) {
            ContractUtils.RequiresNotNull(propertyAccessor, "propertyAccessor");
            ContractUtils.RequiresNotNull(expression, "expression");
            ValidateMethodInfo(propertyAccessor);
            return Bind(GetProperty(propertyAccessor), expression);
        }
        
        //CONFORMING
        private static void ValidateSettableFieldOrPropertyMember(MemberInfo member, out Type memberType) {
            FieldInfo fi = member as FieldInfo;
            if (fi == null) {
                PropertyInfo pi = member as PropertyInfo;
                if (pi == null) {
                    throw Error.ArgumentMustBeFieldInfoOrPropertInfo();
                }
                if (!pi.CanWrite) {
                    throw Error.PropertyDoesNotHaveSetter(pi);
                }
                memberType = pi.PropertyType;
            } else {
                memberType = fi.FieldType;
            }
        }
    }
}