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

using System.Dynamic;
using Microsoft.Contracts;
using Microsoft.Scripting.Utils;
using System;

namespace Microsoft.Scripting.Actions {
    [Obsolete("Use ExtensionBinaryOperationBinder or ExtensionUnaryOperationBinder")]
    public abstract class OperationBinder : DynamicMetaObjectBinder {
        private string _operation;

        protected OperationBinder(string operation) {
            _operation = operation;
        }

        public string Operation {
            get {
                return _operation;
            }
        }

        public DynamicMetaObject FallbackOperation(DynamicMetaObject target, DynamicMetaObject[] args) {
            return FallbackOperation(target, args, null);
        }

        public abstract DynamicMetaObject FallbackOperation(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion);

        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNullItems(args, "args");

            // Try to call BindOperation
            var emo = target as OperationMetaObject;
            if (emo != null) {
                return emo.BindOperation(this, args);
            }

            // Otherwise just fall back (it's as if they didn't override BindOperation)
            return FallbackOperation(target, args);
        }

        [Confined]
        public override bool Equals(object obj) {
            OperationBinder oa = obj as OperationBinder;
            return oa != null && oa._operation == _operation;
        }

        [Confined]
        public override int GetHashCode() {
            return 0x10000000 ^ _operation.GetHashCode();
        }
    }
}
