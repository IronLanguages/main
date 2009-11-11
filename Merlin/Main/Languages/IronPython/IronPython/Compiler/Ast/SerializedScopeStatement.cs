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
using System.Collections.Generic;
using System.Text;

using Microsoft.Scripting;

using IronPython.Runtime;

#if !CLR2
using MSAst = System.Linq.Expressions;
#else
using MSAst = Microsoft.Scripting.Ast;
#endif

namespace IronPython.Compiler.Ast {
    /// <summary>
    /// Fake ScopeStatement for FunctionCode's to hold on to after we have deserialized pre-compiled code.
    /// </summary>
    class SerializedScopeStatement : ScopeStatement {
        private readonly string _name;
        private readonly string _filename;
        private readonly FunctionAttributes _flags;
        private readonly string[] _parameterNames;

        internal SerializedScopeStatement(string name, string[] argNames, FunctionAttributes flags, SourceSpan span, string path, string[] freeVars, string[] names, string[] cellVars, string[] varNames) {
            _name = name;
            _filename = path;
            _flags = flags;
            this.SetLoc(span.Start, span.End);
            _parameterNames = argNames;
            if (freeVars != null) {
                foreach (string freeVar in freeVars) {
                    AddFreeVariable(new PythonVariable(freeVar, VariableKind.Local, this), false);
                }
            }
            if (names != null) {
                foreach (string globalName in names) {
                    AddGlobalVariable(new PythonVariable(globalName, VariableKind.Global, this));
                }
            }
            if (varNames != null) {
                foreach (string variable in varNames) {
                    EnsureVariable(variable);
                }
            }
            if (cellVars != null) {
                foreach (string cellVar in cellVars) {
                    AddCellVariable(new PythonVariable(cellVar, VariableKind.Local, this));
                }
            }
        }

        internal override MSAst.LambdaExpression GetLambda() {
            throw new InvalidOperationException();
        }

        internal override bool ExposesLocalVariable(PythonVariable variable) {
            throw new InvalidOperationException();
        }

        internal override PythonVariable BindReference(PythonNameBinder binder, PythonReference reference) {
            throw new InvalidOperationException();
        }

        public override void Walk(PythonWalker walker) {
            throw new InvalidOperationException();
        }

        public override string Name {
            get {
                return _name;
            }
        }

        internal override string Filename {
            get {
                return _filename;
            }
        }

        internal override FunctionAttributes Flags {
            get {
                return _flags;
            }
        }

        internal override string[] ParameterNames {
            get {
                return _parameterNames;
            }
        }

        internal override int ArgCount {
            get {
                return _parameterNames.Length;
            }
        }
    }
}
