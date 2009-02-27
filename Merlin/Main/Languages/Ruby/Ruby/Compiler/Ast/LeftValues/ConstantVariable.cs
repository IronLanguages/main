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

using System.Diagnostics;
using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using MSA = System.Linq.Expressions;
    using Ast = System.Linq.Expressions.Expression;
    using System;
    
    internal enum StaticScopeKind {
        Global,
        EnclosingModule,
        Explicit
    }

    public partial class ConstantVariable : Variable {
        private readonly bool _explicitlyBound;
        private readonly Expression _qualifier;

        public Expression Qualifier {
            get { return _qualifier; }
        }

        public bool IsGlobal {
            get {
                return _explicitlyBound && _qualifier == null;
            }
        }

        public bool IsBound {
            get {
                return _explicitlyBound;
            }
        }

        /// <summary>
        /// Unbound constant (Foo).
        /// </summary>
        public ConstantVariable(string/*!*/ name, SourceSpan location)
            : base(name, location) {

            _qualifier = null;
            _explicitlyBound = false;
        }

        /// <summary>
        /// Bound constant (::Foo - bound to Object, qualifier.Foo - bound to qualifier object).
        /// </summary>
        public ConstantVariable(Expression qualifier, string/*!*/ name, SourceSpan location)
            : base(name, location) {

            _qualifier = qualifier;
            _explicitlyBound = true;
        }

        internal StaticScopeKind TransformQualifier(AstGenerator/*!*/ gen, out MSA.Expression transformedQualifier) {
            if (_qualifier != null) {
                Debug.Assert(_explicitlyBound);

                // qualifier.Foo
                transformedQualifier = _qualifier.TransformRead(gen);
                return StaticScopeKind.Explicit;
            } else if (_explicitlyBound) {
                // ::Foo
                transformedQualifier = null;
                return StaticScopeKind.Global;
            } else if (gen.CurrentModule != null) {
                // statically (lexically) implicitly bound to the enclosing module:
                transformedQualifier = gen.CurrentModule.SelfVariable; // TODO: remove, should be retrieved from code context/scope
                return StaticScopeKind.EnclosingModule;
            } else {
                // statically (lexically) implicitly bound to top declaring module:
                transformedQualifier = null;
                return StaticScopeKind.EnclosingModule;
            }
        }

        internal override MSA.Expression/*!*/ TransformReadVariable(AstGenerator/*!*/ gen, bool tryRead) {
            return TransformRead(gen, OpGet);
        }

        private const int OpGet = 0;
        private const int OpIsDefined = 1;

        private MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen, int/*!*/ opKind) {
            MSA.Expression transformedName = TransformName(gen);
            MSA.Expression transformedQualifier;

            switch (TransformQualifier(gen, out transformedQualifier)) {
                case StaticScopeKind.Global:
                    return (opKind == OpGet ? Methods.GetGlobalConstant : Methods.IsDefinedGlobalConstant).
                        OpCall(gen.CurrentScopeVariable, transformedName);

                case StaticScopeKind.EnclosingModule:
                    return (opKind == OpGet ? Methods.GetUnqualifiedConstant : Methods.IsDefinedUnqualifiedConstant).
                        OpCall(gen.CurrentScopeVariable, transformedName);

                case StaticScopeKind.Explicit:
                    if (opKind == OpGet) {
                        return Methods.GetQualifiedConstant.OpCall(AstFactory.Box(transformedQualifier), gen.CurrentScopeVariable, transformedName);
                    } else {
                        return gen.TryCatchAny(
                            Methods.IsDefinedQualifiedConstant.OpCall(AstFactory.Box(transformedQualifier), gen.CurrentScopeVariable, transformedName), 
                            AstUtils.Constant(false)
                        );
                    }
            }

            throw Assert.Unreachable;
        }

        internal override MSA.Expression/*!*/ TransformWriteVariable(AstGenerator/*!*/ gen, MSA.Expression/*!*/ rightValue) {
            MSA.Expression transformedName = TransformName(gen);
            MSA.Expression transformedQualifier;

            switch (TransformQualifier(gen, out transformedQualifier)) {
                case StaticScopeKind.Global:
                    return Methods.SetGlobalConstant.OpCall(AstFactory.Box(rightValue), gen.CurrentScopeVariable, transformedName);

                case StaticScopeKind.EnclosingModule:
                    return Methods.SetUnqualifiedConstant.OpCall(AstFactory.Box(rightValue), gen.CurrentScopeVariable, transformedName);

                case StaticScopeKind.Explicit:
                    return Methods.SetQualifiedConstant.OpCall(AstFactory.Box(rightValue), transformedQualifier, gen.CurrentScopeVariable, transformedName);
            }

            throw Assert.Unreachable;
        }

        internal override MSA.Expression TransformDefinedCondition(AstGenerator/*!*/ gen) {
            return TransformRead(gen, OpIsDefined);
        }

        internal override string/*!*/ GetNodeName(AstGenerator/*!*/ gen) {
            return "constant";
        }
    }
}
