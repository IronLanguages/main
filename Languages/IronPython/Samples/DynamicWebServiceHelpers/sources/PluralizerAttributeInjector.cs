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

[assembly: ExtensionType(typeof(String), typeof(PluralizerAttributesInjector))]

public static class PluralizerAttributesInjector {
    public class CallablePluralizerHelper  {
        string _word;
        bool _pluralize;

        public CallablePluralizerHelper(string word, bool pluralize) {
            _word = word;
            _pluralize = pluralize;
        }

        [SpecialName]
        public object Call() {
            if (_pluralize) {
                return DynamicWebServiceHelpers.Pluralizer.ToPlural(_word);
            } else {
                return DynamicWebServiceHelpers.Pluralizer.ToSingular(_word);
            }
        }

        [SpecialName]
        public object Call(int count) {
            if (_pluralize) {
                if (count == 1) {
                    return string.Format("{0} {1}", count, _word);
                } else {
                    return string.Format("{0} {1}", count, DynamicWebServiceHelpers.Pluralizer.ToPlural(_word));
                }
            }
            throw new ArgumentException();
        }
    }

    [SpecialName]
    public static IList<SymbolId> GetMemberNames(string self) {
        List<SymbolId> list = new List<SymbolId>();
        list.Add(SymbolTable.StringToId("ToPlural"));
        list.Add(SymbolTable.StringToId("ToSingular"));
        return list;
    }

    [SpecialName]
    public static object GetBoundMember(string self, string name) {
        switch (name) {
            case "ToPlural":
                return new CallablePluralizerHelper(self, true);
            case "ToSingular":
                return new CallablePluralizerHelper(self, false);
            default:
                return OperationFailed.Value;
        }
    }
}
