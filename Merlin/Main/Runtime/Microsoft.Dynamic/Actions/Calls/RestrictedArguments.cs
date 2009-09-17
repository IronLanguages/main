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
using System.Diagnostics;
using System.Dynamic;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions.Calls {
    public sealed class RestrictedArguments {
        private readonly DynamicMetaObject[] _objects;
        private readonly Type[] _types;
        private readonly bool _hasUntypedRestrictions;

        public RestrictedArguments(DynamicMetaObject[] objects, Type[] types, bool hasUntypedRestrictions) {
            Assert.NotNullItems(objects);
            Assert.NotNull(types);
            Debug.Assert(objects.Length == types.Length);

            _objects = objects;
            _types = types;
            _hasUntypedRestrictions = hasUntypedRestrictions;
        }

        public int Length {
            get { return _objects.Length; }
        }

        public DynamicMetaObject GetObject(int i) {
            return _objects[i];
        }

        public Type GetType(int i) {
            return _types[i];
        }

        /// <summary>
        /// True if there are restrictions beyond just simple type restrictions
        /// </summary>
        public bool HasUntypedRestrictions {
            get {
                return _hasUntypedRestrictions;
            }
        }

        public BindingRestrictions GetAllRestrictions() {
            return BindingRestrictions.Combine(_objects);
        }

        public IList<DynamicMetaObject> GetObjects() {
            return new ReadOnlyCollection<DynamicMetaObject>(_objects);
        }

        public IList<Type> GetTypes() {
            return new ReadOnlyCollection<Type>(_types);
        }
    }
}
