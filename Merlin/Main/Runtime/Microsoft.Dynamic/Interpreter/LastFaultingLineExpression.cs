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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.Scripting.Runtime;
using System.Threading;


namespace Microsoft.Scripting.Interpreter {
    public class LastFaultingLineExpression : Expression {
        private readonly Expression _lineNumberExpression;
        
        public LastFaultingLineExpression(Expression lineNumberExpression) {
            _lineNumberExpression = lineNumberExpression;
        }

        public sealed override ExpressionType NodeType {
            get { return ExpressionType.Extension; }
        }

        public sealed override Type Type {
            get { return typeof(int); }
        }

        public override bool CanReduce {
            get {
                return true;
            }
        }

        public override Expression/*!*/ Reduce() {
            return _lineNumberExpression;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor) {
            Expression lineNo = visitor.Visit(_lineNumberExpression);
            if (lineNo != _lineNumberExpression) {
                return new LastFaultingLineExpression(lineNo);
            }

            return this;
        }
    }

    internal sealed class UpdateStackTraceInstruction : Instruction {
        internal DebugInfo[] _debugInfos;

        public override int ProducedStack {
            get {
                return 1;
            }
        }

        public override int Run(InterpretedFrame frame) {
            DebugInfo info = DebugInfo.GetMatchingDebugInfo(_debugInfos, frame.FaultingInstruction);
            if (info != null && !info.IsClear) {
                frame.Push(info.StartLine);
            }else{
                frame.Push(-1);
            }

            return +1;
        }
    }
}