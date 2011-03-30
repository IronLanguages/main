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
using System.Diagnostics;
using System.Runtime.InteropServices;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Compiler.Ast;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.StandardLibrary.ParseTree {
    [RubyModule("IronRuby", Extends = typeof(Ruby))]
    public static class IronRubyOps {

        [RubyModule("ParseTree")]
        public static class ParseTreeOps {

            [RubyMethod("parse_tree_for_meth")]
            public static RubyArray/*!*/ CreateParseTreeForMethod(object self,
                [NotNull]RubyModule/*!*/ module, [DefaultProtocol, NotNull]string/*!*/ methodName, bool isClassMethod) {

                // TODO:
                // bool includeNewLines = IncludeNewLines(module.Context, self);

                if (isClassMethod) {
                    module = module.ImmediateClass;
                }

                var member = module.GetMethod(methodName);

                // TODO: aliases, module_functions, define_methods, method witch changed visibility:
                var method = member as RubyMethodInfo;
                if (method == null) {
                    return RubyArray.Create(null);
                }

                var visitor = new AstVisitor(module.Context, GetNodeNames(module.Context, self), false);
                visitor.Walk(method.GetSyntaxTree());
                
                return visitor.Result;
            }

            [RubyMethod("parse_tree_for_str")]
            public static RubyArray/*!*/ CreateParseTreeForString(RubyScope/*!*/ scope, object self, 
                [NotNull]MutableString/*!*/ code, [Optional, NotNull]MutableString/*!*/ file, int line) {

                SourceUnit source = scope.RubyContext.CreateSnippet(
                    code.ConvertToString(),
                    file != null ? file.ConvertToString() : null,
                    SourceCodeKind.Statements
                );

                var options = RubyUtils.CreateCompilerOptionsForEval(scope, line);

                SourceUnitTree ast = new Parser().Parse(source, options, scope.RubyContext.RuntimeErrorSink);
                // TODO:
                // bool includeNewLines = IncludeNewLines(scope.RubyContext, self);
                var visitor = new AstVisitor(scope.RubyContext, GetNodeNames(scope.RubyContext, self), false);
                visitor.Walk(ast);
                return visitor.Result;
            }

#if TODO
            private static bool IncludeNewLines(RubyContext/*!*/ context, object self) {
                object value;
                if (context.TryGetInstanceVariable(self, "@include_newlines", out value)) {
                    return Protocols.IsTrue(value);
                }
                return false;
            }
#endif
            private static RubyArray/*!*/ GetNodeNames(RubyContext/*!*/ context, object self) {
                object value;
                context.GetClassOf(self).TryGetConstant(null, "NODE_NAMES", out value);
                return value as RubyArray ?? new RubyArray();
            }

            private enum NodeKind {
                // 00
                Method, fbody, cfunc, scope, block,
                @if, @case, when, opt_n, @while,
                // 10
                until, iter, @for, @break, next,
                redo, retry, begin, rescue, resbody,
                //  20
                ensure, and, or, not, masgn,
                lasgn, dasgn, dasgn_curr, gasgn, iasgn,
                //  30
                cdecl, cvasgn, cvdecl, op_asgn1, op_asgn2,
                op_asgn_and, op_asgn_or, call, fcall, vcall,
                //  40
                super, zsuper, array, zarray, hash,
                @return, yield, lvar, dvar, gvar,
                //  50
                ivar, @const, cvar, nth_ref, back_ref,
                match, match2, match3, lit, str,
                //  60
                dstr, xstr, dxstr, evstr, dregx,
                dregx_once, args, argscat, argspush, splat,
                //  70
                to_ary, svalue, block_arg, block_pass, defn,
                defs, alias, valias, undef, @class,
                //  80
                module, sclass, colon2, colon3, cref,
                dot2, dot3, flip2, flip3, attrset,
                //  90
                self, nil, @true, @false, defined,
                //  95
                newline, postexe, alloca, dmethod, bmethod,
                // 100
                memo, ifunc, dsym, attrasgn,
                last
            }

            private sealed class Rhs {
                public object Value { get; set; }
                public bool InBlockParameters { get; set; }
                public bool InCompoundLhs { get; set; }
                public bool InTopCompoundLhs { get; set; }
                public bool IsRhsArg { get; set; }
            }

            private sealed class AstVisitor : Walker {
                private static readonly Rhs BlockRhs = new Rhs { InBlockParameters = true };
                private static readonly object Skip = new object();
                private object _result;
                private RubyEncoding _encoding;
                
                // null -> no rhs
                private Rhs _rhs;

                private readonly RubyContext/*!*/ _context;
                private readonly RubyArray/*!*/ _nodeNames;
                private bool _isMethodAlias;

                public AstVisitor(RubyContext/*!*/ context, RubyArray/*!*/ nodeNames, bool isMethodAlias) {
                    Assert.NotNull(nodeNames);
                    _nodeNames = nodeNames;
                    _isMethodAlias = isMethodAlias;
                    _context = context;
                }

                #region Helpers

                private RubySymbol/*!*/ CreateSymbol(string/*!*/ identifier) {
                    return _context.CreateSymbol(identifier, _encoding);
                }

                private object GetNodeName(NodeKind nodeKind) {
                    int index = (int)nodeKind;
                    return (index < _nodeNames.Count) ? _nodeNames[index] : null;
                }

                private RubyArray/*!*/ MakeNode(NodeKind nodeKind, int valueCount) {
                    var node = new RubyArray(1 + valueCount);
                    node.Add(GetNodeName(nodeKind));
                    return node;
                }

                private RubyArray/*!*/ MakeNode(NodeKind nodeKind) {
                    var node = MakeNode(nodeKind, 0);
                    return node;
                }

                private RubyArray/*!*/ MakeNode(NodeKind nodeKind, object value1) {
                    var node = MakeNode(nodeKind, 1);
                    node.Add(value1);
                    return node;
                }

                private RubyArray/*!*/ MakeNode(NodeKind nodeKind, object value1, object value2) {
                    var node = MakeNode(nodeKind, 2);
                    node.Add(value1);
                    node.Add(value2);
                    return node;
                }

                private RubyArray/*!*/ MakeNode(NodeKind nodeKind, object value1, object value2, object value3) {
                    var node = MakeNode(nodeKind, 3);
                    node.Add(value1);
                    node.Add(value2);
                    node.Add(value3);
                    return node;
                }

                public RubyArray/*!*/ Result { 
                    get { 
                        return (RubyArray)_result; 
                    }
                }

                private bool TryGetRhsValue(out object value) {
                    if (_rhs != null && !_rhs.InBlockParameters && !_rhs.InCompoundLhs) {
                        value = _rhs.Value;
                        return true;
                    } else {
                        value = null;
                        return false;
                    }
                }

                private RubyArray/*!*/ AddRange<T>(RubyArray/*!*/ list, IEnumerable<T> nodes) where T : Node {
                    if (nodes != null) {
                        foreach (T node in nodes) {
                            Walk(node);
                            if (_result != Skip) {
                                list.Add(_result);
                            }
                        }
                    }
                    return list;
                }

                private void UsingRhs(Rhs rhs, Action/*!*/ region) {
                    var oldRhs = _rhs;
                    _rhs = rhs;
                    region();
                    _rhs = oldRhs;
                }

                #endregion

                #region SourceUnitTree

                public override bool Enter(SourceUnitTree/*!*/ node) {
                    _encoding = node.Encoding;

                    if (node.Statements == null || node.Statements.Count == 0) {
                        _result = new RubyArray();
                    } else if (node.Statements.Count == 1) {
                        Walk(node.Statements.First);
                        _result = RubyArray.Create(_result);
                    } else {
                        _result = RubyArray.Create(AddRange(MakeNode(NodeKind.block, node.Statements.Count), node.Statements));
                    }

                    return false;
                }

                #endregion

                #region Literals

                public override bool Enter(Literal/*!*/ node) {
                    if (node.Value == null) {
                        _result = MakeNode(NodeKind.nil);
                    } else if (node.Value is bool) {
                        _result = MakeNode((bool)node.Value ? NodeKind.@true : NodeKind.@false);
                    } else {
                        _result = MakeNode(NodeKind.lit, node.Value);
                    }
                    return false;
                }

                private bool Enter(RangeExpression/*!*/ node, bool isCondition) {
                    Literal litBegin = node.Begin as Literal;
                    Literal litEnd = node.End as Literal;

                    if (!isCondition && litBegin != null && litEnd != null && litBegin.Value is int && litBegin.Value is int) {
                        _result = MakeNode(NodeKind.lit, new Range((int)litBegin.Value, (int)litEnd.Value, node.IsExclusive));
                    } else {
                        var range = MakeNode(isCondition ? 
                            (node.IsExclusive ? NodeKind.flip3 : NodeKind.flip2) :
                            (node.IsExclusive ? NodeKind.dot3 : NodeKind.dot2), 2
                        );

                        Walk(node.Begin);
                        range.Add(_result);

                        Walk(node.End);
                        range.Add(_result);
                        _result = range;
                    }
                    return false;
                }

                public override bool Enter(RangeExpression/*!*/ node) {
                    return Enter(node, false);
                }

                public override bool Enter(RangeCondition/*!*/ node) {
                    return Enter(node.Range, true);
                }

                public override bool Enter(StringLiteral/*!*/ node) {
                    _result = MakeNode(NodeKind.str, node.GetMutableString());
                    return false;
                }

                public override bool Enter(SymbolLiteral/*!*/ node) {
                    _result = MakeNode(NodeKind.lit, _context.CreateSymbol(node.GetMutableString()));
                    return false;
                }

                public override bool Enter(FileLiteral/*!*/ node) {
                    // TODO:
                    _result = MakeNode(NodeKind.lit, CreateSymbol("__FILE__"));
                    return false;
                }

                public override bool Enter(StringConstructor/*!*/ node) {
                    StringLiteral lit;
                    if (node.Parts.Count == 1 && (lit = node.Parts[0] as StringLiteral) != null) {
                        NodeKind kind;
                        object value;
                        switch (node.Kind) {
                            case StringKind.Symbol: kind = NodeKind.lit; value = _context.CreateSymbol(lit.GetMutableString()); break;
                            case StringKind.Command: kind = NodeKind.xstr; value = lit.GetMutableString(); break;
                            case StringKind.Mutable: kind = NodeKind.str; value = lit.GetMutableString(); break;
                            default: throw Assert.Unreachable;
                        }

                        _result = MakeNode(kind, value);
                    } else {
                        NodeKind kind;
                        switch (node.Kind) {
                            case StringKind.Command: kind = NodeKind.dxstr; break;
                            case StringKind.Symbol: kind = NodeKind.dsym; break;
                            case StringKind.Mutable: kind = NodeKind.dstr; break;
                            default: throw Assert.Unreachable;
                        }

                        _result = VisitStringConstructor(node.Parts, kind);
                    }

                    return false;
                }

                private RubyArray/*!*/ VisitStringConstructor(List<Expression>/*!*/ parts, NodeKind kind) {
                    StringLiteral lit;
                    var str = MakeNode(kind, parts.Count);

                    if (parts.Count == 1) {
                        str.Add(MutableString.FrozenEmpty);
                    }

                    for (int i = 0; i < parts.Count; i++) {
                        var part = parts[i];
                        lit = part as StringLiteral;
                        if (lit != null) {
                            object value = lit.GetMutableString();
                            if (i > 0) {
                                value = MakeNode(NodeKind.str, value);
                            }
                            str.Add(value);
                        } else {
                            Walk(part);
                            str.Add(MakeNode(NodeKind.evstr, _result));
                        }
                    }

                    return str;
                }

                public override bool Enter(RegularExpression/*!*/ node) {
                    StringLiteral lit;
                    if (node.Pattern.Count == 0) {
                        _result = MakeNode(NodeKind.lit, new RubyRegex(MutableString.CreateEmpty(), node.Options));
                    } else if (node.Pattern.Count == 1 && (lit = node.Pattern[0] as StringLiteral) != null) {
                        _result = MakeNode(NodeKind.lit, new RubyRegex(lit.GetMutableString(), node.Options));
                    } else {
                        var regex = VisitStringConstructor(node.Pattern, NodeKind.dregx);
                        if (node.Options != RubyRegexOptions.NONE) {
                            regex.Add((int)node.Options);
                        }
                        _result = regex;
                    }

                    return false;
                }

                public override bool Enter(RegularExpressionCondition/*!*/ node) {
                    Walk(node.RegularExpression);
                    _result = MakeNode(NodeKind.match, _result);
                    return false;
                }
                
                public override bool Enter(MatchExpression/*!*/ node) {
                    var match = MakeNode(NodeKind.match2, 2);
                    
                    Walk(node.Regex);
                    match.Add(_result);

                    Walk(node.Expression);
                    match.Add(_result);

                    _result = match;
                    return false;
                }

                public override bool Enter(HashConstructor/*!*/ node) {
                    _result = MakeHash(node.Maplets);
                    return false;
                }

                private RubyArray/*!*/ MakeHash(IList<Maplet>/*!*/ maplets) {
                    var hash = MakeNode(NodeKind.hash, maplets.Count * 2);
                    foreach (var maplet in maplets) {
                        Walk(maplet.Key);
                        hash.Add(_result);

                        Walk(maplet.Value);
                        hash.Add(_result);
                    }
                    return hash;
                }

                public override bool Enter(ArrayConstructor/*!*/ node) {
                    if (node.Arguments.IsEmpty) {
                        _result = MakeNode(NodeKind.zarray);
                    } else {
                        Walk(node.Arguments);
                        if (_result == Skip) {
                            _result = MakeNode(NodeKind.zarray);
                        }
                    }
                    return false;
                }

                #endregion

                #region CallExpressions

                public override bool Enter(MethodCall/*!*/ node) {
                    RubyArray call;
                    if (node.Target != null) {
                        call = MakeNode(NodeKind.call, 2 + SizeOf(node.Arguments));
                    } else if (node.Arguments != null) {
                        call = MakeNode(NodeKind.fcall, 1 + SizeOf(node.Arguments));
                    } else {
                        call = MakeNode(NodeKind.vcall, 1);
                    }

                    // add target:
                    if (node.Target != null) {
                        Walk(node.Target);
                        call.Add(_result);
                    }

                    // add name:
                    call.Add(CreateSymbol(node.MethodName));
                    
                    // add arguments:
                    AddArguments(call, node.Arguments);

                    _result = MakeCallWithBlock(node.Block, call);
                    return false;
                }
                
                public override bool Enter(SuperCall/*!*/ node) {
                    RubyArray call;

                    if (node.Arguments != null) {
                        call = MakeNode(NodeKind.super, SizeOf(node.Arguments));

                        // add arguments:
                        AddArguments(call, node.Arguments);
                    } else {
                        call = MakeNode(NodeKind.zsuper);
                    }

                    _result = MakeCallWithBlock(node.Block, call);
                    return false;
                }

                public override bool Enter(YieldCall/*!*/ node) {
                    var call = MakeNode(NodeKind.yield, SizeOf(node.Arguments));

                    // add arguments:
                    AddArguments(call, node.Arguments); // TODO: splat [:array, value]

                    _result = call;
                    return false;
                }

                private static int SizeOf(Arguments args) {
                    return args != null && !args.IsEmpty ? 1 : 0;
                }

                private void AddArguments(RubyArray/*!*/ list, Arguments args) {
                    if (args != null && !args.IsEmpty) {
                        Walk(args);
                        if (_result != Skip) {
                            list.Add(_result);
                        }
                    }
                }

                public override bool Enter(Arguments/*!*/ node) {
                    throw new NotSupportedException("TODO: argument splatting");
#if TODO
                    RubyArray exprs = VisitExpressionsAndMaplets(node);
                    if (node.Array != null) {
                        RubyArray args = MakeSplatArguments(exprs, node.Array);

                        object rhsValue;
                        if (TryGetRhsValue(out rhsValue)) {
                            _result = MakeNode(NodeKind.argspush, args, rhsValue);
                        } else {
                            _result = args;
                        }
                    } else if (exprs != null) {
                        _result = exprs;
                    } else {
                        _result = Skip;
                    }
                    return false;
#endif
                }

                private RubyArray/*!*/ MakeSplatArguments(RubyArray/*!*/ exprs, Expression/*!*/ splattedValue) {
                    RubyArray args;
                    if (exprs != null) {
                        args = MakeNode(NodeKind.argscat, 2);
                        args.Add(exprs);
                    } else {
                        args = MakeNode(NodeKind.splat, 1);
                    }

                    Walk(splattedValue);
                    args.Add(_result);

                    return args;
                }

                private RubyArray VisitExpressionsAndMaplets(Arguments/*!*/ node) {
                    if (!node.IsEmpty) {
                        var array = MakeNode(NodeKind.array, node.Expressions.Length);

                        AddRange(array, node.Expressions);

                        // TDOO: 1.9? append RHS unless splat is present:
                        object rhsValue;
                        if (TryGetRhsValue(out rhsValue)) {
                            array.Add(rhsValue);
                        }
                        
                        return array;
                    }

                    return null;
                }

                public override bool Enter(Maplet/*!*/ node) {
                    throw Assert.Unreachable;
                }

                #endregion

                #region Blocks

                private RubyArray/*!*/ MakeCallWithBlock(Block block, RubyArray/*!*/ call) {
                    if (block != null) {
                        var blockRef = block as BlockReference;
                        if (blockRef != null) {
                            var result = MakeNode(NodeKind.block_pass, 2); // 0 .. block, 1 .. call

                            // block:
                            Walk(blockRef.Expression);
                            result.Add(_result);

                            // call:
                            result.Add(call);

                            return result;
                        } else {
                            var blockDef = (BlockDefinition)block;
                            var result = MakeNode(NodeKind.iter, 3); // 0 .. call, 1 .. args(opt), 2 .. body(opt)

                            // call:
                            result.Add(call);

                            // block args:
                            UsingRhs(BlockRhs, () => {

                                Walk(blockDef.Parameters);
                                result.Add(_result);

                            });

                            // block body:
                            AddRange(result, blockDef.Body);
                            return result;
                        }
                    } else {
                        return call;
                    }
                }

                public override bool Enter(BlockReference/*!*/ node) {
                    throw Assert.Unreachable;
                }

                public override bool Enter(BlockDefinition/*!*/ node) {
                    throw Assert.Unreachable;                    
                }

                #endregion

                #region Variables

                public bool EnterVariable(string/*!*/ name, NodeKind read, NodeKind write) {
                    RubySymbol symbol = CreateSymbol(name);

                    RubyArray variable;
                    if (_rhs == null || _rhs.IsRhsArg) {
                        variable = MakeNode(read, symbol);
                    } else if (_rhs.InBlockParameters) {
                        variable = MakeNode((write == NodeKind.lasgn) ? NodeKind.dasgn_curr : write, symbol);
                    } else if (_rhs.InCompoundLhs) {
                        variable = MakeNode(write, symbol);
                    } else {
                        variable = MakeNode(write, symbol, _rhs.Value);
                    }
                    
                    _result = variable;
                    return false;
                }

                public override bool Enter(ClassVariable/*!*/ node) {
                    return EnterVariable(node.Name, NodeKind.cvar, NodeKind.cvdecl); 
                }

                public override bool Enter(ConstantVariable/*!*/ node) {
                    if (node.IsGlobal) {
                        return EnterVariable(node.Name, NodeKind.colon3, NodeKind.cdecl);
                    } else if (node.IsBound) {
                        var qualified = MakeNode(NodeKind.colon2, 2);
                        
                        Walk(node.Qualifier);
                        qualified.Add(_result);

                        qualified.Add(CreateSymbol(node.Name));

                        _result = qualified;
                        return false;
                    } else {
                        return EnterVariable(node.Name, NodeKind.@const, NodeKind.cdecl);
                    }
                }

                public override bool Enter(IronRuby.Compiler.Ast.GlobalVariable/*!*/ node) {
                    return EnterVariable(node.FullName, NodeKind.gvar, NodeKind.gasgn);
                }

                public override bool Enter(InstanceVariable/*!*/ node) {
                    return EnterVariable(node.Name, NodeKind.ivar, NodeKind.iasgn);
                }

                public override bool Enter(LocalVariable/*!*/ node) {
                    return EnterVariable(node.Name, NodeKind.lvar, NodeKind.lasgn);
                }

                public override bool Enter(RegexMatchReference/*!*/ node) {
                    if (node.FullName == "$~") {
                        return EnterVariable(node.FullName, NodeKind.gvar, NodeKind.gasgn);
                    } else if (node.Index > 0) {
                        Debug.Assert(_rhs == null);
                        _result = MakeNode(NodeKind.nth_ref, ScriptingRuntimeHelpers.Int32ToObject(node.Index));
                    } else {
                        Debug.Assert(_rhs == null);
                        _result = MakeNode(NodeKind.back_ref, CreateSymbol(node.VariableName));
                    }
                    return false;
                }

                public override bool Enter(Placeholder/*!*/ node) {
                    // nop
                    _result = Skip;
                    return false;
                }

                #endregion

                #region Assignment

                public override bool Enter(SimpleAssignmentExpression/*!*/ node) {
                    bool isAnd = node.Operation == "&&";
                    bool isOr = node.Operation == "||";

                    var oldRhs = _rhs;
                    
                    _rhs = null;
                    Walk(node.Right);
                    var rvalue = _result;

                    if (node.Operation != null && !isAnd && !isOr) {
                        Walk(node.Left);
                        rvalue = MakeNode(NodeKind.call, _result, CreateSymbol(node.Operation), MakeNode(NodeKind.array, rvalue));
                    }

                    _rhs = new Rhs { Value = rvalue };
                    Walk(node.Left);
                    
                    if (isAnd || isOr) {
                        var lvalue = _result;
                        _rhs = null;
                        Walk(node.Left);
                        _result = MakeNode(isAnd ? NodeKind.op_asgn_and : NodeKind.op_asgn_or, _result, lvalue);
                    }

                    _rhs = oldRhs;
                    return false;
                }

                public override bool Enter(MemberAssignmentExpression/*!*/ node) {
                    // TODO:
                    throw new NotImplementedException(node.NodeType.ToString());
                }

                public override bool Enter(ParallelAssignmentExpression/*!*/ node) {
                    // TODO: 1.9:

                    throw new NotSupportedException("TODO: parallel assignment");
#if TODO
                    var oldRhs = _rhs;
                    _rhs = null;
                    
                    if (node.Right.SplattedValue == null && node.Right.RightValues.Length == 1 && node.Left.LeftValues.Length > 0) {
                        Walk(node.Right.RightValues[0]);
                        _rhs = new Rhs { InCompoundLhs = true, InTopCompoundLhs = true, Value = MakeNode(NodeKind.to_ary, _result) };
                    } else if (node.Right.SplattedValue != null && node.Right.RightValues.Length == 0) {
                        Walk(node.Right.SplattedValue);

                        var rvalue = MakeNode(NodeKind.splat, _result);
                        if (node.Left.UnsplattedValue == null && node.Left.LeftValues.Length == 1) {
                            _rhs = new Rhs { Value = MakeNode(NodeKind.svalue, rvalue) };
                        } else {
                            _rhs = new Rhs { InCompoundLhs = true, InTopCompoundLhs = true, Value = rvalue };
                        }

                    } else {
                        var exprs = AddRange(MakeNode(NodeKind.array, node.Right.RightValues.Length), node.Right.RightValues);

                        if (node.Right.SplattedValue != null) {
                            exprs = MakeSplatArguments(exprs, node.Right.SplattedValue);
                        }

                        if (node.Left.UnsplattedValue == null && node.Left.LeftValues.Length == 1) {
                            _rhs = new Rhs { Value = MakeNode(NodeKind.svalue, exprs) };
                        } else {
                            _rhs = new Rhs { InCompoundLhs = true, InTopCompoundLhs = true, Value = exprs };
                        }
                    }

                    Walk(node.Left);
                    _rhs = oldRhs;
                    return false;
#endif
                }

                // RHS: [:call, ARRAY, :[], ARGUMENTS]
                // LHS: [:attrasgn, ARRAY, :[]=, ARGUMENTS + RHS]
                public override bool Enter(ArrayItemAccess/*!*/ node) {
                    if (_rhs == null) {
                        RubyArray call = MakeNode(NodeKind.call, 2 + SizeOf(node.Arguments));

                        // add target:
                        Walk(node.Array);
                        call.Add(_result);

                        // add name:
                        call.Add(CreateSymbol("[]"));

                        // add arguments:
                        AddArguments(call, node.Arguments);

                        // TODO: 1.9: add block:
                        // call.Add(node.Block);

                        _result = call;
                    } else {
                        var isRhsArg = _rhs.IsRhsArg;
                        _rhs.IsRhsArg = true;

                        var assignment = MakeNode(NodeKind.attrasgn, 2 + SizeOf(node.Arguments));

                        UsingRhs(null, () => {
                            Walk(node.Array);
                            assignment.Add(_result);
                        });

                        assignment.Add(CreateSymbol("[]="));

                        AddArguments(assignment, node.Arguments);

                        _rhs.IsRhsArg = isRhsArg;
                        _result = assignment;
                    }
                    return false;
                }

                // [:attrasgn, QUALIFIER, :NAME=, [:array, RHS]]
                public override bool Enter(AttributeAccess/*!*/ node) {
                    Debug.Assert(_rhs != null);
                    
                    var assignment = MakeNode(NodeKind.attrasgn, 3);

                    // qualifier:
                    UsingRhs(null, () => {
                        Walk(node.Qualifier);
                        assignment.Add(_result);
                    });

                    // name:
                    assignment.Add(CreateSymbol(node.Name));

                    // rhs array:
                    object rhsValue;
                    if (TryGetRhsValue(out rhsValue)) {
                        assignment.Add(MakeNode(NodeKind.array, rhsValue));
                    }

                    _result = assignment;
                    return false;
                }

                public override bool Enter(AssignmentExpression/*!*/ node) {
                    throw Assert.Unreachable;
                }

                #endregion

                #region CompoundLeftValue

                public override bool Enter(CompoundLeftValue/*!*/ node) {
                    Debug.Assert(_rhs != null);

                    if (node.UnsplattedValue == null) {
                        if (node.LeftValues.Length == 0) {
                            Debug.Assert(_rhs == BlockRhs);
                            _result = ScriptingRuntimeHelpers.Int32ToObject(0);
                            return false;
                        }

                        if (node.LeftValues.Length == 1) {
                            Walk(node.LeftValues[0]);
                            return false;
                        }
                    }

                    bool isTop = _rhs.InTopCompoundLhs;
                    _rhs.InTopCompoundLhs = false;

                    var assignment = MakeNode(NodeKind.masgn,
                        (node.LeftValues.Length > 1 ? 1 : 0) + 
                        (node.UnsplattedValue != null ? 1 : 0) +
                        (_rhs != null ? 1 : 0) +
                        (isTop ? 1 : 0) // outer most gets RHS
                    );

                    // TODO: 1.9:  leading-l-values, *, trailing-l-values

                    if (node.LeftValues.Length > 1) {
                        assignment.Add(
                            AddRange(MakeNode(NodeKind.array, node.LeftValues.Length), node.LeftValues)
                        );
                    }

                    if (node.UnsplattedValue != null) {
                        if (node.UnsplattedValue is Placeholder) {
                            assignment.Add(MakeNode(NodeKind.splat));
                        } else {
                            Walk(node.UnsplattedValue);
                            assignment.Add(_result);
                        }
                    }

                    if (_rhs.InCompoundLhs && isTop) {
                        assignment.Add(_rhs.Value);
                    }

                    _rhs.InTopCompoundLhs = isTop;
                    _result = assignment;
                    return false;
                }

                //MemberAssignmentExpression
                //SimpleAssignmentExpression
                //

                #endregion

                #region Alias, Undefine, Defined?, Self, Initializer, Finalizer

                public override bool Enter(AliasStatement/*!*/ node) {
                    if (node.IsGlobalVariableAlias) {
                        _result = MakeNode(NodeKind.valias,
                            CreateSymbol("$" + node.NewName),
                            CreateSymbol("$" + node.OldName)
                        );
                    } else {
                        // TODO: handle constructed symbols
                        _result = MakeNode(NodeKind.alias,
                            MakeNode(NodeKind.lit, CreateSymbol((string)node.NewName.Value)),
                            MakeNode(NodeKind.lit, CreateSymbol((string)node.OldName.Value))
                        );
                    }
                    return false;
                }

                public override bool Enter(UndefineStatement/*!*/ node) {
                    if (node.Items.Count == 1) {
                        // TODO: handle constructed symbols
                        _result = MakeNode(NodeKind.undef, MakeNode(NodeKind.lit, CreateSymbol((string)node.Items[0].Value)));
                    } else {
                        var block = MakeNode(NodeKind.block, node.Items.Count);
                        foreach (var item in node.Items) {
                            // TODO: handle constructed symbols
                            block.Add(MakeNode(NodeKind.undef, MakeNode(NodeKind.lit, CreateSymbol((string)item.Value))));
                        }
                        _result = block;
                    }
                    return false;
                }

                public override bool Enter(IsDefinedExpression/*!*/ node) {
                    var def = MakeNode(NodeKind.defined, 1);

                    Walk(node.Expression);
                    def.Add(_result);

                    _result = def;
                    return false;
                }

                public override bool Enter(SelfReference/*!*/ node) {
                    _result = MakeNode(NodeKind.self);
                    return false;
                }

                public override bool Enter(FileInitializerStatement/*!*/ node) {
                    // TODO:
                    throw new NotImplementedException();
                }

                public override bool Enter(ShutdownHandlerStatement/*!*/ node) {
                    // TODO:
                    throw new NotImplementedException();
                }

                #endregion

                #region Boolean Expressions

                private bool EnterBooleanExpression(Expression/*!*/ left, Expression/*!*/ right, NodeKind kind) {
                    var b = MakeNode(kind, 2);
                    Walk(left);
                    b.Add(_result);

                    Walk(right);
                    b.Add(_result);

                    _result = b;
                    return false;
                }

                public override bool Enter(AndExpression/*!*/ node) {
                    return EnterBooleanExpression(node.Left, node.Right, NodeKind.and);
                }

                public override bool Enter(OrExpression/*!*/ node) {
                    return EnterBooleanExpression(node.Left, node.Right, NodeKind.or);
                }

                public override bool Enter(NotExpression/*!*/ node) {
                    var b = MakeNode(NodeKind.not, 1);
                    Walk(node.Expression);
                    b.Add(_result);

                    _result = b;
                    return false; 
                }

                #endregion

                #region JumpStatements

                private bool EnterJumpStatement(JumpStatement/*!*/ node, NodeKind kind) {
                    var jmp = MakeNode(kind, SizeOf(node.Arguments));

                    // add arguments:
                    AddArguments(jmp, node.Arguments); // TODO: splat [:array, value]

                    _result = jmp;
                    return false;
                }

                public override bool Enter(BreakStatement/*!*/ node) {
                    return EnterJumpStatement(node, NodeKind.@break);
                }

                public override bool Enter(NextStatement/*!*/ node) {
                    return EnterJumpStatement(node, NodeKind.next);
                }

                public override bool Enter(ReturnStatement/*!*/ node) {
                    return EnterJumpStatement(node, NodeKind.@return);
                }

                public override bool Enter(RetryStatement/*!*/ node) {
                    return EnterJumpStatement(node, NodeKind.retry);
                }

                public override bool Enter(RedoStatement/*!*/ node) {
                    return EnterJumpStatement(node, NodeKind.redo);
                }

                #endregion

                #region Body (EH)

                public override bool Enter(BlockExpression/*!*/ node) {
                    _result = MakeBlock(node.Statements);
                    return false;
                }

                public override bool Enter(Body/*!*/ node) {
                    var begin = MakeNode(NodeKind.begin, 5);
                    AddBody(begin, node);

                    _result = begin;
                    return false;
                }

                private RubyArray/*!*/ AddBody(RubyArray/*!*/ list, Body/*!*/ node) {
                    RubyArray current;
                    if (node.EnsureStatements != null) {
                        list.Add(current = MakeNode(NodeKind.ensure, 2));
                    } else {
                        current = list;
                    }

                    if (node.RescueClauses != null) {
                        var rescue = MakeNode(NodeKind.rescue);
                        rescue.Add(MakeBlock(node.Statements));
                        AddRescueBody(rescue, node);
                        AddRange(rescue, node.ElseStatements);
                        current.Add(rescue);
                    } else {
                        current.Add(MakeBlock(node.Statements));
                    }

                    AddRange(current, node.EnsureStatements);

                    return list;
                }

                /// <code>
                /// rescue
                ///   2
                /// rescue A => e
                ///   3
                /// rescue A,B => e
                ///   4
                /// rescue A
                ///   5
                /// rescue A,B
                ///   6
                /// end
                /// 
                /// [:resbody, 
                ///     nil, 
                ///     [:lit, 2], 
                ///     [:resbody, 
                ///         [:array, [:const, :A]], 
                ///         [:block, 
                ///             [:lasgn, :e, [:gvar, :$!]], 
                ///             [:lit, 3]
                ///         ], 
                ///         [:resbody, 
                ///             [:array, [:const, :A], [:const, :B]], 
                ///             [:block, 
                ///                 [:lasgn, :e, [:gvar, :$!]], 
                ///                 [:lit, 4]
                ///             ], 
                ///             [:resbody, 
                ///                 [:array, [:const, :A]], 
                ///                 [:lit, 5], 
                ///                 [:resbody, 
                ///                     [:array, [:const, :A], [:const, :B]],
                ///                     [:lit, 6]
                ///                 ]
                ///             ]
                ///         ]
                ///     ]
                /// ]
                /// </code>
                private void AddRescueBody(RubyArray/*!*/ current, Body/*!*/ node) {
                    foreach (var clause in node.RescueClauses) {
                        var resbody = MakeNode(NodeKind.resbody, 3);

                        if (clause.Types != null) {
                            resbody.Add(AddRange(MakeNode(NodeKind.array, clause.Types.Length), clause.Types));
                        } else {
                            resbody.Add(null);
                        }

                        if (clause.Target != null) {
                            UsingRhs(new Rhs { Value = MakeNode(NodeKind.gvar, CreateSymbol("$!")) }, () => {
                                Walk(clause.Target);
                            });
                            var assignment = _result;

                            var block = MakeNode(NodeKind.block, 1 + (clause.Statements != null ? clause.Statements.Count : 0));
                            block.Add(assignment);
                            AddRange(block, clause.Statements);

                            resbody.Add(block);
                        } else {
                            AddRange(resbody, clause.Statements);
                        }
                       
                        current.Add(resbody);
                        current = resbody;
                    }
                }

                public override bool Enter(RescueClause/*!*/ node) {
                    throw Assert.Unreachable;
                }

                public override bool Enter(RescueExpression/*!*/ node) {
                    var rescue = MakeNode(NodeKind.rescue, 2);

                    Walk(node.GuardedExpression);
                    rescue.Add(_result);

                    var resbody = MakeNode(NodeKind.resbody, 2);
                    
                    resbody.Add(null);
                    
                    Walk(node.RescueClauseStatement);
                    resbody.Add(_result);

                    rescue.Add(resbody);

                    _result = rescue;
                    return false;
                }

                #endregion

                #region Flow

                private object MakeBlock(Statements statements) {
                    var block = MakeBlockOpt(statements);
                    return (block != Skip) ? block : MakeNode(NodeKind.nil);
                }

                private RubyArray/*!*/ AddBlock(RubyArray/*!*/ list, Statements statements) {
                    var block = MakeBlockOpt(statements);
                    if (block != Skip) {
                        list.Add(block);
                    }
                    return list;
                }

                private object MakeBlockOpt(Statements statements) {
                    if (statements == null || statements.Count == 0) {
                        return Skip;
                    } else if (statements.Count == 1) {
                        Walk(statements.First);
                        return _result;
                    } else {
                        return AddRange(MakeNode(NodeKind.block, statements.Count), statements);
                    }
                }

                public override bool Enter(IfExpression/*!*/ node) {
                    RubyArray @if;
                    RubyArray current = @if = MakeNode(NodeKind.@if, 3);

                    Walk(node.Condition);
                    current.Add(_result);

                    current.Add(MakeBlock(node.Body));

                    if (node.ElseIfClauses != null && node.ElseIfClauses.Count != 0) {
                        foreach (var clause in node.ElseIfClauses) {
                            if (clause.Condition != null) {
                                // elsif cond
                                current.Add(current = MakeNode(NodeKind.@if, 3));

                                Walk(clause.Condition);
                                current.Add(_result);
                            }

                            current.Add(MakeBlock(clause.Statements));
                        }

                        if (node.ElseIfClauses[node.ElseIfClauses.Count - 1].Condition != null) {
                            current.Add(null);
                        }
                    } else {
                        current.Add(null);
                    }

                    _result = @if;
                    return false;
                }

                public override bool Enter(UnlessExpression/*!*/ node) {
                    var @if = MakeNode(NodeKind.@if, 3);

                    Walk(node.Condition);
                    @if.Add(_result);

                    if (node.ElseClause != null) {
                        Debug.Assert(node.ElseClause.Condition == null);
                        @if.Add(MakeBlock(node.ElseClause.Statements));
                    } else {
                        @if.Add(null);
                    }

                    @if.Add(MakeBlock(node.Statements));

                    _result = @if;
                    return false;
                }

                public override bool Enter(ElseIfClause/*!*/ node) {
                    throw Assert.Unreachable;
                }

                private bool EnterTernary(NodeKind kind, Expression/*!*/ expr1, Expression expr2, Expression expr3) {
                    var result = MakeNode(NodeKind.@if, 3);

                    Walk(expr1);
                    result.Add(_result);

                    if (expr2 != null) {
                        Walk(expr2);
                        result.Add(_result);
                    } else {
                        result.Add(null);
                    }

                    if (expr3 != null) {
                        Walk(expr3);
                        result.Add(_result);
                    } else {
                        result.Add(null);
                    }

                    _result = result;
                    return false;
                }

                public override bool Enter(ConditionalExpression/*!*/ node) {
                    return EnterTernary(NodeKind.@if, node.Condition, node.TrueExpression, node.FalseExpression);
                }

                public override bool Enter(ConditionalJumpExpression/*!*/ node) {
                    if (node.IsBooleanExpression) {
                        if (node.NegateCondition) {
                            return EnterBooleanExpression(node.Condition, node.JumpStatement, NodeKind.or);
                        } else {
                            return EnterBooleanExpression(node.Condition, node.JumpStatement, NodeKind.and);
                        }
                    }

                    if (node.NegateCondition) {
                        return EnterTernary(NodeKind.@if, node.Condition, node.Value, node.JumpStatement);
                    } else {
                        return EnterTernary(NodeKind.@if, node.Condition, node.JumpStatement, node.Value);
                    }
                }

                public override bool Enter(ConditionalStatement/*!*/ node) {
                    if (node.IsUnless) {
                        return EnterTernary(NodeKind.@if, node.Condition, node.ElseStatement, node.Body);
                    } else {
                        return EnterTernary(NodeKind.@if, node.Condition, node.Body, node.ElseStatement);
                    }
                }

                public override bool Enter(WhileLoopExpression/*!*/ node) {
                    var loop = MakeNode(node.IsWhileLoop ? NodeKind.@while : NodeKind.until);

                    Walk(node.Condition);
                    loop.Add(_result);
                    
                    loop.Add(MakeBlock(node.Statements));
                    
                    loop.Add(!node.IsPostTest);
                    
                    _result = loop;
                    return false;
                }

                public override bool Enter(ForLoopExpression/*!*/ node) {
                    var loop = MakeNode(NodeKind.@for, 3);

                    Walk(node.List);
                    loop.Add(_result);

                    UsingRhs(new Rhs { InCompoundLhs = true, InTopCompoundLhs = false}, () => {
                        Walk(node.Block.Parameters);
                        loop.Add(_result);

                        // block body:
                        AddBlock(loop, node.Block.Body);
                    });

                    _result = loop;
                    return false;
                }

                // case v 
                //     when ca1,ca2: a
                //     when cb: b
                //     when cc:
                //     when cd1, cd2, cd3*: a
                // else
                //     e
                // end
                //
                // [:case, 
                //     <v>, 
                //     [:when, [:array, <ca1>, <ca2>], <a>], 
                //     [:when, [:array, <cb>], <b>], 
                //     [:when, [:array, <cb>], nil], 
                //     [:when, [:array, <cd1>, <cd2>, [:when, <cd3>, nil]], <a>]
                //     <e>
                // ]
                public override bool Enter(CaseExpression/*!*/ node) {
                    var c = MakeNode(NodeKind.@case, 1 + node.WhenClauses.Length + 1);

                    if (node.Value != null) {
                        Walk(node.Value);
                        c.Add(_result);
                    } else {
                        c.Add(null);
                    }

                    if (node.WhenClauses != null) {
                        foreach (var whenClause in node.WhenClauses) {
                            var when = MakeNode(NodeKind.when, 2);

                            var array = MakeNode(NodeKind.array, whenClause.Comparisons.Length);

                            AddRange(array, whenClause.Comparisons);

                            // TODO: 1.9 splatting
                            //if (whenClause.ComparisonArray != null) {
                            //    Walk(whenClause.ComparisonArray);
                            //    array.Add(MakeNode(NodeKind.when, _result, null));
                            //}

                            when.Add(array);
                            when.Add(MakeBlock(whenClause.Statements));

                            c.Add(when);
                        }
                    }

                    c.Add(MakeBlock(node.ElseStatements));

                    _result = c;
                    return false;
                }

                public override bool Enter(WhenClause/*!*/ node) {
                    throw Assert.Unreachable;
                }
                                
                #endregion

                #region Declarations

                private void AddScope(RubyArray/*!*/ list, DefinitionExpression/*!*/ node) {
                    list.Add(AddBody(MakeNode(NodeKind.scope), node.Body));
                }

                public override bool Enter(ModuleDefinition/*!*/ node) {
                    var module = MakeNode(NodeKind.module, 2);

                    Walk(node.QualifiedName);
                    module.Add(_result);

                    AddScope(module, node);
                    
                    _result = module;
                    return false;
                }

                public override bool Enter(ClassDefinition/*!*/ node) {
                    var module = MakeNode(NodeKind.@class, 3);

                    Walk(node.QualifiedName);
                    module.Add(_result);

                    if (node.SuperClass != null) {
                        Walk(node.SuperClass);
                        module.Add(_result);
                    } else {
                        module.Add(null);
                    }

                    AddScope(module, node);

                    _result = module;
                    return false;
                }

                public override bool Enter(SingletonDefinition/*!*/ node) {
                    var module = MakeNode(NodeKind.sclass, 2);

                    Walk(node.Singleton);
                    module.Add(_result);

                    AddScope(module, node);

                    _result = module;
                    return false;
                }

                public override bool Enter(MethodDefinition/*!*/ node) {
                    bool isMethodAlias = _isMethodAlias;
                    _isMethodAlias = false;

                    RubyArray method;
                    if (node.Target != null) {
                        method = MakeNode(NodeKind.defs, 3);

                        Walk(node.Target);
                        method.Add(_result);
                    } else {
                        method = MakeNode(NodeKind.defn, 2);
                    }

                    method.Add(CreateSymbol(node.Name));

                    var scope = MakeNode(NodeKind.scope, 1);
                    var block = MakeNode(NodeKind.block, 5);

                    var parameters = MakeNode(NodeKind.args, 
                        node.Parameters.Mandatory.Length + 
                        node.Parameters.Optional.Length + 
                        (node.Parameters.Unsplat != null ? 1 : 0)
                    );

                    // TODO: 1.9
                    if (node.Parameters.Mandatory.Length > 0) {
                        //foreach (var p in node.Parameters.Mandatory) {
                        //    parameters.Add(CreateSymbol(p.Name));
                        //}
                        throw new NotSupportedException("TODO: compound parameters");
                    }

                    foreach (var assignment in node.Parameters.Optional) {
                        parameters.Add(CreateSymbol(((LocalVariable)assignment.Left).Name));
                    }

                    if (node.Parameters.Unsplat != null) {
                        parameters.Add(CreateSymbol("*" + ((LocalVariable)node.Parameters.Unsplat).Name));
                    }

                    if (node.Parameters.Optional.Length > 0) {
                        var paramInit = MakeNode(NodeKind.block);
                        foreach (var assignment in node.Parameters.Optional) {
                            Walk(assignment);
                            paramInit.Add(_result);
                        }
                        parameters.Add(paramInit);
                    }

                    block.Add(parameters);

                    if (node.Parameters.Block != null) {
                        block.Add(MakeNode(NodeKind.block_arg, CreateSymbol(node.Parameters.Block.Name)));
                    }

                    AddBody(block, node.Body);

                    scope.Add(block);

                    if (isMethodAlias) {
                        method.Add(MakeNode(NodeKind.fbody, scope));
                    } else {
                        method.Add(scope);
                    }

                    _isMethodAlias = isMethodAlias;
                    _result = method;
                    return false;
                }

                public override bool Enter(Parameters/*!*/ node) {
                    throw Assert.Unreachable;
                }

                #endregion

            }
        }
    }
}
