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
using Microsoft.Scripting.Utils;
using Microsoft.Contracts;

namespace Microsoft.Scripting.Actions {
    public class OldCreateInstanceAction : OldCallAction, IEquatable<OldCreateInstanceAction> {
        protected OldCreateInstanceAction(ActionBinder binder, CallSignature callSignature)
            : base(binder, callSignature) {
        }

        public static new OldCreateInstanceAction Make(ActionBinder binder, CallSignature signature) {
            return new OldCreateInstanceAction(binder, signature);
        }

        public static new OldCreateInstanceAction Make(ActionBinder binder, int argumentCount) {
            ContractUtils.Requires(argumentCount >= 0, "argumentCount");
            return new OldCreateInstanceAction(binder, new CallSignature(argumentCount));
        }

        [Confined]
        public override bool Equals(object obj) {
            return Equals(obj as OldCreateInstanceAction);
        }

        [StateIndependent]
        public bool Equals(OldCreateInstanceAction other) {
            return base.Equals(other);
        }

        [Confined]
        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override DynamicActionKind Kind {
            get {
                return DynamicActionKind.CreateInstance;
            }
        }
    }
}
