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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// Provides a simple expression which enables embedding FieldBuilder's
    /// in an AST before the type is complete.
    /// </summary>
    public class FieldBuilderExpression : Expression {
        private readonly FieldBuilder _builder;

#if SILVERLIGHT
        private readonly StrongBox<Type> _finishedType;

        // Silverlight doesn't have ModuleInfo.ResolveField so we need to
        // get something which can be updated w/ the final type instead.
        public FieldBuilderExpression(FieldBuilder builder, StrongBox<Type> finishedType) {
            _builder = builder;
            _finishedType = finishedType;
        }
#else
        public FieldBuilderExpression(FieldBuilder builder) {
            _builder = builder;
        }
#endif

        public override bool CanReduce {
            get {
                return true;
            }
        }

        public sealed override ExpressionType NodeType {
            get { return ExpressionType.Extension; }
        }

        public sealed override Type Type {
            get { return _builder.FieldType; }
        }

        public override Expression Reduce() {
            FieldInfo fi = GetFieldInfo();
            Debug.Assert(fi.Name == _builder.Name);
            return Expression.Field(
                null,
                fi
            );
        }

        private FieldInfo GetFieldInfo() {
            // turn the field builder back into a FieldInfo
#if SILVERLIGHT
            return _finishedType.Value.GetField(_builder.Name);
#else
            return _builder.DeclaringType.Module.ResolveField(
                _builder.GetToken().Token
            );
#endif
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor) {
            return this;
        }
    }
}
