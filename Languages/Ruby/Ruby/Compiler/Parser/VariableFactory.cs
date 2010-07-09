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
using Microsoft.Scripting.Utils;
using IronRuby.Compiler.Ast;

namespace IronRuby.Compiler {
    internal static class VariableFactory {
        public const int Identifier = 0;
        public const int Instance = 1;
        public const int Global = 2;
        public const int Constant = 3;
        public const int Class = 4;
        public const int Nil = 5;
        public const int Self = 6;
        public const int True = 7;
        public const int False = 8;
        public const int File = 9;
        public const int Line = 10;
        public const int Encoding = 11;

        internal static Expression/*!*/ MakeRead(int kind, Parser/*!*/ parser, string name, SourceSpan location) {
            switch (kind) {
                case Identifier:
                    return (Expression)parser.CurrentScope.ResolveVariable(name) ?? new MethodCall(null, name, null, null, location);

                case Instance:
                    return new InstanceVariable(name, location);

                case Global:
                    return new GlobalVariable(name, location);

                case Constant:
                    return new ConstantVariable(name, location);

                case Class:
                    return new ClassVariable(name, location);

                case Nil:
                    return Literal.Nil(location);

                case Self:
                    return new SelfReference(location);

                case True:
                    return Literal.True(location);

                case False:
                    return Literal.False(location);

                case File:
                    return new FileLiteral(location);

                case Line:
                    return Literal.Integer(parser.Tokenizer.TokenSpan.Start.Line, location);

                case Encoding:
                    return new EncodingExpression(location);
            }

            throw Assert.Unreachable;
        }

        internal static LeftValue/*!*/ MakeLeftValue(int kind, Parser/*!*/ parser, string name, SourceSpan location) {
            switch (kind) {
                case Identifier:
                    return parser.CurrentScope.ResolveOrAddVariable(name, location);

                case Instance:
                    return new InstanceVariable(name, location);

                case Global:
                    return new GlobalVariable(name, location);

                case Constant:
                    return new ConstantVariable(name, location);

                case Class:
                    return new ClassVariable(name, location);

                case Nil:
                    return parser.CannotAssignError("nil", location);
                
                case Self:
                    return parser.CannotAssignError("self", location);

                case True:
                    return parser.CannotAssignError("true", location);
                
                case False:
                    return parser.CannotAssignError("false", location);

                case File:
                    return parser.CannotAssignError("__FILE__", location);

                case Line:
                    return parser.CannotAssignError("__LINE__", location);
                
                case Encoding:
                    return parser.CannotAssignError("__ENCODING__", location);
            }
        
            return null;
        }
    }
}
