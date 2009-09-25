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
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using AstFactory = IronRuby.Compiler.Ast.AstFactory;
using IronRuby.Compiler;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using System.Collections;

namespace IronRuby.Runtime.Calls {
    using Ast = Expression;

    public sealed class ArgsBuilder {
        private readonly Expression[]/*!*/ _arguments;
        private readonly int _mandatoryParamCount;
        private readonly int _optionalParamCount;
        private readonly bool _hasUnsplatParameter;

        // Actual arguments that overflow the signature of the callee and thus will be unsplatted.
        private List<Expression> _argsToUnsplat;

        private int _nextArgIndex;

        // Total number of arguments explicitly passed to the call site 
        // (whether they map to a mandatory, optional or unsplat parameter).
        private int _explicitArgCount;

        public int ExplicitArgumentCount {
            get { return _explicitArgCount; }
        }

        public int MandatoryParamCount {
            get { return _mandatoryParamCount; }
        }

        public int OptionalParamCount {
            get { return _optionalParamCount; }
        }

        public bool HasUnsplatParameter {
            get { return _hasUnsplatParameter; }
        }
        
        public bool HasTooFewArguments {
            get { return _explicitArgCount < _mandatoryParamCount; }
        }

        public bool HasTooManyArguments {
            get { return !_hasUnsplatParameter && _explicitArgCount > _mandatoryParamCount + _optionalParamCount; }
        }

        /// <param name="implicitParamCount">Parameters for which arguments are provided implicitly, i.e. not specified by user.</param>
        /// <param name="mandatoryParamCount">Number of parameters for which an actual argument must be specified.</param>
        /// <param name="optionalParamCount">Number of optional parameters.</param>
        /// <param name="hasUnsplatParameter">Method has * parameter (accepts any number of additional parameters).</param>
        public ArgsBuilder(int implicitParamCount, int mandatoryParamCount, int optionalParamCount, bool hasUnsplatParameter) {
            _arguments = new Expression[implicitParamCount + mandatoryParamCount + optionalParamCount + (hasUnsplatParameter ? 1 : 0)];
            _mandatoryParamCount = mandatoryParamCount;
            _optionalParamCount = optionalParamCount;
            _nextArgIndex = implicitParamCount;
            _explicitArgCount = 0;
            _argsToUnsplat = null;
            _hasUnsplatParameter = hasUnsplatParameter;
        }

        /// <summary>
        /// Adds explicit arguments and maps themp to parameters.
        /// </summary>
        public void AddRange(IList<Expression>/*!*/ values) {
            foreach (Expression value in values) {
                Add(value);
            }
        }

        /// <summary>
        /// Adds an explicit argument and maps it to a parameter.
        /// </summary>
        public void Add(Expression/*!*/ value) {
            Assert.NotNull(value);

            if (_explicitArgCount < _mandatoryParamCount + _optionalParamCount) {
                Debug.Assert(_nextArgIndex < _arguments.Length);
                _arguments[_nextArgIndex++] = value;
            } else {
                if (_argsToUnsplat == null) {
                    _argsToUnsplat = new List<Expression>();
                }
                _argsToUnsplat.Add(value);
            }

            _explicitArgCount++;
        }

        public void SetImplicit(int index, Expression/*!*/ arg) {
            _arguments[index] = arg;
        }

        public Expression this[int index] {
            get { return _arguments[index]; }
        }

        internal Expression[]/*!*/ GetArguments() {
            Debug.Assert(_nextArgIndex == _arguments.Length);
            return _arguments;
        }

        // Adds an argument expression that wraps the remaining arguments into an array.
        public void AddUnsplat() {
            Debug.Assert(_hasUnsplatParameter);
            Debug.Assert(_nextArgIndex == _arguments.Length - 1);
            _arguments[_nextArgIndex++] = (_argsToUnsplat != null) ? Methods.MakeArrayOpCall(_argsToUnsplat) : Methods.MakeArray0.OpCall();
        }

        /// <summary>
        /// Fills missing arguments with the missing argument placeholder (RubyOps.DefaultArgument singleton).
        /// </summary>
        public void FillMissingArguments() {
            for (int i = _explicitArgCount; i < _mandatoryParamCount + _optionalParamCount; i++) {
                // TODO: optimize field read?
                _arguments[_nextArgIndex++] = Ast.Field(null, Fields.DefaultArgument);
            }
        }

        public void AddSplatted(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            var arg = args.GetSplattedMetaArgument();

            int listLength;
            ParameterExpression listVariable;
            metaBuilder.AddSplattedArgumentTest((IList)arg.Value, arg.Expression, out listLength, out listVariable);
            if (listLength > 0) {
                for (int i = 0; i < listLength; i++) {
                    Add(
                        Ast.Call(
                            listVariable,
                            typeof(IList).GetMethod("get_Item"),
                            AstUtils.Constant(i)
                        )
                    );
                }
            }
        }

        public void AddCallArguments(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            // simple args:
            for (int i = 0; i < args.SimpleArgumentCount; i++) {
                Add(args.GetSimpleArgumentExpression(i));
            }

            // splat arg:
            if (args.Signature.HasSplattedArgument) {
                AddSplatted(metaBuilder, args);
            }

            // rhs arg:
            if (args.Signature.HasRhsArgument) {
                Add(args.GetRhsArgumentExpression());
            }

            if (HasTooFewArguments) {
                metaBuilder.SetWrongNumberOfArgumentsError(_explicitArgCount, _mandatoryParamCount);
                return;
            }

            if (HasTooManyArguments) {
                metaBuilder.SetWrongNumberOfArgumentsError(_explicitArgCount, _mandatoryParamCount);
                return;
            }

            // add optional placeholders:
            FillMissingArguments();

            if (_hasUnsplatParameter) {
                AddUnsplat();
            }
        }
    }
}
