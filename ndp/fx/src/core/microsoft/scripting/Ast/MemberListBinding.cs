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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Dynamic.Utils;

namespace System.Linq.Expressions {
    //CONFORMING
    public sealed class MemberListBinding : MemberBinding {
        ReadOnlyCollection<ElementInit> _initializers;
        internal MemberListBinding(MemberInfo member, ReadOnlyCollection<ElementInit> initializers)
            : base(MemberBindingType.ListBinding, member) {
            _initializers = initializers;
        }
        public ReadOnlyCollection<ElementInit> Initializers {
            get { return _initializers; }
        }
    }
    

    public partial class Expression {
        //CONFORMING
        public static MemberListBinding ListBind(MemberInfo member, params ElementInit[] initializers) {
            ContractUtils.RequiresNotNull(member, "member");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            return ListBind(member, initializers.ToReadOnly());
        }
        //CONFORMING
        public static MemberListBinding ListBind(MemberInfo member, IEnumerable<ElementInit> initializers) {
            ContractUtils.RequiresNotNull(member, "member");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            Type memberType;
            ValidateGettableFieldOrPropertyMember(member, out memberType);
            ReadOnlyCollection<ElementInit> initList = initializers.ToReadOnly();
            ValidateListInitArgs(memberType, initList);
            return new MemberListBinding(member, initList);
        }
        //CONFORMING
        public static MemberListBinding ListBind(MethodInfo propertyAccessor, params ElementInit[] initializers) {
            ContractUtils.RequiresNotNull(propertyAccessor, "propertyAccessor");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            return ListBind(propertyAccessor, initializers.ToReadOnly());
        }
        //CONFORMING
        public static MemberListBinding ListBind(MethodInfo propertyAccessor, IEnumerable<ElementInit> initializers) {
            ContractUtils.RequiresNotNull(propertyAccessor, "propertyAccessor");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            return ListBind(GetProperty(propertyAccessor), initializers);
        }

        //CONFORMING
        private static void ValidateListInitArgs(Type listType, ReadOnlyCollection<ElementInit> initializers) {
            if (!typeof(IEnumerable).IsAssignableFrom(listType)) {
                throw Error.TypeNotIEnumerable(listType);
            }
            for (int i = 0, n = initializers.Count; i < n; i++) {
                ElementInit element = initializers[i];
                ContractUtils.RequiresNotNull(element, "initializers");
                ValidateCallInstanceType(listType, element.AddMethod);
            }
        }
    }
}