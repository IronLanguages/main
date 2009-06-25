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

using System;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Binding;

using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    public abstract class Expression : Node {
        internal abstract MSAst.Expression Transform(AstGenerator ag, Type type);

        internal virtual MSAst.Expression TransformSet(AstGenerator ag, SourceSpan span, MSAst.Expression right, PythonOperationKind op) {
            // unreachable, CheckAssign prevents us from calling this at parse time.
            throw new InvalidOperationException();
        }

        internal virtual MSAst.Expression TransformDelete(AstGenerator ag) {
            throw new InvalidOperationException();
        }

        internal virtual string CheckAssign() {
            return "can't assign to " + NodeName;
        }

        internal virtual string CheckAugmentedAssign() {
            if (CheckAssign() != null) {
                return "illegal expression for augmented assignment";
            }

            return null;
        }

        internal virtual string CheckDelete() {
            return "can't delete " + NodeName;
        }
    }
}
