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

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {

    // TODO: this class should be abstract
    public class ScopeExtension {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly ScopeExtension[] EmptyArray = new ScopeExtension[0];

        private readonly Scope _scope;

        public Scope Scope {
            get { return _scope; }
        }

        public ScopeExtension(Scope scope) {
            ContractUtils.RequiresNotNull(scope, "scope");
            _scope = scope;
        }
    }
}
