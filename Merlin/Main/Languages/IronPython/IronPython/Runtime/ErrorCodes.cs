/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

namespace IronPython.Hosting {
    public static class ErrorCodes {
        // The error flags
        public const int IncompleteMask = 0x000F;

        public const int IncompleteStatement = 0x0001;      // unexpected <eof> found
        public const int IncompleteToken = 0x0002;

        // The actual error values

        public const int ErrorMask = 0x7FFFFFF0;

        public const int SyntaxError = 0x0010;              // general syntax error
        public const int IndentationError = 0x0020;         // invalid intendation
        public const int TabError = 0x0030;                 // invalid tabs
    }
}
