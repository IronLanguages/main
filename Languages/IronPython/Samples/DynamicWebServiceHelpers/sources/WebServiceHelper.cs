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
using System.Net;
using System.Text;

public class WebServiceHelper {
    #region Provider Registration

    IWebServiceProvider[] _providers = new IWebServiceProvider[0];
    object _providersLock = new object();

    #endregion

    internal WebServiceHelper() {
        RegisterProvider(new WsdlProvider());
        RegisterProvider(new RestProvider());
        RegisterProvider(new RssProvider());
    }

    #region public APIs

    public void RegisterProvider(IWebServiceProvider provider) {
        lock (_providersLock) {
            List<IWebServiceProvider> l = new List<IWebServiceProvider>(_providers);
            l.Add(provider);
            _providers = l.ToArray();
        }
    }

    public object Load(string url) {
        IWebServiceProvider[] providers = _providers;

        foreach (IWebServiceProvider p in providers) {
            if (p.MatchUrl(url)) {
                return p.LoadWebService(url);
            }
        }

        throw new InvalidOperationException(
            string.Format("Web Service Provider not found for '{0}'", url));
    }

    public object Load(string providerName, string url) {
        IWebServiceProvider p = GetProvider(providerName);

        if (p != null) {
            return p.LoadWebService(url);
        }

        throw new InvalidOperationException(
            string.Format("Web Service Provider '{0}' not found", providerName));
    }

    #endregion

    #region Helpers

    IWebServiceProvider GetProvider(string providerName) {
        IWebServiceProvider[] providers = _providers;

        foreach (IWebServiceProvider p in providers) {
            if (string.Compare(providerName, p.Name, StringComparison.OrdinalIgnoreCase) == 0) {
                return p;
            }
        }

        return null;
    }

    internal bool IsValidProviderName(string providerName) {
        return (GetProvider(providerName) != null);
    }

    internal List<string> GetProviderNames() {
        IWebServiceProvider[] providers = _providers;
        List<string> list = new List<string>();

        foreach (IWebServiceProvider p in providers) {
            list.Add(p.Name);
        }

        return list;
    }

    #endregion
}
