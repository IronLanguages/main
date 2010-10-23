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

#if !CLR2
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using System;

namespace IronRuby.Compiler.Ast {
    using Ast = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    
    public sealed class Statements : IEnumerable<Expression> {
        private static readonly Statements _Empty = new Statements();

        internal static Statements/*!*/ Empty {
            get {
                Debug.Assert(_Empty.Count == 0);
                return _Empty;
            }
        }

        private Expression[] _statements;
        private int _count;

        public Statements() {
        }

        public int Count {
            get { return _count; }
        }

        public Statements(Expression/*!*/ statement) {
            Assert.NotNull(statement);
            AddFirst(statement);
        }

        private void AddFirst(Expression/*!*/ statement) {
            _statements = new Expression[] { statement };
            _count = 1;
        }

        public Expression/*!*/ Add(Expression/*!*/ statement) {
            Assert.NotNull(statement);
            if (_count == 0) {
                AddFirst(statement);
            } else {
                if (_count == _statements.Length) {
                    Array.Resize(ref _statements, 2 * _count);
                }
                _statements[_count] = statement;
                _count++;
            }
            return statement;
        }

        public Expression/*!*/ First {
            get { return _statements[0]; }
        }

        public Expression/*!*/ Last {
            get { return _statements[_count - 1]; }
        }

        public IEnumerable<Expression>/*!*/ AllButLast {
            get {
                for (int i = 0; i < _count - 1; i++) {
                    yield return _statements[i];
                }
            }
        }

        public IEnumerator<Expression>/*!*/ GetEnumerator() {
            for (int i = 0; i < _count; i++) {
                yield return _statements[i];
            }
        }

        System.Collections.IEnumerator/*!*/ System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
