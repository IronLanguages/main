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
using System.Runtime.CompilerServices;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

[assembly: ExtensionType(typeof(WebServiceHelper), typeof(WebServiceHelperAttributesInjector))]

public static class WebServiceHelperAttributesInjector {
    public class CallableLoadHelper {
        string _providerName;

        public CallableLoadHelper(string providerName) {
            _providerName = providerName;
        }

        [SpecialName]
        public object Call(string url) {
            return DynamicWebServiceHelpers.WebService.Load(_providerName, url);
        }
    }

    [SpecialName]
    public static IList<SymbolId> GetMemberNames(WebServiceHelper self) {
        List<string> providerNames = DynamicWebServiceHelpers.WebService.GetProviderNames();
        List<SymbolId> attrNames = new List<SymbolId>(providerNames.Count);

        foreach (string providerName in providerNames) {
            attrNames.Add(SymbolTable.StringToId("Load" + providerName));
        }

        return attrNames;
    }

    [SpecialName]
    public static object GetBoundMember(WebServiceHelper self, string name) {
        if (name.StartsWith("Load", StringComparison.Ordinal)) {
            name = name.Substring("Load".Length);

            if (DynamicWebServiceHelpers.WebService.IsValidProviderName(name)) {
                return new CallableLoadHelper(name);
            }
        }

        return OperationFailed.Value;
    }
}
