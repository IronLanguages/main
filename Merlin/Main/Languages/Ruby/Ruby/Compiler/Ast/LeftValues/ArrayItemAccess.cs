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

using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast {

    // {array}[{arguments}]
    // {array}[{arguments}] = rhs
    public partial class ArrayItemAccess : LeftValue {
        private readonly Expression/*!*/ _array;
        private readonly Arguments/*!*/ _arguments;

        public Expression/*!*/ Array {
            get { return _array; }
        }

        public Arguments/*!*/ Arguments {
            get { return _arguments; }
        }

        public ArrayItemAccess(Expression/*!*/ array, Arguments/*!*/ arguments, SourceSpan location)
            : base(location) {
            ContractUtils.RequiresNotNull(array, "array");
            ContractUtils.RequiresNotNull(arguments, "arguments");

            _array = array;
            _arguments = arguments;
        }

        internal override MSA.Expression TransformTargetRead(AstGenerator/*!*/ gen) {
            return _array.TransformRead(gen);
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen, MSA.Expression targetValue, bool tryRead) {
            Assert.NotNull(gen, targetValue);
            return MethodCall.TransformRead(this, gen, false, "[]", targetValue, _arguments, null, null, null);
        }

        internal override MSA.Expression/*!*/ TransformWrite(AstGenerator/*!*/ gen, MSA.Expression target, MSA.Expression/*!*/ rightValue) {
            Assert.NotNull(target);
            return MethodCall.TransformRead(this, gen, _array.NodeType == NodeTypes.SelfReference, "[]=", target, _arguments, null, null, rightValue);
        }
    }

}
