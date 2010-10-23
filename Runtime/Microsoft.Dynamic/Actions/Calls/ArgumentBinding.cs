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
using System.Text;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions.Calls {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")] // TODO
    public struct ArgumentBinding {
        private static readonly int[] _EmptyBinding = new int[0];

        private readonly int _positionalArgCount;
        private readonly int[] _binding; // immutable

        internal ArgumentBinding(int positionalArgCount) {
            _positionalArgCount = positionalArgCount;
            _binding = _EmptyBinding;
        }

        internal ArgumentBinding(int positionalArgCount, int[] binding) {
            Assert.NotNull(binding);
            _binding = binding;
            _positionalArgCount = positionalArgCount;
        }

        public int PositionalArgCount {
            get { return _positionalArgCount; }
        }

        public int ArgumentToParameter(int argumentIndex) {
            int i = argumentIndex - _positionalArgCount;
            return (i < 0) ? argumentIndex : _positionalArgCount + _binding[i];
        }
    }
}
