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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Dynamic.Utils;

namespace System.Linq.Expressions.Compiler {
    internal sealed class AnalyzedTree {
        internal readonly Dictionary<object, CompilerScope> Scopes = new Dictionary<object, CompilerScope>();
        internal readonly Dictionary<LambdaExpression, BoundConstants> Constants = new Dictionary<LambdaExpression, BoundConstants>();

        // Lazy initialized because many trees will not need it
        private Dictionary<SymbolDocumentInfo, ISymbolDocumentWriter> _symbolWriters;
        internal Dictionary<SymbolDocumentInfo, ISymbolDocumentWriter> SymbolWriters {
            get {
                if (_symbolWriters == null) {
                    _symbolWriters = new Dictionary<SymbolDocumentInfo, ISymbolDocumentWriter>();
                }
                return _symbolWriters;
            }
        }

        // Created by VariableBinder
        internal AnalyzedTree() {
        }
    }
}
