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

using System.Diagnostics;
using System.Linq.Expressions;

namespace Microsoft.Scripting.Interpretation {
    // Singleton objects of this enum type are used as return values from Statement.Execute() to handle control flow.
    enum ControlFlowKind {
        NextStatement,
        Goto,
        Yield,
        NextForYield
    };

    sealed class ControlFlow {
        internal readonly ControlFlowKind Kind;
        internal readonly LabelTarget Label;
        internal readonly object Value;

        private ControlFlow(ControlFlowKind kind)
            : this(kind, null, null) {
        }

        private ControlFlow(ControlFlowKind kind, LabelTarget label, object value) {
            Kind = kind;
            Label = label;
            Value = value;
        }

        internal static ControlFlow YieldReturn(object value) {
            Debug.Assert(!(value is ControlFlow));

            return new ControlFlow(ControlFlowKind.Yield, null, value);
        }

        internal static ControlFlow Goto(LabelTarget label, object value) {
            return new ControlFlow(ControlFlowKind.Goto, label, value);
        }

        // Hold on to one instance for each member of the ControlFlow enumeration to avoid unnecessary allocation
        internal static readonly ControlFlow NextStatement = new ControlFlow(ControlFlowKind.NextStatement);
        internal static readonly ControlFlow NextForYield = new ControlFlow(ControlFlowKind.NextForYield);
        internal static readonly ControlFlow YieldBreak = new ControlFlow(ControlFlowKind.Yield);
    }
}
