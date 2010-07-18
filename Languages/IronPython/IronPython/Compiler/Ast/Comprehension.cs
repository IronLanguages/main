/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.Scripting;

using IronPython.Runtime;

#if !CLR2
using MSAst = System.Linq.Expressions;
#else
using MSAst = Microsoft.Scripting.Ast;
#endif

using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Compiler.Ast {
    using Ast = MSAst.Expression;

    public abstract class ComprehensionIterator : Node {
        internal abstract MSAst.Expression Transform(MSAst.Expression body);
    }

    public abstract class Comprehension : Expression {
        public abstract IList<ComprehensionIterator> Iterators { get; }
        public abstract override string NodeName { get; }

        protected abstract MSAst.ParameterExpression MakeParameter();
        protected abstract MethodInfo Factory();
        protected abstract MSAst.Expression Body(MSAst.ParameterExpression res);

        public abstract override void Walk(PythonWalker walker);

        public override Ast Reduce() {
            MSAst.ParameterExpression res = MakeParameter();

            // 1. Initialization code - create list and store it in the temp variable
            MSAst.Expression initialize =
                Ast.Assign(
                    res,
                    Ast.Call(Factory())
                );

            // 2. Create body from LHS: res.Append(item), res.Add(key, value), etc.
            MSAst.Expression body = Body(res);

            // 3. Transform all iterators in reverse order, building the true bodies
            for (int current = Iterators.Count - 1; current >= 0; current--) {
                ComprehensionIterator iterator = Iterators[current];
                body = iterator.Transform(body);
            }

            return Ast.Block(
                new[] { res },
                initialize,
                body,
                res
            );
        }
    }

    public sealed class ListComprehension : Comprehension {
        private readonly ComprehensionIterator[] _iterators;
        private readonly Expression _item;

        public ListComprehension(Expression item, ComprehensionIterator[] iterators) {
            _item = item;
            _iterators = iterators;
        }

        public Expression Item {
            get { return _item; }
        }

        public override IList<ComprehensionIterator> Iterators {
            get { return _iterators; }
        }

        protected override MSAst.ParameterExpression MakeParameter() {
            return Ast.Parameter(typeof(List), "list_comprehension_list");
        }

        protected override MethodInfo Factory() {
            return AstMethods.MakeList;
        }

        protected override Ast Body(MSAst.ParameterExpression res) {
            return GlobalParent.AddDebugInfo(
                Ast.Call(
                    AstMethods.ListAddForComprehension,
                    res,
                    AstUtils.Convert(_item, typeof(object))
                ),
                _item.Span
            );
        }

        public override string NodeName {
            get {
                return "list comprehension";
            }
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_item != null) {
                    _item.Walk(walker);
                }
                if (_iterators != null) {
                    foreach (ComprehensionIterator ci in _iterators) {
                        ci.Walk(walker);
                    }
                }
            }
            walker.PostWalk(this);
        }
    }

    public sealed class SetComprehension : Comprehension {
        private readonly ComprehensionIterator[] _iterators;
        private readonly Expression _item;

        public SetComprehension(Expression item, ComprehensionIterator[] iterators) {
            _item = item;
            _iterators = iterators;
        }

        public Expression Item {
            get { return _item; }
        }

        public override IList<ComprehensionIterator> Iterators {
            get { return _iterators; }
        }

        protected override MSAst.ParameterExpression MakeParameter() {
            return Ast.Parameter(typeof(SetCollection), "set_comprehension_set");
        }

        protected override MethodInfo Factory() {
            return AstMethods.MakeEmptySet;
        }

        protected override Ast Body(MSAst.ParameterExpression res) {
            return GlobalParent.AddDebugInfo(
                Ast.Call(
                    AstMethods.SetAddForComprehension,
                    res,
                    AstUtils.Convert(_item, typeof(object))
                ),
                _item.Span
            );
        }

        public override string NodeName {
            get {
                return "set comprehension";
            }
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_item != null) {
                    _item.Walk(walker);
                }
                if (_iterators != null) {
                    foreach (ComprehensionIterator ci in _iterators) {
                        ci.Walk(walker);
                    }
                }
            }
            walker.PostWalk(this);
        }
    }

    public sealed class DictionaryComprehension : Comprehension {
        private readonly ComprehensionIterator[] _iterators;
        private readonly Expression _key, _value;

        public DictionaryComprehension(Expression key, Expression value, ComprehensionIterator[] iterators) {
            _key = key;
            _value = value;
            _iterators = iterators;
        }

        public Expression Key {
            get { return _key; }
        }

        public Expression Value {
            get { return _value; }
        }

        public override IList<ComprehensionIterator> Iterators {
            get { return _iterators; }
        }

        protected override MSAst.ParameterExpression MakeParameter() {
            return Ast.Parameter(typeof(PythonDictionary), "dict_comprehension_dict");
        }

        protected override MethodInfo Factory() {
            return AstMethods.MakeEmptyDict;
        }

        protected override Ast Body(MSAst.ParameterExpression res) {
            return GlobalParent.AddDebugInfo(
                Ast.Call(
                    AstMethods.DictAddForComprehension,
                    res,
                    AstUtils.Convert(_key, typeof(object)),
                    AstUtils.Convert(_value, typeof(object))
                ),
                new SourceSpan(_key.Span.Start, _value.Span.End)
            );
        }

        public override string NodeName {
            get {
                return "dict comprehension";
            }
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_key != null) {
                    _key.Walk(walker);
                }
                if (_value != null) {
                    _value.Walk(walker);
                }
                if (_iterators != null) {
                    foreach (ComprehensionIterator ci in _iterators) {
                        ci.Walk(walker);
                    }
                }
            }
            walker.PostWalk(this);
        }
    }
}
