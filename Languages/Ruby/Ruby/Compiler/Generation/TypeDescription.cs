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

using System;
using System.Collections.Generic;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Generation {

    /// <summary>
    /// TypeDescription captures the minimal information required by TypeDispenser to define a distinct CLS type
    /// </summary>
    internal sealed class TypeDescription {
        // The CLI base-type.
        private readonly Type/*!*/ _baseType;

        // Do not override base type's virtual methods.
        private readonly bool _noOverrides;

        private readonly IList<ITypeFeature/*!*/>/*!*/ _features;
        private readonly int _hash;

        public TypeDescription(Type/*!*/ baseType, IList<ITypeFeature/*!*/>/*!*/ features, bool noOverrides) {
            Assert.NotNull(baseType);
            Assert.NotNull(features);

            _noOverrides = noOverrides;
            _baseType = baseType;
            _features = features;

            _hash = (_noOverrides ? 3 : 7) ^ _baseType.GetHashCode();
            for (int i = 0; i < features.Count; i++) {
                _hash ^= features[i].GetHashCode();
            }
        }

        public Type/*!*/ BaseType {
            get { return _baseType; }
        }

        public bool NoOverrides {
            get { return _noOverrides; }
        }

        public IList<ITypeFeature/*!*/>/*!*/ Features {
            get { return _features; }
        }

        public override int GetHashCode() {
            return _hash;
        }

        public static bool FeatureSetsMatch(IList<ITypeFeature/*!*/>/*!*/ f1, IList<ITypeFeature/*!*/>/*!*/ f2) {
            if (f1.Count != f2.Count || f1.GetHashCode() != f2.GetHashCode()) {
                return false;
            }

            // The size of a feature set is expected to be small enough that this should be
            // reasonably fast.  If in the future, sets grows much larger -- perhaps
            // because we expose a large number of CLS methods -- then the representation of
            // a feature set may need to be modified to allow for faster indexing
            foreach (ITypeFeature feature in f2) {
                if (!f1.Contains(feature)) {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj) {
            TypeDescription other = obj as TypeDescription;
            if (other == null) return false;

            if (_baseType.Equals(other._baseType) && FeatureSetsMatch(Features, other.Features)) {
                return true;
            }
            return false;
        }
    }
}
