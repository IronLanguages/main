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

namespace Chiron {
    class HttpRequestData {
        IList<KeyValuePair<string, string>> _headers = new List<KeyValuePair<string, string>>();
        IList<byte[]> _body = new List<byte[]>();

        private string _method, _uri;

        public string Method { get { return _method; } set { _method = value; } }
        public string Uri { get { return _uri; } set { _uri = value; } }
        public IList<KeyValuePair<string, string>> Headers { get { return _headers; } }
        public IList<byte[]> Body { get { return _body; } }

        public int BodyLength {
            get {
                int c = 0;
                foreach (byte[] b in Body) c += b.Length;
                return c;
            }
        }
    }
}
