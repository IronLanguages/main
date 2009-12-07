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

#if !CLR2
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;

    public partial class Parameters : Node {
        internal static readonly Parameters/*!*/ Empty = new Parameters(null, null, null, null, SourceSpan.None);

        private readonly List<LocalVariable> _mandatory;
        private readonly List<SimpleAssignmentExpression> _optional;
        private readonly LocalVariable _array;
        private readonly LocalVariable _block;

        public List<LocalVariable> Mandatory {
            get { return _mandatory; }
        }

        public List<SimpleAssignmentExpression> Optional {
            get { return _optional; }
        }

        public LocalVariable Array {
            get { return _array; }
        }

        public LocalVariable Block {
            get { return _block; }
        }

        public int MandatoryCount {
            get {
                return (_mandatory != null) ? _mandatory.Count : 0;
            }
        }

        public int OptionalCount {
            get {
                return (_optional != null) ? _optional.Count : 0;
            }
        }

        public Parameters(List<LocalVariable> mandatory, List<SimpleAssignmentExpression> optional, 
            LocalVariable array, LocalVariable block, SourceSpan location)
            : base(location) {

            _mandatory = mandatory;
            _optional = optional;
            _array = array;
            _block = block;
        }

        internal MSA.Expression/*!*/ TransformOptionalsInitialization(AstGenerator/*!*/ gen) {
            Assert.NotNull(gen);

            if (_optional == null) return AstUtils.Empty();

            MSA.Expression singleton = gen.CurrentScope.DefineHiddenVariable("#default", typeof(object));

            MSA.Expression result = AstUtils.Empty();
            for (int i = 0; i < _optional.Count; i++) {
                result = AstUtils.IfThen(Ast.Equal(_optional[i].Left.TransformRead(gen), singleton),
                    result,
                    _optional[i].TransformRead(gen)
                );
            }
            
            return Ast.Block(
                Ast.Assign(singleton, Ast.Field(null, Fields.DefaultArgument)),
                result,
                AstUtils.Empty()
            );
        }

        internal void TransformForSuperCall(AstGenerator/*!*/ gen, CallSiteBuilder/*!*/ siteBuilder) {
            if (_mandatory != null) {
                foreach (Variable v in _mandatory) {
                    siteBuilder.Add(v.TransformRead(gen));
                }
            }

            if (_optional != null) {
                foreach (SimpleAssignmentExpression s in _optional) {
                    siteBuilder.Add(s.Left.TransformRead(gen));
                }
            }

            if (_array != null) {
                siteBuilder.SplattedArgument = _array.TransformRead(gen);
            }
        }
    }
}