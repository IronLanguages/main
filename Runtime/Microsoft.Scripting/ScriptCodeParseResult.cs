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

namespace Microsoft.Scripting {
    public enum ScriptCodeParseResult {
        /// <summary>
        /// Source code is a syntactically correct.
        /// </summary>
        Complete,

        /// <summary>
        /// Source code represents an empty statement/expression.
        /// </summary>
        Empty,
            
        /// <summary>
        /// Source code is already invalid and no suffix can make it syntactically correct.
        /// </summary>
        Invalid,

        /// <summary>
        /// Last token is incomplete. Source code can still be completed correctly.
        /// </summary>
        IncompleteToken,

        /// <summary>
        /// Last statement is incomplete. Source code can still be completed correctly.
        /// </summary>
        IncompleteStatement,
    }

    // TODO: rename or remove
    public static class SourceCodePropertiesUtils {
        public static bool IsCompleteOrInvalid(/*this*/ ScriptCodeParseResult props, bool allowIncompleteStatement) {
            return
                props == ScriptCodeParseResult.Invalid ||
                props != ScriptCodeParseResult.IncompleteToken &&
                (allowIncompleteStatement || props != ScriptCodeParseResult.IncompleteStatement);
        }
    }
}
