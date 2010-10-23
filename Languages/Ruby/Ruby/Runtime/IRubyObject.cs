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

using IronRuby.Builtins;
using IronRuby.Compiler.Generation;

namespace IronRuby.Runtime {
    [ReflectionCached]
    public interface IRubyObject : IRubyObjectState {
        /// <summary>
        /// Gets or sets the immediate class of this object.
        /// </summary>
        [Emitted]
        RubyClass/*!*/ ImmediateClass { get; set; }

        // Returns the instance object data. May return null.
        [Emitted]
        RubyInstanceData TryGetInstanceData();

        // Returns the instance object data.
        [Emitted]
        RubyInstanceData/*!*/ GetInstanceData();

        // Calls GetHashCode via static virtual dispatch, not virtual dynamic dispatch.
        [Emitted]
        int BaseGetHashCode();

        // Calls Equals via static virtual dispatch, not virtual dynamic dispatch.
        [Emitted]
        bool BaseEquals(object other);

        // Calls ToString via static virtual dispatch, not virtual dynamic dispatch.
        [Emitted]
        string/*!*/ BaseToString();
    }
}
