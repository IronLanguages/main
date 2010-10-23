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

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

[assembly: ExtensionType(typeof(XmlElement), typeof(XmlElementAttributesInjector))]

public static class XmlElementAttributesInjector {
    [SpecialName]
    public static IList<SymbolId> GetMemberNames(XmlElement self) {
        List<SymbolId> list = new List<SymbolId>();

        foreach (XmlAttribute attr in self.Attributes) {
            SymbolId id = SymbolTable.StringToId(attr.Name);

            if (!list.Contains(id)) {
                list.Add(id);
            }
        }

        for (XmlNode n = self.FirstChild; n != null; n = n.NextSibling) {
            if (n is XmlElement) {
                SymbolId id = SymbolTable.StringToId(n.Name);

                if (!list.Contains(id)) {
                    list.Add(id);
                }
            }
        }

        return list;
    }

    [SpecialName]
    public static object GetCustomMember(XmlElement self, string name) {
        XmlAttribute attr = self.Attributes[name];
        if (attr != null) {
            return attr.Value;
        }

        for (XmlNode n = self.FirstChild; n != null; n = n.NextSibling) {
            if (n is XmlElement && string.CompareOrdinal(n.Name, name) == 0) {
                if (n.HasChildNodes && n.FirstChild == n.LastChild &&
                    n.FirstChild is XmlText) {
                    return n.InnerText;
                }
                else {
                    return n;
                }
            }
        }

        // see if they ask for pluralized element - return array in that case
        string singularName = null;
        List<XmlNode> elementList = null;

        for (XmlNode n = self.FirstChild; n != null; n = n.NextSibling) {
            if (n is XmlElement) {
                if (singularName == null) {
                    if (DynamicWebServiceHelpers.Pluralizer.IsNounPluralOfNoun(name, n.Name)) {
                        singularName = n.Name;
                        elementList = new List<XmlNode>();
                        elementList.Add(n);
                    }
                }
                else if (string.CompareOrdinal(n.Name, singularName) == 0) {
                    elementList.Add(n);
                }
            }
        }

        if (elementList != null) {
            return elementList.ToArray();
        }

        return OperationFailed.Value;
    }
}
