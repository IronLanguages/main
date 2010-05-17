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

using System.Dynamic;
using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast {
    public struct Identifier {
        private readonly string/*!*/ _name;
        private readonly SourceSpan _location;

        public string/*!*/ Name { get { return _name; } }
        public SourceSpan Location { get { return _location; } }

        public Identifier(string/*!*/ name, SourceSpan location) {
            _name = name;
            _location = location;
        }
    }

}
