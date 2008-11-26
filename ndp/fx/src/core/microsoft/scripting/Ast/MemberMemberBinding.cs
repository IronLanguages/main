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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Dynamic.Utils;

namespace System.Linq.Expressions {
    //CONFORMING
    public sealed class MemberMemberBinding : MemberBinding {
        ReadOnlyCollection<MemberBinding> _bindings;
        internal MemberMemberBinding(MemberInfo member, ReadOnlyCollection<MemberBinding> bindings)
            : base(MemberBindingType.MemberBinding, member) {
            _bindings = bindings;
        }
        public ReadOnlyCollection<MemberBinding> Bindings {
            get { return _bindings; }
        }
    }
    

    public partial class Expression {
        //CONFORMING
        public static MemberMemberBinding MemberBind(MemberInfo member, params MemberBinding[] bindings) {
            ContractUtils.RequiresNotNull(member, "member");
            ContractUtils.RequiresNotNull(bindings, "bindings");
            return MemberBind(member, bindings.ToReadOnly());
        }
        //CONFORMING
        public static MemberMemberBinding MemberBind(MemberInfo member, IEnumerable<MemberBinding> bindings) {
            ContractUtils.RequiresNotNull(member, "member");
            ContractUtils.RequiresNotNull(bindings, "bindings");
            ReadOnlyCollection<MemberBinding> roBindings = bindings.ToReadOnly();
            Type memberType;
            ValidateGettableFieldOrPropertyMember(member, out memberType);
            ValidateMemberInitArgs(memberType, roBindings);
            return new MemberMemberBinding(member, roBindings);
        }
        //CONFORMING
        public static MemberMemberBinding MemberBind(MethodInfo propertyAccessor, params MemberBinding[] bindings) {
            ContractUtils.RequiresNotNull(propertyAccessor, "propertyAccessor");
            return MemberBind(GetProperty(propertyAccessor), bindings);
        }
        //CONFORMING
        public static MemberMemberBinding MemberBind(MethodInfo propertyAccessor, IEnumerable<MemberBinding> bindings) {
            ContractUtils.RequiresNotNull(propertyAccessor, "propertyAccessor");
            return MemberBind(GetProperty(propertyAccessor), bindings);
        }


        //CONFORMING
        private static void ValidateGettableFieldOrPropertyMember(MemberInfo member, out Type memberType) {
            FieldInfo fi = member as FieldInfo;
            if (fi == null) {
                PropertyInfo pi = member as PropertyInfo;
                if (pi == null) {
                    throw Error.ArgumentMustBeFieldInfoOrPropertInfo();
                }
                if (!pi.CanRead) {
                    throw Error.PropertyDoesNotHaveGetter(pi);
                }
                memberType = pi.PropertyType;
            } else {
                memberType = fi.FieldType;
            }
        }
        //CONFORMING
        private static void ValidateMemberInitArgs(Type type, ReadOnlyCollection<MemberBinding> bindings) {
            for (int i = 0, n = bindings.Count; i < n; i++) {
                MemberBinding b = bindings[i];
                ContractUtils.RequiresNotNull(b, "bindings");
                if (!b.Member.DeclaringType.IsAssignableFrom(type)) {
                    throw Error.NotAMemberOfType(b.Member.Name, type);
                }
            }
        }
    }
}