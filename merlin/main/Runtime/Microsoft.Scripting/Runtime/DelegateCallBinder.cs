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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// Call site binder used by the DelegateSignatureInfo to call IDynamicObject
    /// </summary>
    class DelegateCallBinder : CallSiteBinder {
        private readonly int _args;

        internal DelegateCallBinder(int args) {
            _args = args;
        }

        public override int GetHashCode() {
            return _args ^ 31321;
        }

        public override bool Equals(object obj) {
            DelegateCallBinder dcb = obj as DelegateCallBinder;
            return dcb != null && dcb._args == _args;
        }

        public override object CacheIdentity {
            get { return this; }
        }

        private static CodeContext ExtractCodeContext(ref object[] args) {
            CodeContext cc = null;
            if (args.Length > 0 && (cc = args[0] as CodeContext) != null) {
                args = ArrayUtils.ShiftLeft(args, 1);
            }
            return cc;
        }

        public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
            ContractUtils.RequiresNotNull(args, "args");
            CodeContext cc = ExtractCodeContext(ref args);
            ContractUtils.Requires(args.Length > 0);
            IOldDynamicObject ido = args[0] as IOldDynamicObject;
            ContractUtils.RequiresNotNull(ido, "args");

            OldCallAction ca = OldCallAction.Make(cc.LanguageContext.Binder, _args);
            var builder = new RuleBuilder(parameters, returnLabel);
            if (!ido.GetRule(ca, cc, args, builder)) {
                throw new InvalidOperationException("Cannot perform call.");
            }

            return builder.CreateRule();
        }
    }
}
