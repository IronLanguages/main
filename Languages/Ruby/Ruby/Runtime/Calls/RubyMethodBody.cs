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

#if FEATURE_CORE_DLR
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

using System;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Compiler.Ast;
using IronRuby.Compiler.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using MethodDeclaration = IronRuby.Compiler.Ast.MethodDefinition;

namespace IronRuby.Runtime.Calls {
    /// <summary>
    /// Represents a Ruby method body AST. Multiple RubyMethodInfos can share the same instance.
    /// </summary>
    public sealed class RubyMethodBody {
        private readonly MethodDeclaration/*!*/ _ast;
        private readonly MSA.SymbolDocumentInfo _document;
        private readonly RubyEncoding/*!*/ _encoding;

        private Delegate _delegate;

        internal RubyMethodBody(MethodDeclaration/*!*/ ast, MSA.SymbolDocumentInfo document, RubyEncoding/*!*/ encoding) {
            Assert.NotNull(ast, encoding);

            _ast = ast;
            _document = document;
            _encoding = encoding;
        }

        public MethodDeclaration Ast { get { return _ast; } }
        public MSA.SymbolDocumentInfo Document { get { return _document; } }
        public bool HasTarget { get { return _ast.Target != null; } }
        public string/*!*/ Name { get { return _ast.Name; } }

        internal Delegate GetDelegate(RubyScope/*!*/ declaringScope, RubyModule/*!*/ declaringModule) {
            if (_delegate == null) {
                lock (this) {
                    if (_delegate == null) {
                        // TODO: remove options
                        AstGenerator gen = new AstGenerator(declaringScope.RubyContext, new RubyCompilerOptions(), _document, _encoding, false);
                        MSA.LambdaExpression lambda = _ast.TransformBody(gen, declaringScope, declaringModule);
                        _delegate = RubyScriptCode.CompileLambda(lambda, declaringScope.RubyContext);
                    }
                }
            }

            return _delegate;
        }
    }
}
