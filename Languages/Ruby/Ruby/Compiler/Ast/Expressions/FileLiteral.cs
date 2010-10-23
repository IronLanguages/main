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

using System.Diagnostics;
using Microsoft.Scripting;
using IronRuby.Builtins;

namespace IronRuby.Compiler.Ast {
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    /// <summary>
    /// Represents __FILE__ literal encoded by the containing source file encoding.
    /// </summary>
    public partial class FileLiteral : Expression {
        internal FileLiteral(SourceSpan location)
            : base(location) {
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return Methods.CreateMutableStringL.OpCall(
                gen.SourcePathConstant, AstUtils.Constant(gen.Context.GetPathEncoding())
            );
        }
    }
}
