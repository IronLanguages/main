using System;
using System.Data;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Runtime;
using Microsoft.Web.Scripting.MembersInjectors;

[assembly: ExtensionType(typeof(DataRowView), typeof(DataRowViewMembersInjector))]
namespace Microsoft.Web.Scripting.MembersInjectors {
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
