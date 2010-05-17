using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using IronRuby.Builtins;
using Microsoft.Scripting.Utils;

namespace IronRuby.Rack {
    public class Request {

        private readonly Hash headers;
        private readonly string queryString;
        private readonly RubyIO body;
        private readonly string scheme;

        internal readonly HttpRequestBase OrigionalRequest;
        
        public Request(HttpRequestBase request) {
            ContractUtils.RequiresNotNull(request, "request");

            // http or https
            this.scheme = request.Url.Scheme;

            // move headers to a Ruby Hash
            this.headers = new Hash(RubyEngine.Context);
            foreach (string key in request.Headers.AllKeys) {
                string value = request.Headers.Get(key);
                if (string.IsNullOrEmpty(value)) continue;
                headers.Add(key, value);
            }

            this.queryString = request.QueryString.ToString();

            this.body = new RubyIO(RubyEngine.Context, request.InputStream, IOMode.ReadOnly);

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
