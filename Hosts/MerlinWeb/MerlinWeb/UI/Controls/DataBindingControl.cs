using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.UI;
using Microsoft.Web.Scripting.Util;

namespace Microsoft.Web.Scripting.UI.Controls {
    // This control handles <%# ... %> expressions used as control attribute values
    public class DataBindingControl : BaseCodeControl {
        private string _targetId;
        private string _attributeName;
        private PropertyInfo _propInfo;
        private IAttributeAccessor _attributeAccessor;

        public string TargetId {
            get { return _targetId; }
            set { _targetId = value; }
        }

        public string AttributeName {
            get { return _attributeName; }
            set { _attributeName = value; }
        }

        protected override void OnInit(EventArgs e) {
            Control targetControl = FindControl(_targetId);

            _propInfo = targetControl.GetType().GetProperty(_attributeName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (_propInfo == null) {
                // Couldn't find a real property. Try IAttributeAccessor
                _attributeAccessor = targetControl as IAttributeAccessor;
                if (_attributeAccessor == null) {
                    Misc.ThrowException("Control '" + _targetId +
                            "' doesn't have a property named '" + _attributeName + "'",
                        null, TemplateControl.AppRelativeVirtualPath, Line);
                }
            }

            targetControl.DataBinding += new EventHandler(ControlOnDataBinding);
        }

        private void ControlOnDataBinding(object sender, EventArgs e) {
            ScriptTemplateControl scriptTemplateControl = ScriptTemplateControl.GetScriptTemplateControl(this);

            
            object evaluatedCode = scriptTemplateControl.EvaluateDataBindingExpression(
                this, Code.Trim(), Line);
            
            // Don't perform the assignment if we got back null/DBNull
            if (evaluatedCode != null && evaluatedCode != DBNull.Value) {
                if (_propInfo != null) {
                    // Convert the value to a string if needed
                    // TODO: more generic type conversion logic?
                    if (_propInfo.PropertyType == typeof(string) && !(evaluatedCode is string)) {
                        evaluatedCode = evaluatedCode.ToString();
                    }

                    _propInfo.SetValue(sender, evaluatedCode, null);
                } else {
                    _attributeAccessor.SetAttribute(AttributeName, evaluatedCode.ToString());
                }
            }
        }
    }
}
