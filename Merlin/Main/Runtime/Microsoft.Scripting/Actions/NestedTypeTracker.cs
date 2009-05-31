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
using Microsoft.Contracts;

namespace Microsoft.Scripting.Actions {
    public class NestedTypeTracker : TypeTracker {
        private readonly Type _type;

        public NestedTypeTracker(Type type) {
            _type = type;
        }

        public override Type DeclaringType {
            get { return _type.DeclaringType; }
        }

        public override TrackerTypes MemberType {
            get { return TrackerTypes.Type; }
        }

        public override string Name {
            get { return _type.Name; }
        }

        public override bool IsPublic {
            get {
                return _type.IsPublic;
            }
        }

        public override Type Type {
            get { return _type; }
        }

        public override bool IsGenericType {
            get { return _type.IsGenericType; }
        }

        [Confined]
        public override string ToString() {
            return _type.ToString();
        }
    }
}
