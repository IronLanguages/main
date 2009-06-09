using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using IronRuby.Builtins;

namespace IronRuby.Rack {
    public class Request {

        private readonly Hash headers;
        private readonly string queryString;
        private readonly string body;
        private readonly string scheme;

        internal readonly HttpRequestBase OrigionalRequest;
        
        public Request(HttpRequestBase request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            // http or https
            this.scheme = request.Url.Scheme;

            // go through the headers of the request and move to a more tenable
            // storage location than NameValueCollection
            Dictionary<object, object> headers = new Dictionary<object, object>();
            foreach (string key in request.Headers.AllKeys) {
                string value = request.Headers.Get(key);
                if (string.IsNullOrEmpty(value)) {
                    continue;
                }
                headers.Add(key, value);
            }

            // explicitly put in Content-Type and Content-Length
            headers["Content-Type"] = request.ContentType;
            headers["Content-Length"] = request.ContentLength;
            this.headers = new Hash(headers);

            // recombine the query string into 1 single string
            StringBuilder qs = new StringBuilder();
            foreach (string key in request.QueryString.AllKeys) {
                string value = request.QueryString.Get(key);
                if (string.IsNullOrEmpty(value)) {
                    continue;
                }
                qs.Append(key).Append('=').Append(value).Append('&');
            }

            this.queryString = qs.ToString();

            // was form data posted?
            if (request.ContentType.StartsWith("application/x-www-form-urlencoded") ||
                request.ContentType.StartsWith("multipart/form-data")) {
                // yep, so we need to build a body string that contains the form data so that
                // Rack can do the correct thing.  If it is multipart/form-data, we change it 
                // to x-www-form-urlencoded as that is easier to generate
                if (request.ContentType.StartsWith("multipart/form-data")) {
                    headers["Content-Type"] = "application/x-www-form-urlencoded";
                }
                StringBuilder body = new StringBuilder();
                foreach (string key in request.Form.AllKeys) {
                    string value = request.Form.Get(key);
                    if (string.IsNullOrEmpty(value)) {
                        continue;
                    }
                    qs.Append(key).Append('=').Append(value).Append('&');
                }
                this.body = qs.ToString();
            } else {
                // not form data, not sure what to do about files right now, going to
                // punt and not deal with it right now.  Not sure what do do about other
                // types of body content either.  If text/*, could read it in using the
                // encoding.  Again going to punt for now and not deal with it.
            }

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
        public string Body { get { return this.body; } }

        public string Scheme { get { return this.scheme; } }
    }
}
