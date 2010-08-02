/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Web;
using IronRuby.Builtins;
using Microsoft.Scripting.Utils;

namespace IronRubyRack {

    public class AspNetRequest {

        private readonly Hash headers;
        private readonly string queryString;
        private readonly RubyIO body;
        private readonly string scheme;

        internal readonly HttpRequest OrigionalRequest;
        
        public AspNetRequest(HttpRequest request) {
            ContractUtils.RequiresNotNull(request, "request");

            // http or https
            this.scheme = request.Url.Scheme;

            // move headers to a Ruby Hash
            this.headers = new Hash(IronRubyEngine.Context);
            foreach (string key in request.Headers.AllKeys) {
                string value = request.Headers.Get(key);
                if (string.IsNullOrEmpty(value)) continue;
                headers.Add(key, value);
            }

            this.queryString = request.QueryString.ToString();

            this.body = new RubyIO(IronRubyEngine.Context, request.InputStream, IOMode.ReadOnly);

            // Save the origional request incase it's needed.
            OrigionalRequest = request;
        }

        /// <summary>
        /// Gets the headers of this HTTP request
        /// </summary>
        public Hash Headers { get { return this.headers; } }

        /// <summary>
        /// Gets the query string of this HTTP request
        /// </summary>
        public string QueryString { get { return this.queryString; } }

        /// <summary>
        /// Gets the body of this HTTP request
        /// </summary>
        public RubyIO Body { get { return this.body; } }

        public string Scheme { get { return this.scheme; } }
    }
}
