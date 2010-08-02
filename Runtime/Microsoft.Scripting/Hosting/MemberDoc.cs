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
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting {
    /// <summary>
    /// Provides documentation about a member in a live object.
    /// </summary>
    [Serializable]
    public class MemberDoc {
        private readonly string _name;
        private readonly MemberKind _kind;

        public MemberDoc(string name, MemberKind kind) {
            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.Requires(kind >= MemberKind.None && kind <= MemberKind.Namespace, "kind");

            _name = name;
            _kind = kind;
        }

        /// <summary>
        /// The name of the member
        /// </summary>
        public string Name {
            get {
                return _name;
            }
        }

        /// <summary>
        /// The kind of the member if it's known.
        /// </summary>
        public MemberKind Kind {
            get {
                return _kind;
            }
        }
    }

}
