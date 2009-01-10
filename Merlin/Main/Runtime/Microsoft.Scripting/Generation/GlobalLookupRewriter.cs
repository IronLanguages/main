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

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Generation {

    /// <summary>
    /// Converts globals to late bound lookups on the scope
    /// TODO: move to IronPython
    /// </summary>
    public sealed class GlobalLookupRewriter : GlobalRewriter {
        // Global & top-level locals are in seperate namespaces for named lookup
        private readonly Dictionary<string, GlobalVariableExpression> _globalNames = new Dictionary<string, GlobalVariableExpression>();
        private readonly Dictionary<string, GlobalVariableExpression> _localNames = new Dictionary<string, GlobalVariableExpression>();

        // TODO: internal
        public GlobalLookupRewriter() { }

        protected override Expression RewriteGet(GlobalVariableExpression node) {
            EnsureUniqueName(node);

            return AstUtils.Convert(
                Expression.Call(
                    typeof(ScriptingRuntimeHelpers).GetMethod(node.IsLocal ? "LookupName" : "LookupGlobalName"),
                    new Expression[]{
                        Context,
                        AstUtils.Constant(SymbolTable.StringToId(node.Name))
                    }
                ),
                node.Type
            );
        }

        protected override Expression RewriteSet(AssignmentExtensionExpression node) {
            GlobalVariableExpression lvalue = (GlobalVariableExpression)node.Expression;
            EnsureUniqueName(lvalue);

            return AstUtils.Convert(
                Expression.Call(
                    typeof(ScriptingRuntimeHelpers).GetMethod(lvalue.IsLocal ? "SetName" : "SetGlobalName"),
                    new Expression[]{
                        Context,
                        AstUtils.Constant(SymbolTable.StringToId(lvalue.Name)),
                        Visit(node.Value)
                    }
                ),
                node.Type
            );
        }

        private void EnsureUniqueName(GlobalVariableExpression node) {
            EnsureUniqueName(node.IsLocal ? _localNames : _globalNames, node);
        }
    }
}

namespace Microsoft.Scripting.Runtime {
    // TODO: move these to Microsoft.Scripting
    public static partial class ScriptingRuntimeHelpers {
        /// <summary>
        /// Called from generated code, helper to do a global name lookup
        /// </summary>
        public static object LookupGlobalName(CodeContext context, SymbolId name) {
            return context.LanguageContext.LookupName(context.GlobalScope, name);
        }

        /// <summary>
        /// Called from generated code, helper to do global name assignment
        /// </summary>
        public static object SetGlobalName(CodeContext context, SymbolId name, object value) {
            // TODO: could we get rid of new context creation:
            context.LanguageContext.SetName(context.GlobalScope, name, value);
            return value;
        }

        /// <summary>
        /// Called from generated code, helper to do name lookup
        /// </summary>
        public static object LookupName(CodeContext context, SymbolId name) {
            return context.LanguageContext.LookupName(context.Scope, name);
        }

        /// <summary>
        /// Called from generated code, helper to do name assignment
        /// </summary>
        public static object SetName(CodeContext context, SymbolId name, object value) {
            context.LanguageContext.SetName(context.Scope, name, value);
            return value;
        }

    }
}