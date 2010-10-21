/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using Microsoft.PyAnalysis.Values;

namespace Microsoft.PyAnalysis {
    internal static class HashSetExtensions {
        public static ISet<Namespace> Union(this ISet<Namespace> self, ISet<Namespace> value) {
            bool dummy = false;
            return Union(self, value, ref dummy);
        }

        /// <summary>
        /// Returns the union of the new two sets as a new set tracking whether
        /// or not self is a locally created HashSet.
        /// </summary>
        public static ISet<Namespace> Union(this ISet<Namespace> self, ISet<Namespace> value, ref bool madeSet) {
            Namespace selfOne, valueOne;

            if (self.Count == 0) {
                return value;
            } else if (value.Count == 0) {
                return self;
            } else if ((selfOne = self as Namespace) != null) {
                if ((valueOne = value as Namespace) != null) {
                    var res = new HashSet<Namespace>();
                    res.Add(selfOne);
                    res.Add(valueOne);
                    return res;
                } else {
                    var res = new HashSet<Namespace>(value);
                    res.Add(selfOne);
                    return res;
                }
            } else if ((valueOne = value as Namespace) != null) {
                var res = new HashSet<Namespace>(self);
                res.Add(valueOne);
                return res;
            }

            if (!madeSet) {
                self = new HashSet<Namespace>(self);
                madeSet = true;
            }
            self.UnionWith(value);
            return self;
        }
    }
}
