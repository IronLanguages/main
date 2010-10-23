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

using System;
using System.Collections.Generic;
using System.Text;

namespace IronRuby.Compiler.Ast {
    public partial class Walker {
        // TODO: generate
        // root:
        public virtual bool Enter(SourceUnitTree/*!*/ node) { return true; }
        public virtual void Exit(SourceUnitTree/*!*/ node) { }

        // misc:
        public virtual bool Enter(BlockDefinition/*!*/ node) { return true; }
        public virtual void Exit(BlockDefinition/*!*/ node) { }
        public virtual bool Enter(BlockReference/*!*/ node) { return true; }
        public virtual void Exit(BlockReference/*!*/ node) { }
        public virtual bool Enter(Body/*!*/ node) { return true; }
        public virtual void Exit(Body/*!*/ node) { }
        public virtual bool Enter(Maplet/*!*/ node) { return true; }
        public virtual void Exit(Maplet/*!*/ node) { }
        public virtual bool Enter(Parameters/*!*/ node) { return true; }
        public virtual void Exit(Parameters/*!*/ node) { }
        public virtual bool Enter(Arguments/*!*/ node) { return true; }
        public virtual void Exit(Arguments/*!*/ node) { }
        public virtual bool Enter(SplattedArgument/*!*/ node) { return true; }
        public virtual void Exit(SplattedArgument/*!*/ node) { }

        // declarations:
        public virtual bool Enter(ClassDefinition/*!*/ node) { return true; }
        public virtual void Exit(ClassDefinition/*!*/ node) { }
        public virtual bool Enter(ModuleDefinition/*!*/ node) { return true; }
        public virtual void Exit(ModuleDefinition/*!*/ node) { }
        public virtual bool Enter(SingletonDefinition/*!*/ node) { return true; }
        public virtual void Exit(SingletonDefinition/*!*/ node) { }
        public virtual bool Enter(MethodDefinition/*!*/ node) { return true; }
        public virtual void Exit(MethodDefinition/*!*/ node) { }
        public virtual bool Enter(LambdaDefinition/*!*/ node) { return true; }
        public virtual void Exit(LambdaDefinition/*!*/ node) { }

        // expressions:
        public virtual bool Enter(AndExpression/*!*/ node) { return true; }
        public virtual void Exit(AndExpression/*!*/ node) { }
        public virtual bool Enter(ArrayConstructor/*!*/ node) { return true; }
        public virtual void Exit(ArrayConstructor/*!*/ node) { }
        public virtual bool Enter(AssignmentExpression/*!*/ node) { return true; }
        public virtual void Exit(AssignmentExpression/*!*/ node) { }
        public virtual bool Enter(IsDefinedExpression/*!*/ node) { return true; }
        public virtual void Exit(IsDefinedExpression/*!*/ node) { }
        public virtual bool Enter(BlockExpression/*!*/ node) { return true; }
        public virtual void Exit(BlockExpression/*!*/ node) { }
        public virtual bool Enter(CaseExpression/*!*/ node) { return true; }
        public virtual void Exit(CaseExpression/*!*/ node) { }
        public virtual bool Enter(ConditionalExpression/*!*/ node) { return true; }
        public virtual void Exit(ConditionalExpression/*!*/ node) { }
        public virtual bool Enter(ConditionalJumpExpression/*!*/ node) { return true; }
        public virtual void Exit(ConditionalJumpExpression/*!*/ node) { }
        public virtual bool Enter(ErrorExpression/*!*/ node) { return true; }
        public virtual void Exit(ErrorExpression/*!*/ node) { }
        public virtual bool Enter(ForLoopExpression/*!*/ node) { return true; }
        public virtual void Exit(ForLoopExpression/*!*/ node) { }
        public virtual bool Enter(HashConstructor/*!*/ node) { return true; }
        public virtual void Exit(HashConstructor/*!*/ node) { }
        public virtual bool Enter(IfExpression/*!*/ node) { return true; }
        public virtual void Exit(IfExpression/*!*/ node) { }
        public virtual bool Enter(Literal/*!*/ node) { return true; }
        public virtual void Exit(Literal/*!*/ node) { }
        public virtual bool Enter(StringLiteral/*!*/ node) { return true; }
        public virtual void Exit(StringLiteral/*!*/ node) { }
        public virtual bool Enter(SymbolLiteral/*!*/ node) { return true; }
        public virtual void Exit(SymbolLiteral/*!*/ node) { }
        public virtual bool Enter(FileLiteral/*!*/ node) { return true; }
        public virtual void Exit(FileLiteral/*!*/ node) { }
        public virtual bool Enter(EncodingExpression/*!*/ node) { return true; }
        public virtual void Exit(EncodingExpression/*!*/ node) { }
        public virtual bool Enter(MethodCall/*!*/ node) { return true; }
        public virtual void Exit(MethodCall/*!*/ node) { }
        public virtual bool Enter(MatchExpression/*!*/ node) { return true; }
        public virtual void Exit(MatchExpression/*!*/ node) { }
        public virtual bool Enter(NotExpression/*!*/ node) { return true; }
        public virtual void Exit(NotExpression/*!*/ node) { }
        public virtual bool Enter(OrExpression/*!*/ node) { return true; }
        public virtual void Exit(OrExpression/*!*/ node) { }
        public virtual bool Enter(RangeExpression/*!*/ node) { return true; }
        public virtual void Exit(RangeExpression/*!*/ node) { }
        public virtual bool Enter(RangeCondition/*!*/ node) { return true; }
        public virtual void Exit(RangeCondition/*!*/ node) { }
        public virtual bool Enter(RegularExpression/*!*/ node) { return true; }
        public virtual void Exit(RegularExpression/*!*/ node) { }
        public virtual bool Enter(RegularExpressionCondition/*!*/ node) { return true; }
        public virtual void Exit(RegularExpressionCondition/*!*/ node) { }
        public virtual bool Enter(RescueExpression/*!*/ node) { return true; }
        public virtual void Exit(RescueExpression/*!*/ node) { }
        public virtual bool Enter(SelfReference/*!*/ node) { return true; }
        public virtual void Exit(SelfReference/*!*/ node) { }
        public virtual bool Enter(StringConstructor/*!*/ node) { return true; }
        public virtual void Exit(StringConstructor/*!*/ node) { }
        public virtual bool Enter(SuperCall/*!*/ node) { return true; }
        public virtual void Exit(SuperCall/*!*/ node) { }
        public virtual bool Enter(UnlessExpression/*!*/ node) { return true; }
        public virtual void Exit(UnlessExpression/*!*/ node) { }
        public virtual bool Enter(WhileLoopExpression/*!*/ node) { return true; }
        public virtual void Exit(WhileLoopExpression/*!*/ node) { }
        public virtual bool Enter(YieldCall/*!*/ node) { return true; }
        public virtual void Exit(YieldCall/*!*/ node) { }

        // l-values:
        public virtual bool Enter(ArrayItemAccess/*!*/ node) { return true; }
        public virtual void Exit(ArrayItemAccess/*!*/ node) { }
        public virtual bool Enter(AttributeAccess/*!*/ node) { return true; }
        public virtual void Exit(AttributeAccess/*!*/ node) { }
        public virtual bool Enter(ClassVariable/*!*/ node) { return true; }
        public virtual void Exit(ClassVariable/*!*/ node) { }
        public virtual bool Enter(CompoundLeftValue/*!*/ node) { return true; }
        public virtual void Exit(CompoundLeftValue/*!*/ node) { }
        public virtual bool Enter(ConstantVariable/*!*/ node) { return true; }
        public virtual void Exit(ConstantVariable/*!*/ node) { }
        public virtual bool Enter(GlobalVariable/*!*/ node) { return true; }
        public virtual void Exit(GlobalVariable/*!*/ node) { }
        public virtual bool Enter(InstanceVariable/*!*/ node) { return true; }
        public virtual void Exit(InstanceVariable/*!*/ node) { }
        public virtual bool Enter(LocalVariable/*!*/ node) { return true; }
        public virtual void Exit(LocalVariable/*!*/ node) { }
        public virtual bool Enter(Placeholder/*!*/ node) { return true; }
        public virtual void Exit(Placeholder/*!*/ node) { }
        public virtual bool Enter(RegexMatchReference/*!*/ node) { return true; }
        public virtual void Exit(RegexMatchReference/*!*/ node) { }
        
        // assignments:
        public virtual bool Enter(MemberAssignmentExpression/*!*/ node) { return true; }
        public virtual void Exit(MemberAssignmentExpression/*!*/ node) { }
        public virtual bool Enter(ParallelAssignmentExpression/*!*/ node) { return true; }
        public virtual void Exit(ParallelAssignmentExpression/*!*/ node) { }
        public virtual bool Enter(SimpleAssignmentExpression/*!*/ node) { return true; }
        public virtual void Exit(SimpleAssignmentExpression/*!*/ node) { }

        // statements:
        public virtual bool Enter(AliasStatement/*!*/ node) { return true; }
        public virtual void Exit(AliasStatement/*!*/ node) { }
        public virtual bool Enter(ConditionalStatement/*!*/ node) { return true; }
        public virtual void Exit(ConditionalStatement/*!*/ node) { }
        public virtual bool Enter(ShutdownHandlerStatement/*!*/ node) { return true; }
        public virtual void Exit(ShutdownHandlerStatement/*!*/ node) { }
        public virtual bool Enter(FileInitializerStatement/*!*/ node) { return true; }
        public virtual void Exit(FileInitializerStatement/*!*/ node) { }
        public virtual bool Enter(UndefineStatement/*!*/ node) { return true; }
        public virtual void Exit(UndefineStatement/*!*/ node) { }
        
        // jump statements:
        public virtual bool Enter(BreakStatement/*!*/ node) { return true; }
        public virtual void Exit(BreakStatement/*!*/ node) { }
        public virtual bool Enter(NextStatement/*!*/ node) { return true; }
        public virtual void Exit(NextStatement/*!*/ node) { }
        public virtual bool Enter(RedoStatement/*!*/ node) { return true; }
        public virtual void Exit(RedoStatement/*!*/ node) { }
        public virtual bool Enter(RetryStatement/*!*/ node) { return true; }
        public virtual void Exit(RetryStatement/*!*/ node) { }
        public virtual bool Enter(ReturnStatement/*!*/ node) { return true; }
        public virtual void Exit(ReturnStatement/*!*/ node) { }

        // clauses:
        public virtual bool Enter(RescueClause/*!*/ node) { return true; }
        public virtual void Exit(RescueClause/*!*/ node) { }
        public virtual bool Enter(WhenClause/*!*/ node) { return true; }
        public virtual void Exit(WhenClause/*!*/ node) { }
        public virtual bool Enter(ElseIfClause/*!*/ node) { return true; }
        public virtual void Exit(ElseIfClause/*!*/ node) { }
    }
}
