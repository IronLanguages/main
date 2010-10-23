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

using System.IO;
using System.Net;
using System.Xml;

public static class DynamicWebServiceHelpers {
    static WebServiceHelper _webServiceHelper;
    static PluralizerHelper _pluralizerHelper;
    static SimpleXmlHelper _simpleXmlHelper;

    static DynamicWebServiceHelpers() {
        _webServiceHelper = new WebServiceHelper();
        _pluralizerHelper = new PluralizerHelper();
        _simpleXmlHelper = new SimpleXmlHelper();
    }

    public static WebServiceHelper WebService {
        get { return _webServiceHelper; }
    }

    public static PluralizerHelper Pluralizer {
        get { return _pluralizerHelper; }
    }

    public static SimpleXmlHelper SimpleXml {
        get { return _simpleXmlHelper; }
    }

    #region Misc Internal Helper Methods

    internal static byte[] GetBytesForUrl(string url) {
        return new WebClient().DownloadData(url);
    }

    internal static string GetStringForUrl(string url) {
        return new WebClient().DownloadString(url);
    }

    internal static XmlElement LoadXmlFromBytes(byte[] xml) {
        XmlDocument doc = new XmlDocument();
        doc.Load(new MemoryStream(xml));
        return doc.DocumentElement;
    }

    internal static XmlElement LoadXmlFromString(string text) {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(text);
        return doc.DocumentElement;
    }

    #endregion
}
