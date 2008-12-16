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

using System.Reflection;
using System.Text;

namespace System.Linq.Expressions {
    //CONFORMING
    public enum MemberBindingType {
        Assignment,
        MemberBinding,
        ListBinding
    }

    //CONFORMING
    public abstract class MemberBinding {
        MemberBindingType _type;
        MemberInfo _member;
        protected MemberBinding(MemberBindingType type, MemberInfo member) {
            _type = type;
            _member = member;
        }
        public MemberBindingType BindingType {
            get { return _type; }
        }
        public MemberInfo Member {
            get { return _member; }
        }
        public override string ToString() {
            return ExpressionStringBuilder.MemberBindingToString(this);
        }
    }
}