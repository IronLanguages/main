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
using System.Text;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions.Calls {
    public sealed class ApplicableCandidate {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public readonly MethodCandidate Method;
        public readonly ArgumentBinding ArgumentBinding;

        internal ApplicableCandidate(MethodCandidate method, ArgumentBinding argBinding) {
            Assert.NotNull(method, argBinding);
            Method = method;
            ArgumentBinding = argBinding;
        }

        public ParameterWrapper GetParameter(int argumentIndex) {
            return Method.GetParameter(argumentIndex, ArgumentBinding);
        }

        public override string ToString() {
            return Method.ToString();
        }
    }
}
