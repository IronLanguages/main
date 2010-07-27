/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Data;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.AspNet.MembersInjectors;

[assembly: ExtensionType(typeof(DataRowView), typeof(DataRowViewMembersInjector))]
namespace Microsoft.Scripting.AspNet.MembersInjectors {
    public static class DataRowViewMembersInjector {
        [SpecialName]
        public static object GetBoundMember(DataRowView dataRowView, string name) {
            if (dataRowView.Row.Table.Columns.Contains(name)) {
                object value = dataRowView[name];

                // If the value is DBNull.Value, return String.Empty instead to avoid confusing the
                // injector mechanism, which treats DBNull.Value as 'name not found'
                // REVIEW: clean this up
                return (value != DBNull.Value) ? value : String.Empty;
            }
            return OperationFailed.Value;    
        }
    }
}
