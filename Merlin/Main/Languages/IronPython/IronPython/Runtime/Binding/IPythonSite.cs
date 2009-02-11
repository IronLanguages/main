using System;
using System.Collections.Generic;
using System.Text;

namespace IronPython.Runtime.Binding {
    interface IPythonSite {
        BinderState/*!*/ Binder {
            get;
        }
    }
}
