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

using System.Dynamic;
using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast {

    public abstract class Node {
        private readonly SourceSpan _location;
        
        public abstract NodeTypes NodeType { get; }
        internal protected abstract void Walk(Walker/*!*/ walker);
        
        public SourceSpan Location {
            get { return _location; }
        }

        protected Node(SourceSpan location) {
            _location = location;

#if DEBUG
            PerfTrack.NoteEvent(PerfTrack.Categories.Count, "RubyAST: " + GetType().Name);
#endif

        }
    }
}



