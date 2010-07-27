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
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Web;
using System.Web.UI;

namespace Microsoft.Scripting.AspNet.UI.Controls {

    internal class UserControlControlBuilder : ControlBuilder {
        private static PropertyInfo s_linePropertyInfo;
        private int _line = -1;

        static UserControlControlBuilder() {
            try {
                // Get the PropertyInfo for the ControlBuilder.Line property, which is internal
                // REVIEW: can we avoid calling this internal property?
                s_linePropertyInfo = typeof(ControlBuilder).GetProperty("Line", BindingFlags.NonPublic | BindingFlags.Instance);
            } catch { }
        }

        public override void Init(TemplateParser parser, ControlBuilder parentBuilder, Type type, string tagName, string id, System.Collections.IDictionary attribs) {
            base.Init(parser, parentBuilder, type, tagName, id, attribs);

            // Try to get the line number in the page where the user control is declared
            if (s_linePropertyInfo != null) {
                try {
                    _line = (int)s_linePropertyInfo.GetValue(this, null);
                } catch { }
            }
        }

        public override object BuildObject() {
            UserControl uc = (UserControl) base.BuildObject();

            // Give the line number to the control for error handling purpose
            uc.Line = _line;

            return uc;
        }
    }


    // Simple UserControl wrapper control.  It is needed because NoCompile pages are not allowed
    // to use NoCompile user controls using the standard ASP.NEt static UC syntax
    [ControlBuilder(typeof(UserControlControlBuilder))]
    public class UserControl: Control, IAttributeAccessor {
        private string _virtualPath;
        private Dictionary<string, string> _attributes;
        private int _line;
        private Control _uc;

        public string VirtualPath {
            get { return _virtualPath; }
            set { _virtualPath = value; }
        }

        internal int Line {
            get { return _line; }
            set { _line = value; }
        }

        internal Control UC {
            get { return _uc; }
        }

        private void ThrowException(string message, Exception inner) {
            // If we got a line number, throw a nicer ParseException
            if (_line >= 0)
                throw new HttpParseException(message, inner, TemplateControl.AppRelativeVirtualPath, null, _line);

            // Otherwise, just throw a simple exception
            throw new Exception(message, inner);
        }

        protected override void OnInit(EventArgs e) {
            base.OnInit(e);

            // Fail if we didn't get a virtual path
            if (String.IsNullOrEmpty(_virtualPath)) {
                ThrowException("The UserControl tag must have a 'VirtualPath' attribute", null);
            }

            try {
                _uc = TemplateControl.LoadControl(_virtualPath);
            } catch (Exception exception) {
                ThrowException(null, exception);
            }

            // Give our ID to the created UserControl, and change ours to avoid conflict
            string id = ID;
            ID = "__" + id;
            _uc.ID = id;

            // Handle expando attributes by forwarding them to the created control
            IScriptTemplateControl scriptUC = _uc as IScriptTemplateControl;
            if (scriptUC != null) {

                if (_attributes != null) {
                    foreach (KeyValuePair<string, string> entry in _attributes) {
                        try {
                            scriptUC.ScriptTemplateControl.SetProperty(entry.Key, entry.Value);
                        } catch (Exception exception) {
                            ThrowException(null, exception);
                        }
                    }
                }
            }

            Controls.Clear();
            Controls.Add(_uc);
        }

        #region IAttributeAccessor Members
        public virtual string GetAttribute(string key) {
            if (_attributes == null)
                return null;

            return _attributes[key];
        }

        public virtual void SetAttribute(string key, string value) {
            if (_attributes == null)
                _attributes = new Dictionary<string, string>();

            _attributes.Add(key, value);
        }
        #endregion
    }
}
