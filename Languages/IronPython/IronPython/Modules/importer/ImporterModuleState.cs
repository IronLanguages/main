using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronPython.Modules
{
    /// <summary>
    /// Represents a state whether modules can be imported with a given module or not
    /// </summary>
    public enum ImporterModuleState
    {
        /// <summary>
        /// Is ready to be used for importing
        /// </summary>
        Ready = 0,

        /// <summary>
        /// Will be ignored when trying to import data
        /// </summary>
        Ignore = 1
    }
}
