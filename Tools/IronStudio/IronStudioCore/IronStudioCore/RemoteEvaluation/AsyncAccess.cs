/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Runtime.Remoting;

namespace Microsoft.IronStudio.RemoteEvaluation {
    /// <summary>
    /// Special MarshalByRefObject which is used to communicate we need to abort
    /// the current work item on a 2ndary async communication channel.
    /// </summary>
    class AsyncAccess : MarshalByRefObject {
        private readonly RemoteProxy _proxy;

        public AsyncAccess(RemoteProxy proxy) {
            _proxy = proxy;
        }

        public void Abort() {
            _proxy.Abort();
        }

        public ObjectHandle CommandDispatcher {
            get {
                return _proxy.CommandDispatcher;
            }
            set {
                _proxy.CommandDispatcher = value;
            }
        }
    }
}
