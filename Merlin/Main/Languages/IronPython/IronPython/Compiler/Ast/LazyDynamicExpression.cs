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
using MSAst = System.Linq.Expressions;
#else
using MSAst = Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Scripting.Interpreter;
using System.Dynamic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Utils;

namespace IronPython.Compiler.Ast {
    using Ast = MSAst.Expression;

    /// <summary>
    /// Light weight dynamic expression which does no validation on the inputs, supports interpretation
    /// by lazily creating a dynamic site, and supports compilation by reducing using our compilation
    /// mode.
    /// </summary>
    class LazyDynamicExpression : MSAst.Expression, IInstructionProvider {
        private readonly DynamicMetaObjectBinder/*!*/ _binder;
        private readonly MSAst.Expression/*!*/[]/*!*/ _args;
        private readonly CompilationMode/*!*/ _mode;

        public LazyDynamicExpression(DynamicMetaObjectBinder/*!*/ binder, CompilationMode/*!*/ mode, params MSAst.Expression/*!*/[]/*!*/ arguments) {
            Assert.NotNull(binder, mode, arguments);
            Assert.NotNullItems(arguments);

            _binder = binder;
            _mode = mode;
            _args = arguments;
        }

        public override bool CanReduce {
            get {
                return true;
            }
        }

        public override MSAst.ExpressionType NodeType {
            get {
                return MSAst.ExpressionType.Extension;
            }
        }

        public override Type Type {
            get {
                return typeof(object);
            }
        }

        public override MSAst.Expression Reduce() {
            return _mode.Dynamic(
                _binder,
                typeof(object),
                _args
            );
        }

        #region IInstructionProvider Members

        public void AddInstructions(LightCompiler compiler) {
            if (_args.Length > 7) {
                compiler.Compile(Ast.Dynamic(_binder, typeof(object), _args));
            } else {
                for (int i = 0; i < _args.Length; i++) {
                    compiler.Compile(_args[i]);
                }

                switch (_args.Length) {
                    case 1: compiler.Instructions.Emit(new LazyDynamicInstruction1(_binder)); break;
                    case 2: compiler.Instructions.Emit(new LazyDynamicInstruction2(_binder)); break;
                    case 3: compiler.Instructions.Emit(new LazyDynamicInstruction3(_binder)); break;
                    case 4: compiler.Instructions.Emit(new LazyDynamicInstruction4(_binder)); break;
                    case 5: compiler.Instructions.Emit(new LazyDynamicInstruction5(_binder)); break;
                    case 6: compiler.Instructions.Emit(new LazyDynamicInstruction6(_binder)); break;
                    case 7: compiler.Instructions.Emit(new LazyDynamicInstruction7(_binder)); break;
                    default: Debug.Assert(false); break;
                }
            }

        }

        #endregion

        class LazyDynamicInstruction1 : Instruction {
            private readonly DynamicMetaObjectBinder _binder;
            private CallSite<Func<CallSite, object, object>> _site;

            public LazyDynamicInstruction1(DynamicMetaObjectBinder binder) {
                _binder = binder;
            }

            public override int ConsumedStack {
                get {
                    return 1;
                }
            }

            public override int ProducedStack {
                get {
                    return 1;
                }
            }

            public override int Run(InterpretedFrame frame) {
                if (_site == null) {
                    _site = CallSite<Func<CallSite, object, object>>.Create(_binder);
                }

                frame.Push(_site.Target(_site, frame.Pop()));
                return +1;
            }
        }

        class LazyDynamicInstruction2 : Instruction {
            private DynamicMetaObjectBinder _binder;
            private CallSite<Func<CallSite, object, object, object>> _site;

            public LazyDynamicInstruction2(DynamicMetaObjectBinder binder) {
                _binder = binder;
            }

            public override int ConsumedStack {
                get {
                    return 2;
                }
            }

            public override int ProducedStack {
                get {
                    return 1;
                }
            }

            public override int Run(InterpretedFrame frame) {
                if (_site == null) {
                    _site = CallSite<Func<CallSite, object, object, object>>.Create(_binder);
                }
                object arg1 = frame.Pop();
                object arg0 = frame.Pop();
                frame.Push(_site.Target(_site, arg0, arg1));
                return +1;
            }
        }

        class LazyDynamicInstruction3 : Instruction {
            private DynamicMetaObjectBinder _binder;
            private CallSite<Func<CallSite, object, object, object, object>> _site;

            public LazyDynamicInstruction3(DynamicMetaObjectBinder binder) {
                _binder = binder;
            }

            public override int ConsumedStack {
                get {
                    return 3;
                }
            }

            public override int ProducedStack {
                get {
                    return 1;
                }
            }

            public override int Run(InterpretedFrame frame) {
                if (_site == null) {
                    _site = CallSite<Func<CallSite, object, object, object, object>>.Create(_binder);
                }
                object arg2 = frame.Pop();
                object arg1 = frame.Pop();
                object arg0 = frame.Pop();
                frame.Push(_site.Target(_site, arg0, arg1, arg2));
                return +1;
            }
        }

        class LazyDynamicInstruction4 : Instruction {
            private DynamicMetaObjectBinder _binder;
            private CallSite<Func<CallSite, object, object, object, object, object>> _site;

            public LazyDynamicInstruction4(DynamicMetaObjectBinder binder) {
                _binder = binder;
            }

            public override int ConsumedStack {
                get {
                    return 4;
                }
            }

            public override int ProducedStack {
                get {
                    return 1;
                }
            }

            public override int Run(InterpretedFrame frame) {
                if (_site == null) {
                    _site = CallSite<Func<CallSite, object, object, object, object, object>>.Create(_binder);
                }
                object arg3 = frame.Pop();
                object arg2 = frame.Pop();
                object arg1 = frame.Pop();
                object arg0 = frame.Pop();
                frame.Push(_site.Target(_site, arg0, arg1, arg2, arg3));
                return +1;
            }
        }

        class LazyDynamicInstruction5 : Instruction {
            private DynamicMetaObjectBinder _binder;
            private CallSite<Func<CallSite, object, object, object, object, object, object>> _site;

            public LazyDynamicInstruction5(DynamicMetaObjectBinder binder) {
                _binder = binder;
            }

            public override int ConsumedStack {
                get {
                    return 5;
                }
            }

            public override int ProducedStack {
                get {
                    return 1;
                }
            }

            public override int Run(InterpretedFrame frame) {
                if (_site == null) {
                    _site = CallSite<Func<CallSite, object, object, object, object, object, object>>.Create(_binder);
                }
                object arg4 = frame.Pop();
                object arg3 = frame.Pop();
                object arg2 = frame.Pop();
                object arg1 = frame.Pop();
                object arg0 = frame.Pop();
                frame.Push(_site.Target(_site, arg0, arg1, arg2, arg3, arg4));
                return +1;
            }
        }

        class LazyDynamicInstruction6 : Instruction {
            private DynamicMetaObjectBinder _binder;
            private CallSite<Func<CallSite, object, object, object, object, object, object, object>> _site;

            public LazyDynamicInstruction6(DynamicMetaObjectBinder binder) {
                _binder = binder;
            }

            public override int ConsumedStack {
                get {
                    return 6;
                }
            }

            public override int ProducedStack {
                get {
                    return 1;
                }
            }

            public override int Run(InterpretedFrame frame) {
                if (_site == null) {
                    _site = CallSite<Func<CallSite, object, object, object, object, object, object, object>>.Create(_binder);
                }
                object arg5 = frame.Pop();
                object arg4 = frame.Pop();
                object arg3 = frame.Pop();
                object arg2 = frame.Pop();
                object arg1 = frame.Pop();
                object arg0 = frame.Pop();
                frame.Push(_site.Target(_site, arg0, arg1, arg2, arg3, arg4, arg5));
                return +1;
            }
        }

        class LazyDynamicInstruction7 : Instruction {
            private DynamicMetaObjectBinder _binder;
            private CallSite<Func<CallSite, object, object, object, object, object, object, object, object>> _site;

            public LazyDynamicInstruction7(DynamicMetaObjectBinder binder) {
                _binder = binder;
            }

            public override int ConsumedStack {
                get {
                    return 7;
                }
            }

            public override int ProducedStack {
                get {
                    return 1;
                }
            }

            public override int Run(InterpretedFrame frame) {
                if (_site == null) {
                    _site = CallSite<Func<CallSite, object, object, object, object, object, object, object, object>>.Create(_binder);
                }
                object arg6 = frame.Pop();
                object arg5 = frame.Pop();
                object arg4 = frame.Pop();
                object arg3 = frame.Pop();
                object arg2 = frame.Pop();
                object arg1 = frame.Pop();
                object arg0 = frame.Pop();
                frame.Push(_site.Target(_site, arg0, arg1, arg2, arg3, arg4, arg5, arg6));
                return +1;
            }
        }
    }
}
