/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace IronRuby.Builtins {
    /// <summary>
    /// Represents BasicObject class.
    /// </summary>
    public class BasicObject : RubyObject {
        /// <summary>
        /// Implements BasicObject#new.
        /// </summary>
        public BasicObject(RubyClass/*!*/ cls) 
            : base(cls) {
        }

        /// <summary>
        /// Implements BasicObject#new.
        /// </summary>
        public BasicObject(RubyClass/*!*/ cls, params object[] args) 
            : base(cls, args) {
        }
    }

    /// <summary>
    /// Represents Kernel module mixed into Object class.
    /// </summary>
    public static class Kernel {
        // stub
    }

    /// <summary>
    /// Represents a module mixed into all multi-dimensional CLR arrays.
    /// </summary>
    public static class MultiDimensionalArray {
        // stub
    }

    /// <summary>
    /// Represents a module mixed into all flag enums.
    /// </summary>
    public static class FlagEnumeration {
        // stub
    }
}
