using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Scripting.Debugging {

    #region Generated Exception Factory

    // *** BEGIN GENERATED CODE ***

    /// <summary>
    ///    Strongly-typed and parameterized string factory.
    /// </summary>

    internal static partial class ErrorStrings {
        internal static string JumpNotAllowedInNonLeafFrames {
            get {
                return "Frame location can only be changed in leaf frames";
            }
        }

        internal static string DebugContextAlreadyConnectedToTracePipeline {
            get {
                return "Cannot create TracePipeline because DebugContext is already connected to another TracePipeline";
            }
        }

        internal static string InvalidSourceSpan {
            get {
                return "Invalid SourceSpan";
            }
        }

        internal static string SetNextStatementOnlyAllowedInsideTraceback {
            get {
                return "Unable to perform SetNextStatement because current thread is not inside a traceback";
            }
        }

        internal static string ITracePipelineClosed {
            get {
                return "ITracePipeline cannot be used because it has been closed";
            }
        }

        internal static string InvalidFunctionVersion {
            get {
                return "Frame cannot be remapped to function verion {0} because it does not exist";
            }
        }

        internal static string DebugInfoWithoutSymbolDocumentInfo {
            get {
                return "Unable to transform LambdaExpression because DebugInfoExpression #{0} did not have a valid SymbolDocumentInfo";
            }
        }
    }

    #endregion
}
