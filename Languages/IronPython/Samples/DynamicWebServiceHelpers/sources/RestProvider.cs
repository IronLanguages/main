/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Web;
using Microsoft.Scripting;

public class RestProvider : IWebServiceProvider {
    public class RestService {
        string _url;

        public RestService(string url) {
            _url = url;
        }

        public object Invoke(IAttributesCollection args) {
            Dictionary<object, object> dict = new Dictionary<object, object>();

            foreach (KeyValuePair<object, object> pair in args) {
                dict.Add(pair.Key, pair.Value);
            }

            return Invoke(dict);
        }

        public object Invoke([ParamDictionary] IDictionary<object, object> args) {
            string url = _url;

            // append arguments to query string
            if (args != null && args.Count > 0) {
                StringBuilder sb = new StringBuilder(url);

                bool first = false;

                if (url.IndexOf('?') < 0) {
                    sb.Append('?');
                    first = true;
                }
                else if (url.EndsWith("?")) {
                    first = true;
                }

                foreach (KeyValuePair<object, object> p in (IDictionary<object, object>)args) {
                    if (first) {
                        first = false;
                    }
                    else {
                        sb.Append('&');
                    }

                    sb.Append(HttpUtility.UrlEncode(p.Key.ToString()));
                    sb.Append('=');
                    sb.Append(HttpUtility.UrlEncode(p.Value.ToString()));
                }

                url = sb.ToString();
            }

            // download XML
            byte[] responseBytes = DynamicWebServiceHelpers.GetBytesForUrl(url);
            return DynamicWebServiceHelpers.LoadXmlFromBytes(responseBytes);
        }
    }

    #region IWebServiceProvider Members

    string IWebServiceProvider.Name {
        get { return "Rest"; }
    }

    bool IWebServiceProvider.MatchUrl(string url) {
        return url.EndsWith(".rest", StringComparison.OrdinalIgnoreCase) ||
               url.IndexOf("/rest/", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    object IWebServiceProvider.LoadWebService(string url) {
        return new RestService(url);
    }

    #endregion
}
