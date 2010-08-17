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
    public enum NodeTypes {
        // root:
        SourceUnitTree,

        // misc:
        BlockDefinition,
        BlockReference,
        Body,
        Maplet,
        Parameters,
        SplattedArgument,

        // definitions:
        ClassDefinition,
        ModuleDefinition,
        SingletonDefinition,
        MethodDefinition,
        LambdaDefinition,

        // expressions:
        AndExpression,
        ArrayConstructor,
        AssignmentExpression,
        AstNodeDescriptionExpression,
        BlockExpression,
        CaseExpression,
        ConditionalExpression,
        ConditionalJumpExpression,
        ErrorExpression,
        ForLoopExpression,
        HashConstructor,
        IfExpression,
        Literal,
        StringLiteral,
        SymbolLiteral,
        FileLiteral,
        EncodingExpression,
        MethodCall,
        MatchExpression,
        NotExpression,
        OrExpression,
        RangeExpression,
        RangeCondition,
        RegularExpression,
        RegularExpressionCondition,
        RegexMatchReference,
        RescueExpression,
        SelfReference,
        StringConstructor,
        SuperCall,
        UnlessExpression,
        WhileLoopExpression,
        YieldCall,

        // l-values:
        ArrayItemAccess,
        AttributeAccess,
        ClassVariable,
        CompoundLeftValue,
        ConstantVariable,
        GlobalVariable,
        InstanceVariable,
        LocalVariable,
        Placeholder,

        // assignments:
        MemberAssignmentExpression,
        ParallelAssignmentExpression,
        SimpleAssignmentExpression,

        // statements:
        AliasStatement,
        ConditionalStatement,
        ExpressionStatement,
        Finalizer,
        Initializer,
        UndefineStatement,

        // jump statements:
        BreakStatement,
        NextStatement,
        RedoStatement,
        RetryStatement,
        ReturnStatement,

        // clauses:
        RescueClause,
        WhenClause,
        ElseIfClause,
    }

    // TODO: generate:
    public partial class SourceUnitTree { public override NodeTypes NodeType { get { return NodeTypes.SourceUnitTree; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class BlockDefinition { public override NodeTypes NodeType { get { return NodeTypes.BlockDefinition; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class BlockReference { public override NodeTypes NodeType { get { return NodeTypes.BlockReference; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class Body { public override NodeTypes NodeType { get { return NodeTypes.Body; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class Maplet { public override NodeTypes NodeType { get { return NodeTypes.Maplet; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class Parameters { public override NodeTypes NodeType { get { return NodeTypes.Parameters; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class SplattedArgument { public override NodeTypes NodeType { get { return NodeTypes.SplattedArgument; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }

    public partial class ClassDefinition { public override NodeTypes NodeType { get { return NodeTypes.ClassDefinition; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class ModuleDefinition { public override NodeTypes NodeType { get { return NodeTypes.ModuleDefinition; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class SingletonDefinition { public override NodeTypes NodeType { get { return NodeTypes.SingletonDefinition; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class MethodDefinition { public override NodeTypes NodeType { get { return NodeTypes.MethodDefinition; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class LambdaDefinition { public override NodeTypes NodeType { get { return NodeTypes.LambdaDefinition; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }

    public partial class AndExpression { public override NodeTypes NodeType { get { return NodeTypes.AndExpression; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class ArrayConstructor { public override NodeTypes NodeType { get { return NodeTypes.ArrayConstructor; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class IsDefinedExpression { public override NodeTypes NodeType { get { return NodeTypes.AstNodeDescriptionExpression; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class BlockExpression { public override NodeTypes NodeType { get { return NodeTypes.BlockExpression; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class CaseExpression { public override NodeTypes NodeType { get { return NodeTypes.CaseExpression; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class ConditionalExpression { public override NodeTypes NodeType { get { return NodeTypes.ConditionalExpression; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class ConditionalJumpExpression { public override NodeTypes NodeType { get { return NodeTypes.ConditionalJumpExpression; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class ErrorExpression { public override NodeTypes NodeType { get { return NodeTypes.ErrorExpression; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class ForLoopExpression { public override NodeTypes NodeType { get { return NodeTypes.ForLoopExpression; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class HashConstructor { public override NodeTypes NodeType { get { return NodeTypes.HashConstructor; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class IfExpression { public override NodeTypes NodeType { get { return NodeTypes.IfExpression; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class Literal { public override NodeTypes NodeType { get { return NodeTypes.Literal; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class StringLiteral { public override NodeTypes NodeType { get { return NodeTypes.StringLiteral; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class SymbolLiteral { public override NodeTypes NodeType { get { return NodeTypes.SymbolLiteral; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class FileLiteral { public override NodeTypes NodeType { get { return NodeTypes.FileLiteral; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class EncodingExpression { public override NodeTypes NodeType { get { return NodeTypes.EncodingExpression; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class MethodCall { public override NodeTypes NodeType { get { return NodeTypes.MethodCall; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class MatchExpression { public override NodeTypes NodeType { get { return NodeTypes.MatchExpression; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class NotExpression { public override NodeTypes NodeType { get { return NodeTypes.NotExpression; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class OrExpression { public override NodeTypes NodeType { get { return NodeTypes.OrExpression; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class RangeExpression { public override NodeTypes NodeType { get { return NodeTypes.RangeExpression; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class RangeCondition { public override NodeTypes NodeType { get { return NodeTypes.RangeCondition; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class RegularExpression { public override NodeTypes NodeType { get { return NodeTypes.RegularExpression; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class RegularExpressionCondition { public override NodeTypes NodeType { get { return NodeTypes.RegularExpressionCondition; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class RegexMatchReference { public override NodeTypes NodeType { get { return NodeTypes.RegexMatchReference; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class RescueExpression { public override NodeTypes NodeType { get { return NodeTypes.RescueExpression; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class SelfReference { public override NodeTypes NodeType { get { return NodeTypes.SelfReference; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class StringConstructor { public override NodeTypes NodeType { get { return NodeTypes.StringConstructor; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class SuperCall { public override NodeTypes NodeType { get { return NodeTypes.SuperCall; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class UnlessExpression { public override NodeTypes NodeType { get { return NodeTypes.UnlessExpression; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class WhileLoopExpression { public override NodeTypes NodeType { get { return NodeTypes.WhileLoopExpression; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class YieldCall { public override NodeTypes NodeType { get { return NodeTypes.YieldCall; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }

    public partial class ArrayItemAccess { public override NodeTypes NodeType { get { return NodeTypes.ArrayItemAccess; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class AttributeAccess { public override NodeTypes NodeType { get { return NodeTypes.AttributeAccess; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class ClassVariable { public override NodeTypes NodeType { get { return NodeTypes.ClassVariable; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class CompoundLeftValue { public override NodeTypes NodeType { get { return NodeTypes.CompoundLeftValue; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class ConstantVariable { public override NodeTypes NodeType { get { return NodeTypes.ConstantVariable; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class GlobalVariable { public override NodeTypes NodeType { get { return NodeTypes.GlobalVariable; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class InstanceVariable { public override NodeTypes NodeType { get { return NodeTypes.InstanceVariable; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class LocalVariable { public override NodeTypes NodeType { get { return NodeTypes.LocalVariable; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class Placeholder { public override NodeTypes NodeType { get { return NodeTypes.Placeholder; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }

    public partial class MemberAssignmentExpression { public override NodeTypes NodeType { get { return NodeTypes.MemberAssignmentExpression; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class ParallelAssignmentExpression { public override NodeTypes NodeType { get { return NodeTypes.ParallelAssignmentExpression; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class SimpleAssignmentExpression { public override NodeTypes NodeType { get { return NodeTypes.SimpleAssignmentExpression; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }

    public partial class AliasStatement { public override NodeTypes NodeType { get { return NodeTypes.AliasStatement; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class ConditionalStatement { public override NodeTypes NodeType { get { return NodeTypes.ConditionalStatement; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class ShutdownHandlerStatement { public override NodeTypes NodeType { get { return NodeTypes.Finalizer; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class FileInitializerStatement { public override NodeTypes NodeType { get { return NodeTypes.Initializer; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class UndefineStatement { public override NodeTypes NodeType { get { return NodeTypes.UndefineStatement; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    
    public partial class BreakStatement { public override NodeTypes NodeType { get { return NodeTypes.BreakStatement; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class NextStatement { public override NodeTypes NodeType { get { return NodeTypes.NextStatement; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class RedoStatement { public override NodeTypes NodeType { get { return NodeTypes.RedoStatement; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class RetryStatement { public override NodeTypes NodeType { get { return NodeTypes.RetryStatement; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class ReturnStatement { public override NodeTypes NodeType { get { return NodeTypes.ReturnStatement; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }

    public partial class RescueClause { public override NodeTypes NodeType { get { return NodeTypes.RescueClause; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class WhenClause { public override NodeTypes NodeType { get { return NodeTypes.WhenClause; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
    public partial class ElseIfClause { public override NodeTypes NodeType { get { return NodeTypes.ElseIfClause; } } internal protected override void Walk(Walker/*!*/ walker) { walker.Walk(this); } }
}
