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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System.Dynamic;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    public abstract class ExtensionUnaryOperationBinder : UnaryOperationBinder {
        private readonly string _operation;

        protected ExtensionUnaryOperationBinder(string operation)
            : base(ExpressionType.Extension) {
            ContractUtils.RequiresNotNull(operation, "operation");
            _operation = operation;
        }

        public string ExtensionOperation {
            get {
                return _operation;
            }
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ _operation.GetHashCode();
        }

        public override bool Equals(object obj) {
            var euob = obj as ExtensionUnaryOperationBinder;
            return euob != null && base.Equals(obj) && _operation == euob._operation;
        }
    }
}
