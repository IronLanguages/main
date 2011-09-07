using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.IronStudio.Core {
    public static class DlrPredefinedClassificationTypeNames {
        /// <summary>
        /// Open grouping classification.  Used for (, [, etc...  A subtype of the pre-defined
        /// operator grouping.
        /// </summary>
        public const string OpenGrouping = "open grouping";
        /// <summary>
        /// Closed grouping classification.  Used for ), ], etc...  A subtype of the pre-defined
        /// operator grouping.
        /// </summary>
        public const string CloseGrouping = "close grouping";

        /// <summary>
        /// Classification used for comma characters when used outside of a literal, comment, etc...
        /// </summary>
        public const string Comma = "comma";

        /// <summary>
        /// Classification used for . characters when used outside of a literal, comment, etc...
        /// </summary>
        public const string Dot = "dot";

        /// <summary>
        /// Instead of using PredefinedClassificationTypeNames.Operator for our operators, use "script operator" instead, as "operator" seems to
        /// always come out as 0x008080
        /// </summary>
        public const string ScriptOperator = "script operator";
    }
}
