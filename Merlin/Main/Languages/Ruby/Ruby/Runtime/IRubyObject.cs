/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Compiler.Generation;

namespace IronRuby {
    public interface IRubyObject {
        // Gets the ruby class associated with this object
        [Emitted] // RubyTypeBuilder
        RubyClass/*!*/ Class { get; }

        // Returns the instance object data. May return null.
        [Emitted] // RubyTypeBuilder
        RubyInstanceData TryGetInstanceData();

        // Returns the instance object data.
        [Emitted] // RubyTypeBuilder
        RubyInstanceData/*!*/ GetInstanceData();
    }
}
