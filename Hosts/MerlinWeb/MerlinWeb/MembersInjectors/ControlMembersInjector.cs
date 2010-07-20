using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Web.Scripting.MembersInjectors;
using Microsoft.Web.Scripting.UI;

[assembly: ExtensionType(typeof(Control), typeof(ControlMembersInjector))]
namespace Microsoft.Web.Scripting.MembersInjectors {
    public static class ControlMembersInjector {
        [SpecialName]
        public static IList<string> GetMemberNames(object self) {

            // Get all the controls under the current control, but within the same naming container
            Control control = self as Control;
            Debug.Assert(control != null);
            List<string> list = new List<string>();
            GetControlIDsRecursive(control.Controls, list);
            return list;
        }

        private static void GetControlIDsRecursive(ControlCollection controls, IList<string> list) {
            foreach (Control c in controls) {
                if (!String.IsNullOrEmpty(c.ID))
                    list.Add(c.ID);

                if (c.HasControls() && !(c is INamingContainer))
                    GetControlIDsRecursive(c.Controls, list);
            }
        }

        // Helper method to get the Master from either a MasterPage or a Page (if any)
        private static ScriptMaster GetMasterFromPageOrMaster(Control control) {
            Page page = control as Page;
            if (page != null)
                return page.Master as ScriptMaster;

            ScriptMaster master = control as ScriptMaster;
            if (master != null)
                return master.Master as ScriptMaster;

            return null;
        }

        [SpecialName]
        public static object GetBoundMember(Control control, string name) {
            return LocateControl(control, name, 0) ?? OperationFailed.Value;
        }

        // Find the control with ID name, taking care of the presence of master pages
        private static object LocateControl(Control control, string name, int depth) {

            // First, we recurse as long as we find master pages, keeping track of the depth
            ScriptMaster master = GetMasterFromPageOrMaster(control);
            if (master != null)
                return LocateControl(master, name, depth + 1);

            // Now find the control by traversing the same number of ContentPlaceHolders
            return LocateControlInContentPlaceHolders(control as ScriptMaster, control, name, depth);
        }

        private static Control LocateControlInContentPlaceHolders(
            ScriptMaster master, Control control, string name, int depth) {

            // If we're back to zero depth, just look for the id'ed control right here
            if (depth == 0) {
                // If it's a special MerlinWeb user control, use the true user control instead
                Microsoft.Web.Scripting.UI.Controls.UserControl uc = control as Microsoft.Web.Scripting.UI.Controls.UserControl;
                if (uc != null) {
                    control = uc.UC;
                    if (control == null)
                        return null;
                }

                return control.FindControl(name);
            }

            // While we search for the control deeper into the tree, we also walk back
            // in the opposite direction to find the master that defines the current ContentPlaceHolder
            ScriptMaster parentMaster = master.Parent as ScriptMaster;

            // Recurse on each ContentPlaceHolder owned by the current master
            foreach (string s in master.ContentPlaceHolders) {
                ContentPlaceHolder cph = (ContentPlaceHolder)control.FindControl(s);
                if (cph == null)
                    continue;

                Control c = LocateControlInContentPlaceHolders(parentMaster, cph, name, depth - 1);
                if (c != null) return c;
            }

            return null;
        }

    }
}
