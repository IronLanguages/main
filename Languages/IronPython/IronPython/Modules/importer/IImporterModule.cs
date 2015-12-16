using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronPython.Modules
{
    /// <summary>
    /// Can be inherited for stateful importer modules, such as zipimporter
    /// </summary>
    public interface IImporterModule
    {
        /// <summary>
        /// Current state of the importer
        /// </summary>
        ImporterModuleState State
        {
            get;
            set;
        }
    }
}
