using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.PyAnalysis {
    public interface IAnalysisVariable {
        /// <summary>
        /// Returns the location of where the variable is defined.
        /// </summary>
        LocationInfo Location {
            get;
        }

        VariableType Type {
            get;
        }
    }

    public enum VariableType {
        None,
        /// <summary>
        /// A parameter to a function definition or assignment to a member or global.
        /// </summary>
        Definition,

        /// <summary>
        /// A read from a global, local, member variable.
        /// </summary>
        Reference,

        /// <summary>
        /// A reference to a value which is passed into a parameter.
        /// </summary>
        Value
    }

    class AnalysisVariable : IAnalysisVariable {
        private readonly LocationInfo _loc;
        private readonly VariableType _type;

        public AnalysisVariable(VariableType type, LocationInfo location) {
            _loc = location;
            _type = type;
        }

        #region IAnalysisVariable Members

        public LocationInfo Location {
            get { return _loc; }
        }

        public VariableType Type {
            get { return _type; }
        }

        #endregion
    }


}
