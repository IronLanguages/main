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
using System.Security.Permissions;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;
using System.Collections.Generic;
using System.Runtime.Remoting;

namespace Microsoft.Scripting.Hosting {
    /// <summary>
    /// Provides documentation against live objects for use in a REPL window.
    /// </summary>
    public sealed class DocumentationOperations
#if !SILVERLIGHT
 : MarshalByRefObject
#endif
 {
        private readonly DocumentationProvider _provider;

        internal DocumentationOperations(DocumentationProvider provider) {
            _provider = provider;
        }
        
        /// <summary>
        /// Gets the available members defined on the provided object.
        /// </summary>
        public ICollection<MemberDoc> GetMembers(object value) {
            return _provider.GetMembers(value);
        }

        /// <summary>
        /// Gets the overloads available for the provided object if it is invokable.
        /// </summary>
        public ICollection<OverloadDoc> GetOverloads(object value) {
            return _provider.GetOverloads(value);
        }

#if !SILVERLIGHT
        /// <summary>
        /// Gets the available members on the provided remote object.
        /// </summary>
        public ICollection<MemberDoc> GetMembers(ObjectHandle value) {
            return _provider.GetMembers(value.Unwrap());
        }

        /// <summary>
        /// Gets the overloads available for the provided remote object if it is invokable.
        /// </summary>
        public ICollection<OverloadDoc> GetOverloads(ObjectHandle value) {
            return _provider.GetOverloads(value.Unwrap());
        }

        // TODO: Figure out what is the right lifetime
        public override object InitializeLifetimeService() {
            return null;
        }
#endif
    }
}
