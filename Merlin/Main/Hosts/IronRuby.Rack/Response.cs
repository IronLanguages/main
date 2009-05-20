using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace IronRuby.Rack {
    public class Response {
        internal readonly HttpResponseBase OrigionalResponse;

        /// <summary>
        /// Initializes a new instance of the HttpResponseAdapter class.
        /// </summary>
        /// <param name="response">The HttpResponseBase to adapt</param>
        public Response(HttpResponseBase response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            this.OrigionalResponse = response;
        }

        /// <summary>
        /// Gets or sets the status associated with this HTTP response
        /// </summary>
        public int Status
        {
            get { return this.OrigionalResponse.StatusCode; }
            set { this.OrigionalResponse.StatusCode = value; }
        }

        /// <summary>
        /// Appends a header to the HTTP response
        /// </summary>
        /// <param name="key">The HTTP header name</param>
        /// <param name="value">The HTTP header value</param>
        public void AppendHeader(string key, string value)
        {
            this.OrigionalResponse.AppendHeader(key, value);
        }

        /// <summary>
        /// Writes a chunk of data to the HTTP response
        /// </summary>
        /// <param name="data">The data to write</param>
        public void Write(string data)
        {
            this.OrigionalResponse.Output.Write(data);
        }
    }
}